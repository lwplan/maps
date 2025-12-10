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
    // Cache key (Pattern + Rotation + AtlasIndex)
    // -------------------------------------------------------------
    private struct TileKey : IEquatable<TileKey>
    {
        public readonly PavingPattern Pattern;
        public readonly Rotation Rotation;
        public readonly int AtlasIndex;

        public TileKey(PavingPattern pattern, Rotation rotation, int atlasIndex)
        {
            Pattern = pattern;
            Rotation = rotation;
            AtlasIndex = atlasIndex;
        }

        public bool Equals(TileKey other) =>
            Pattern == other.Pattern &&
            Rotation == other.Rotation &&
            AtlasIndex == other.AtlasIndex;

        public override bool Equals(object obj) =>
            obj is TileKey other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine((int)Pattern, (int)Rotation, AtlasIndex);
    }

    private class TileCacheEntry
    {
        public Mesh Mesh;
    }

    private static readonly Dictionary<TileKey, TileCacheEntry> _cache =
        new Dictionary<TileKey, TileCacheEntry>();

    // -------------------------------------------------------------
    // Public API — unchanged externally
    // -------------------------------------------------------------
    public static GameObject BuildChunks(TileInfo[,] tiles, TilePrefabRegistry registry)
    {
        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        var root = new GameObject("TileChunks");

        int chunksX = Mathf.CeilToInt(width / (float)CHUNK_SIZE);
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

    // -------------------------------------------------------------
    // UV-remapped mesh retrieval + caching
    // -------------------------------------------------------------
    private static TileCacheEntry GetOrCreateCachedEntry(TilePrefabRegistry registry, TileInfo t)
    {
        int atlasIndex = registry.GetAtlasIndex(t.PavingPattern, t.Biome);
        var key = new TileKey(t.PavingPattern, t.Rotation, atlasIndex);

        if (_cache.TryGetValue(key, out var cached))
            return cached;

        // Get base mesh (orientation ignored here)
        var prefab = registry.GetPrefab(t.PavingPattern, Rotation.R0, t.Biome);

        Mesh baseMesh = prefab != null
            ? prefab.GetComponentInChildren<MeshFilter>().sharedMesh
            : TileMeshFactory.QuadTile(TILE_SIZE);

        // Clone and remap UVs
        Mesh remapped = MeshUVTools.CreateUVRemappedCopy(
            baseMesh,
            atlasIndex,
            (int)t.Rotation
        );

        cached = new TileCacheEntry { Mesh = remapped };
        _cache[key] = cached;

        return cached;
    }

    // -------------------------------------------------------------
    // Chunk builder
    // -------------------------------------------------------------
    private static GameObject BuildChunkInternal(
        int chunkX,
        int chunkY,
        int localStartX,
        int localStartY,
        TileInfo[,] tiles,
        TilePrefabRegistry registry,
        Transform parent)
    {
        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        List<CombineInstance> combineInstances = new List<CombineInstance>(width * height);
        Mesh defaultMesh = TileMeshFactory.QuadTile(TILE_SIZE);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileInfo t = tiles[x, y];
                var entry = GetOrCreateCachedEntry(registry, t);

                Mesh mesh = entry?.Mesh ?? defaultMesh;

                Vector3 pos = new Vector3(
                    x * TILE_SIZE,
                    0,
                    y * TILE_SIZE
                );

                // Quaternion rot = Quaternion.Euler(0, RotationDegrees(t.Rotation), 0);
                Quaternion rot = Quaternion.Euler(0, 0, 0);
                combineInstances.Add(new CombineInstance
                {
                    mesh = mesh,
                    transform = Matrix4x4.TRS(pos, rot, Vector3.one)
                });
            }
        }

        // Final combined mesh
        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        finalMesh.CombineMeshes(combineInstances.ToArray(), true, true);
        if (combineInstances.Count != height * width)
        {
            Debug.LogError("Mesh count is " + combineInstances.Count);
        }
        var chunkGO = new GameObject($"Chunk_{chunkX}_{chunkY}");
        chunkGO.transform.SetParent(parent, false);

        chunkGO.transform.localPosition = new Vector3(
            localStartX * TILE_SIZE,
            0,
            localStartY * TILE_SIZE
        );

        var mf = chunkGO.AddComponent<MeshFilter>();
        var mr = chunkGO.AddComponent<MeshRenderer>();

        mf.sharedMesh = finalMesh;
        mr.sharedMaterial = registry.DefaultMaterial;

        chunkGO.isStatic = true;
        return chunkGO;
    }

    private static float RotationDegrees(Rotation r) =>
        r switch
        {
            Rotation.R0 => 0f,
            Rotation.R90 => 90f,
            Rotation.R180 => 180f,
            Rotation.R270 => 270f,
            _ => 0f
        };
}
