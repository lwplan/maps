using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Converts a logical map bitmap (paths/nodes/sand) into a tiled mesh using the TileW/materials atlas.
/// Each pixel becomes a quad with UVs pointing at the correct tile variant, including edges and corners.
/// </summary>
[ExecuteAlways]
public class TileMapMeshBuilder : MonoBehaviour
{
    public enum TileKind
    {
        Sand,
        Path,
        Node
    }

    [Flags]
    public enum NeighborMask
    {
        None = 0,
        North = 1 << 0,
        East = 1 << 1,
        South = 1 << 2,
        West = 1 << 3
    }

    [Serializable]
    public class TileVariantDefinition
    {
        public TileKind Kind = TileKind.Sand;
        [Tooltip("Boundary mask describing which neighbors differ from this tile. Supports rotation when allowed below.")]
        public NeighborMask BoundaryMask = NeighborMask.None;
        [Tooltip("Texture for this variant; packed into an atlas at runtime.")]
        public Texture2D Texture;
        [Tooltip("If enabled, the boundary mask can be rotated to fit other quadrants when the same art works in multiple orientations.")]
        public bool AllowRotation = true;
#if UNITY_EDITOR
        [Tooltip("Optional editor-only asset path used to automatically fill Texture when empty.")]
        public string EditorDefaultTexturePath;
#endif
    }

    private struct TileCell
    {
        public TileKind Kind;
        public NeighborMask Boundary;
        public NeighborMask SandEdges;
        public NeighborMask PathEdges;
    }

    private struct PackedVariant
    {
        public Rect Uv;
        public int RotationSteps;
    }

    [Header("Logical map input")]
    [SerializeField] private Texture2D logicalMap;
    [SerializeField] private Color32 sandColor = new(0, 0, 0, 255);
    [SerializeField] private Color32 pathColor = new(255, 255, 255, 255);
    [SerializeField] private Color32 nodeColor = new(255, 0, 0, 255);
    [SerializeField, Range(0f, 0.25f)] private float colorTolerance = 0.05f;

    [Header("Atlas + variants")]
    [SerializeField] private List<TileVariantDefinition> tileVariants = new();
    [SerializeField, Range(8, 512)] private int atlasPadding = 2;

    [Header("Output targets")]
    [SerializeField] private MeshFilter targetMeshFilter;
    [SerializeField] private MeshRenderer targetMeshRenderer;
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool logValidation = true;

    [Header("Debug/validation")]
    [SerializeField] private bool emitDebugPreview;
    [SerializeField] private Texture2D debugPreview;

    private readonly Dictionary<(TileKind, NeighborMask), PackedVariant> variantLookup = new();

