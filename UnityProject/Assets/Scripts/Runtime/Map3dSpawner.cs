using UnityEngine;
using maps;
using maps.Unity;
using System.Collections.Generic;

public class Map3DSpawner : MonoBehaviour
{
    public TilePrefabRegistry Registry;
    public UnityMapGenParams Parameters;
    public TileMap2DRenderer PreviewRenderer;

    private GameObject worldRoot;
    private GameMap activeMap;
    private readonly HashSet<(int chunkX, int chunkY)> builtChunks = new();

    public void Generate()
    {
        Cleanup();

        var map = MapGenerator.Generate(Parameters.ToMapGenParams());
        activeMap = map;

        // optional 2D preview
        PreviewRenderer?.Render(map.TileInfo);

        // root container
        worldRoot = new GameObject("TileChunks");
        worldRoot.transform.SetParent(this.transform);

        builtChunks.Clear();
        RequestAllChunks(map);
        ProcessBuiltChunks();
    }

    private void Start()
    {
        Generate();
    }

    private void Update()
    {
        ProcessBuiltChunks();
    }

    private void RequestAllChunks(GameMap map)
    {
        if (map?.ChunkBuilder == null)
            return;

        int chunkSize = ChunkBuilder.DefaultChunkSize;

        int minChunkX = ChunkBuilder.GetChunkCoordForTile(map.OffsetX, chunkSize);
        int maxChunkX = ChunkBuilder.GetChunkCoordForTile(map.OffsetX + map.TileWidth - 1, chunkSize);
        int minChunkY = ChunkBuilder.GetChunkCoordForTile(map.OffsetY, chunkSize);
        int maxChunkY = ChunkBuilder.GetChunkCoordForTile(map.OffsetY + map.TileHeight - 1, chunkSize);

        for (int cx = minChunkX; cx <= maxChunkX; cx++)
        {
            for (int cy = minChunkY; cy <= maxChunkY; cy++)
            {
                map.ChunkBuilder.RequestChunk(cx, cy);
            }
        }
    }

    private void ProcessBuiltChunks()
    {
        if (activeMap?.ChunkBuilder == null || Registry == null || worldRoot == null)
            return;

        while (activeMap.ChunkBuilder.TryDequeueBuiltChunk(out var built))
        {
            if (built.Tiles == null || built.Tiles.Length == 0)
                continue;

            var coord = (built.ChunkX, built.ChunkY);
            if (builtChunks.Contains(coord))
                continue;

            var chunkGo = TileChunkBuilder.BuildChunk(
                built, //   <Assembly-CSharp>\Assets\Scripts\Runtime\Map3dSpawner.cs:2275 Argument type 'maps.BuiltChunk' is not assignable to parameter type 'int'
                Registry,
                worldRoot.transform,
                activeMap.OffsetX,
                activeMap.OffsetY,
                ChunkBuilder.DefaultChunkSize);

            if (chunkGo != null)
                builtChunks.Add(coord);
        }
    }

    private void Cleanup()
    {
        if (worldRoot != null)
        {
            DestroyImmediate(worldRoot);
            worldRoot = null;
        }

        if (activeMap?.ChunkBuilder != null)
        {
            activeMap.ChunkBuilder.Cancel();
            activeMap.ChunkBuilder.Dispose();
        }

        activeMap = null;
        builtChunks.Clear();
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}