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

    // -------------------------------------------------------------
    // Mesh + material cache: one entry per tile pattern
    // -------------------------------------------------------------
    private struct TileKey : IEquatable<TileKey>
    {
        public readonly PavingPattern Pattern;

        public TileKey(PavingPattern pattern)
        {
            Pattern = pattern;
        }

        public bool Equals(TileKey other) => Pattern == other.Pattern;
        public override bool Equals(object obj) => obj is TileKey other && Equals(other);
        public override int GetHashCode() => (int)Pattern;
    }


    private class TileCacheEntry
    {
        public Mesh Mesh;
        public Material Material;
    }

    private static readonly Dictionary<TileKey, TileCacheEntry> _cache =
        new Dictionary<TileKey, TileCacheEntry>();

    // -------------------------------------------------------------

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

    private static TileCacheEntry GetOrCreateCachedEntry(TilePrefabRegistry registry, TileInfo t)
    {
        var key = new TileKey(t.PavingPattern);

        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var prefab = registry.GetPrefab(t.PavingPattern, Rotation.R0, t.Biome);

        if (prefab == null)
        {
            Debug.LogError($"Missing prefab for pattern={t.PavingPattern}");
            return null;
        }

        // Instantiate ONCE, extract mesh/material, then destroy
        var temp = Object.Instantiate(prefab);
        temp.hideFlags = HideFlags.HideAndDontSave;

        var mf = temp.GetComponentInChildren<MeshFilter>();
        var mr = temp.GetComponentInChildren<MeshRenderer>();

#if UNITY_EDITOR
        Object.DestroyImmediate(temp);
#else
        Object.Destroy(temp);
#endif

        Mesh mesh = mf != null ? mf.sharedMesh : TileMeshFactory.QuadTile(TILE_SIZE);
        Material mat = mr != null ? mr.sharedMaterial : registry.DefaultMaterial;

        cached = new TileCacheEntry
        {
            Mesh = mesh,
            Material = mat
        };

        _cache[key] = cached;
        return cached;
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
        var materialBuckets = new Dictionary<Material, List<CombineInstance>>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var t = tiles[x, y];
                var entry = GetOrCreateCachedEntry(registry, t);

                if (entry == null)
                    continue;

                Vector3 localPos = new Vector3(
                    x * TILE_SIZE,
                    t.ElevationLevel,
                    y * TILE_SIZE);

                Quaternion rot = Quaternion.Euler(0, RotationDegrees(t.Rotation), 0);

                CombineInstance ci = new CombineInstance
                {
                    mesh = entry.Mesh,
                    transform = Matrix4x4.TRS(localPos, rot, Vector3.one)
                };

                if (!materialBuckets.TryGetValue(entry.Material, out var list))
                {
                    list = new List<CombineInstance>();
                    materialBuckets[entry.Material] = list;
                }

                list.Add(ci);
            }
        }

        // Merge per-material
        List<CombineInstance> submeshCombiners = new List<CombineInstance>();
        List<Material> finalMaterials = new List<Material>();

        foreach (var kv in materialBuckets)
        {
            Material mat = kv.Key;
            List<CombineInstance> instances = kv.Value;

            Mesh submesh = new Mesh();
            submesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            submesh.CombineMeshes(instances.ToArray(), true, true);

            submeshCombiners.Add(new CombineInstance
            {
                mesh = submesh,
                transform = Matrix4x4.identity
            });

            finalMaterials.Add(mat);
        }

        // Final mesh (1 submesh per material)
        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        if (submeshCombiners.Count > 0)
            finalMesh.CombineMeshes(submeshCombiners.ToArray(), false, true);

        // Create chunk GameObject
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
}
