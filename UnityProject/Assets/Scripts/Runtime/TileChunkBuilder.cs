using System;
using System.Collections.Generic;
using UnityEngine;
using maps;
using maps.Map3D;
using Runtime;
using Object = UnityEngine.Object;

public static class TileChunkBuilder
{
    public const int CHUNK_SIZE = ChunkBuilder.DefaultChunkSize;
    public const float TILE_SIZE = 2f;

    public static GameObject BuildChunks(TileInfo[,] tiles, TilePrefabRegistry registry)
    {
        int width  = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        var root = new GameObject("TileChunks");

        int chunksX = Mathf.CeilToInt(width  / (float)CHUNK_SIZE);
        int chunksY = Mathf.CeilToInt(height / (float)CHUNK_SIZE);

        for (int cx = 0; cx < chunksX; cx++)
        {
            for (int cy = 0; cy < chunksY; cy++)
            {
                BuildChunk(cx, cy, tiles, registry, root.transform);
            }
        }

        return root;
    }

    public static GameObject BuildChunk(
        BuiltChunk builtChunk,
        TilePrefabRegistry registry,
        Transform parent,
        int mapOffsetX,
        int mapOffsetY,
        int chunkSize = CHUNK_SIZE)
    {
        if (builtChunk.Tiles == null || builtChunk.Tiles.Length == 0)
            return null;

        return BuildChunk(
            builtChunk.ChunkX,
            builtChunk.ChunkY,
            builtChunk.Tiles,
            registry,
            parent,
            chunkSize,
            mapOffsetX,
            mapOffsetY);
    }

    public static GameObject BuildChunk(
        int chunkX,
        int chunkY,
        TileInfo[,] tiles,
        TilePrefabRegistry registry,
        Transform parent,
        int chunkSize = CHUNK_SIZE,
        int mapOffsetX = 0,
        int mapOffsetY = 0)
    {
        int chunkStartTileX = chunkX * chunkSize;
        int chunkStartTileY = chunkY * chunkSize;

        int startTileX = Mathf.Max(chunkStartTileX, mapOffsetX);
        int startTileY = Mathf.Max(chunkStartTileY, mapOffsetY);

        int localStartX = startTileX - mapOffsetX;
        int localStartY = startTileY - mapOffsetY;

        return BuildChunkInternal(
            chunkX,
            chunkY,
            localStartX,
            localStartY,
            tiles,
            registry,
            parent);
    }