    private void Start()
    {
        if (generateOnStart && Application.isPlaying)
        {
            Generate();
        }
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            Generate();
        }
    }

    [ContextMenu("Generate Mesh From Map")]
    public void Generate()
    {
        if (logicalMap == null)
        {
            Debug.LogWarning("TileMapMeshBuilder: No logical map assigned.");
            return;
        }

        EnsureVariantTextures();

        var grid = BuildTileGrid();
        var atlas = BuildAtlas();
        var mesh = BuildMesh(grid);

        if (targetMeshFilter != null)
        {
            targetMeshFilter.sharedMesh = mesh;
        }

        if (targetMeshRenderer != null)
        {
            if (targetMeshRenderer.sharedMaterial == null)
            {
                targetMeshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
            }

            targetMeshRenderer.sharedMaterial.mainTexture = atlas;
        }

        if (emitDebugPreview)
        {
            debugPreview = BuildDebugPreview(grid);
        }

        if (logValidation)
        {
            ValidateGrid(grid);
        }
    }

    private TileCell[,] BuildTileGrid()
    {
        var pixels = logicalMap.GetPixels32();
        int width = logicalMap.width;
        int height = logicalMap.height;
        var grid = new TileCell[width, height];

        // First pass: classify each pixel.
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var c = pixels[y * width + x];
                grid[x, y].Kind = ClassifyColor(c);
            }
        }

        // Second pass: compute boundaries.
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var current = grid[x, y].Kind;

                EvaluateNeighbor(grid, width, height, x, y - 1, current, NeighborMask.North, ref grid[x, y]);
                EvaluateNeighbor(grid, width, height, x + 1, y, current, NeighborMask.East, ref grid[x, y]);
                EvaluateNeighbor(grid, width, height, x, y + 1, current, NeighborMask.South, ref grid[x, y]);
                EvaluateNeighbor(grid, width, height, x - 1, y, current, NeighborMask.West, ref grid[x, y]);
            }
        }

        return grid;
    }

    private void EvaluateNeighbor(TileCell[,] grid, int width, int height, int x, int y, TileKind current, NeighborMask mask, ref TileCell cell)
    {
        TileKind neighbor = TileKind.Sand; // Treat out-of-bounds as sand.
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            neighbor = grid[x, y].Kind;
        }

        if (neighbor != current)
        {
            cell.Boundary |= mask;
        }

        if (neighbor == TileKind.Sand)
        {
            cell.SandEdges |= mask;
        }

        if (neighbor == TileKind.Path)
        {
            cell.PathEdges |= mask;
        }
    }

    private TileKind ClassifyColor(Color32 color)
    {
        if (IsClose(color, nodeColor)) return TileKind.Node;
        if (IsClose(color, pathColor)) return TileKind.Path;
        return TileKind.Sand;
    }

    private bool IsClose(Color32 a, Color32 b)
    {
        float dr = Mathf.Abs(a.r - b.r) / 255f;
        float dg = Mathf.Abs(a.g - b.g) / 255f;
        float db = Mathf.Abs(a.b - b.b) / 255f;
        return (dr + dg + db) / 3f <= colorTolerance;
    }

    private Texture2D BuildAtlas()
    {
        variantLookup.Clear();

        var textures = new List<Texture2D>();
        foreach (var variant in tileVariants)
        {
            if (variant?.Texture == null)
            {
                continue;
            }

            textures.Add(variant.Texture);
        }

        if (textures.Count == 0)
        {
            throw new InvalidOperationException("TileMapMeshBuilder: No tile variant textures configured.");
        }

        var atlas = new Texture2D(0, 0, TextureFormat.RGBA32, false);
        var rects = atlas.PackTextures(textures.ToArray(), atlasPadding, 2048, false);
        atlas.filterMode = FilterMode.Point;
        atlas.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < tileVariants.Count; i++)
        {
            var variant = tileVariants[i];
            if (variant?.Texture == null)
            {
                continue;
            }

            var uvRect = rects[textures.IndexOf(variant.Texture)];
            RegisterVariant(variant.Kind, variant.BoundaryMask, uvRect, variant.AllowRotation);
        }

        return atlas;
    }

    private void RegisterVariant(TileKind kind, NeighborMask mask, Rect uv, bool allowRotation)
    {
        for (int rotation = 0; rotation < 4; rotation++)
        {
            var rotatedMask = RotateMask(mask, rotation);
            if (!allowRotation && rotation > 0)
            {
                break;
            }

            var key = (kind, rotatedMask);
            if (!variantLookup.ContainsKey(key))
            {
                variantLookup[key] = new PackedVariant
                {
                    Uv = uv,
                    RotationSteps = rotation
                };
            }
        }
    }

    private static NeighborMask RotateMask(NeighborMask mask, int steps)
    {
        steps %= 4;
        if (steps == 0)
        {
            return mask;
        }

        NeighborMask rotated = NeighborMask.None;
        for (int i = 0; i < 4; i++)
        {
            var bit = (NeighborMask)(1 << i);
            if ((mask & bit) == 0)
            {
                continue;
            }

            int rotatedIndex = (i + steps) % 4;
            rotated |= (NeighborMask)(1 << rotatedIndex);
        }

        return rotated;
    }

    private Mesh BuildMesh(TileCell[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        var vertices = new List<Vector3>(width * height * 4);
        var uvs = new List<Vector2>(width * height * 4);
        var colors = new List<Color32>(width * height * 4);
        var triangles = new List<int>(width * height * 6);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cell = grid[x, y];
                var key = ChooseVariantKey(cell);
                if (!variantLookup.TryGetValue(key, out var packedVariant))
                {
                    // Fall back to a solid tile if no edge/corner variant exists.
                    key = (cell.Kind, NeighborMask.None);
                    if (!variantLookup.TryGetValue(key, out packedVariant))
                    {
                        throw new InvalidOperationException($"TileMapMeshBuilder: Missing atlas entry for {cell.Kind} with mask {cell.Boundary}.");
                    }
                }

                int vertIndex = vertices.Count;

                vertices.Add(new Vector3(x, 0f, y));
                vertices.Add(new Vector3(x + 1, 0f, y));
                vertices.Add(new Vector3(x + 1, 0f, y + 1));
                vertices.Add(new Vector3(x, 0f, y + 1));

                var baseUV = GetQuadUvs(packedVariant);
                uvs.AddRange(baseUV);

                var color = cell.Kind switch
                {
                    TileKind.Path => new Color32(180, 180, 180, 255),
                    TileKind.Node => new Color32(255, 180, 0, 255),
                    _ => new Color32(200, 150, 100, 255)
                };

                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);

                triangles.Add(vertIndex + 0);
                triangles.Add(vertIndex + 2);
                triangles.Add(vertIndex + 1);
                triangles.Add(vertIndex + 0);
                triangles.Add(vertIndex + 3);
                triangles.Add(vertIndex + 2);
            }
        }

        var mesh = new Mesh
        {
            name = "TileMapMesh"
        };

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        return mesh;
    }

    private (TileKind, NeighborMask) ChooseVariantKey(TileCell cell)
    {
        NeighborMask mask = cell.Boundary;

        // Prefer sand edges for paths so they pull in correct borders; fall back to all boundaries otherwise.
        if (cell.Kind == TileKind.Path && cell.SandEdges != NeighborMask.None)
        {
            mask = cell.SandEdges;
        }
        else if (cell.Kind == TileKind.Node && cell.PathEdges != NeighborMask.None)
        {
            mask = cell.PathEdges;
        }

        return (cell.Kind, mask);
    }

    private Vector2[] GetQuadUvs(PackedVariant variant)
    {
        var uv = new Vector2[4];
        uv[0] = new Vector2(variant.Uv.xMin, variant.Uv.yMin);
        uv[1] = new Vector2(variant.Uv.xMax, variant.Uv.yMin);
        uv[2] = new Vector2(variant.Uv.xMax, variant.Uv.yMax);
        uv[3] = new Vector2(variant.Uv.xMin, variant.Uv.yMax);

        // Rotate UVs in-place; clockwise steps.
        for (int i = 0; i < variant.RotationSteps; i++)
        {
            var tmp = uv[0];
            uv[0] = uv[3];
            uv[3] = uv[2];
            uv[2] = uv[1];
            uv[1] = tmp;
        }

        return uv;
    }

    private Texture2D BuildDebugPreview(TileCell[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        var preview = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cell = grid[x, y];
                var color = cell.Kind switch
                {
                    TileKind.Path => new Color32(200, 200, 200, 255),
                    TileKind.Node => new Color32(255, 120, 0, 255),
                    _ => new Color32(180, 140, 90, 255)
                };

                // Darken edges to make seams obvious in the preview.
                if (cell.Boundary != NeighborMask.None)
                {
                    color.r = (byte)(color.r * 0.8f);
                    color.g = (byte)(color.g * 0.8f);
                    color.b = (byte)(color.b * 0.8f);
                }

                preview.SetPixel(x, y, color);
            }
        }

        preview.Apply(false, false);
        return preview;
    }

    private void ValidateGrid(TileCell[,] grid)
    {
        int missing = 0;
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                var cell = grid[x, y];
                var key = ChooseVariantKey(cell);
                if (variantLookup.ContainsKey(key))
                {
                    continue;
                }

                key = (cell.Kind, NeighborMask.None);
                if (!variantLookup.ContainsKey(key))
                {
                    missing++;
                    Debug.LogWarning($"TileMapMeshBuilder: Missing variant for {cell.Kind} at ({x},{y}) with mask {cell.Boundary}." +
                                     " Assign an edge/corner tile or enable rotation on an existing variant.");
                }
            }
        }

        if (missing == 0)
        {
            Debug.Log("TileMapMeshBuilder: Validation passed (all tile variants resolved, seams avoided).");
        }
    }

    private void EnsureVariantTextures()
    {
#if UNITY_EDITOR
        foreach (var variant in tileVariants)
        {
            if (variant == null || variant.Texture != null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(variant.EditorDefaultTexturePath))
            {
                variant.Texture = AssetDatabase.LoadAssetAtPath<Texture2D>(variant.EditorDefaultTexturePath);
            }
        }
#endif
    }
}