    private static GameObject BuildChunkInternal(
        int chunkX,
        int chunkY,
        int localStartX,
        int localStartY,
        TileInfo[,] tiles,
        TilePrefabRegistry registry,
        Transform parent)
    {
        int width  = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        // MATERIAL → LIST OF COMBINEINSTANCES
        Dictionary<Material, List<CombineInstance>> materialBuckets =
            new Dictionary<Material, List<CombineInstance>>();

        //
        // Collect tile meshes into material buckets
        //
//
// Collect tile meshes into material buckets
//
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var t = tiles[x, y];

                // Attempt to load the prefab
                var prefab = registry.GetPrefab(t.PavingPattern, t.Rotation, t.Biome);

                if (prefab == null)
                {
                    ReportMissingTile(chunkX, chunkY, x, y, t, registry);
                    prefab = TileMeshFactory.DebugCube(TILE_SIZE);
                }

                // Create a temporary instance to fetch mesh + material
                var temp = GameObject.Instantiate(prefab);
                temp.hideFlags = HideFlags.HideAndDontSave;

                var meshFilter   = temp.GetComponentInChildren<MeshFilter>();
                var meshRenderer = temp.GetComponentInChildren<MeshRenderer>();

                Mesh sourceMesh = meshFilter != null ? meshFilter.sharedMesh : null;

                // fallback to a quad if mesh unreadable
                if (sourceMesh == null || !sourceMesh.isReadable || sourceMesh.triangles.Length == 0)
                    sourceMesh = TileMeshFactory.QuadTile(TILE_SIZE);

                Vector3 localPos = new Vector3(
                    x * TILE_SIZE,
                    t.ElevationLevel,
                    y * TILE_SIZE);

                CombineInstance ci = new CombineInstance
                {
                    mesh = sourceMesh,
                    transform = Matrix4x4.TRS(
                        localPos,
                        Quaternion.Euler(0, RotationDegrees(t.Rotation), 0),
                        Vector3.one)
                };

                Material mat =
                    meshRenderer != null && meshRenderer.sharedMaterial != null
                        ? meshRenderer.sharedMaterial
                        : registry.DefaultMaterial;

                if (!materialBuckets.TryGetValue(mat, out var list))
                {
                    list = new List<CombineInstance>();
                    materialBuckets[mat] = list;
                }

                list.Add(ci);

#if UNITY_EDITOR
                Object.DestroyImmediate(temp);
#else
        Object.Destroy(temp);
#endif
            }
        }


        //
        // Build final mesh from material buckets
        //
        List<CombineInstance> submeshCombiners = new List<CombineInstance>();
        List<Material> finalMaterials          = new List<Material>();

        foreach (var kv in materialBuckets)
        {
            Material mat = kv.Key;
            List<CombineInstance> instances = kv.Value;

            Mesh submesh = new Mesh();
            submesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            // merge within same material
            submesh.CombineMeshes(instances.ToArray(), true, true);

            CombineInstance ci = new CombineInstance
            {
                mesh = submesh,
                transform = Matrix4x4.identity
            };

            submeshCombiners.Add(ci);
            finalMaterials.Add(mat);
        }

        //
        // final combine: one submesh per material
        //
        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        if (submeshCombiners.Count > 0)
        {
            finalMesh.CombineMeshes(submeshCombiners.ToArray(), false, true);
        }

        //
        // Create chunk GameObject
        //
        var chunkGO = new GameObject($"Chunk_{chunkX}_{chunkY}");
        chunkGO.transform.SetParent(parent, false);

        float worldX = localStartX * TILE_SIZE;
        float worldZ = localStartY * TILE_SIZE;

        chunkGO.transform.localPosition = new Vector3(worldX, 0, worldZ);

        var mf = chunkGO.AddComponent<MeshFilter>();
        var mr = chunkGO.AddComponent<MeshRenderer>();

        mf.sharedMesh = finalMesh;
        mr.sharedMaterials = finalMaterials.ToArray();

        chunkGO.isStatic = true;

        return chunkGO;
    }

    private static float RotationDegrees(Rotation r)
    {
        return r switch
        {
            Rotation.R0   => 0f,
            Rotation.R90  => 90f,
            Rotation.R180 => 180f,
            Rotation.R270 => 270f,
            _ => 0f
        };
        
        
    }
    private static void ReportMissingTile(
        int chunkX, int chunkY,
        int tileX, int tileY,
        TileInfo tile,
        TilePrefabRegistry registry)
    {
        // Avoid repeated spam
        string key = $"{tile.PavingPattern}_{tile.Rotation}_{tile.Biome}";
        if (_reportedMissing.Contains(key))
            return;

        _reportedMissing.Add(key);

        string ascii = MaskToAscii(tile.PavingMask8);
        string bits  = Convert.ToString((int)tile.PavingMask8, 2).PadLeft(8, '0');

        Debug.LogError(
            $"[MISSING TILE PREFAB]\n" +
            $"Pattern:   {tile.PavingPattern}\n" +
            $"Rotation:  {tile.Rotation}\n" +
            $"Biome:     {tile.Biome}\n" +
            $"Chunk:     ({chunkX},{chunkY})\n" +
            $"Tile:      ({tileX},{tileY})\n" +
            $"8-mask:    {bits} ({(int)tile.PavingMask8})\n" +
            $"ASCII:\n{ascii}\n" +
            $"Prefab was requested but not found in TilePrefabRegistry.");
    }
    private static HashSet<string> _reportedMissing = new HashSet<string>();

    private static string MaskToAscii(Neighbor8 mask)
    {
        char C(bool b) => b ? '#' : '.';

        bool N  = mask.Has(Neighbor8.North);
        bool NE = mask.Has(Neighbor8.NorthEast);
        bool E  = mask.Has(Neighbor8.East);
        bool SE = mask.Has(Neighbor8.SouthEast);
        bool S  = mask.Has(Neighbor8.South);
        bool SW = mask.Has(Neighbor8.SouthWest);
        bool W  = mask.Has(Neighbor8.West);
        bool NW = mask.Has(Neighbor8.NorthWest);

        return
            $"{C(NW)}{C(N)}{C(NE)}\n" +
            $"{C(W)}.{C(E)}\n" +
            $"{C(SW)}{C(S)}{C(SE)}";
    }

}
