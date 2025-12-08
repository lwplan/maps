using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using maps.Map3D;
using UnityEngine;

namespace Runtime
{
    public class TileChunkBuilder
    {
        private const int DefaultMainThreadBatchSize = 200;

        private readonly struct TilePlacement
        {
            public readonly int X;
            public readonly int Y;
            public readonly PavingPattern Pattern;
            public readonly Rotation Rotation;

            public TilePlacement(int x, int y, PavingPattern pattern, Rotation rotation)
            {
                X = x;
                Y = y;
                Pattern = pattern;
                Rotation = rotation;
            }
        }

        private readonly struct ChunkBuildData
        {
            public readonly int Cx;
            public readonly int Cy;
            public readonly List<TilePlacement> Tiles;

            public ChunkBuildData(int cx, int cy, List<TilePlacement> tiles)
            {
                Cx = cx;
                Cy = cy;
                Tiles = tiles;
            }
        }

        public static GameObject BuildChunks(
            TileInfo[,] tiles,
            TilePrefabRegistry registry,
            float tileSize = 2f,
            int chunkSize = 20)
        {
            int w = tiles.GetLength(0);
            int h = tiles.GetLength(1);

            GameObject root = new GameObject("TileMap3D");

            for (int cx = 0; cx < w; cx += chunkSize)
            {
                for (int cy = 0; cy < h; cy += chunkSize)
                {
                    GameObject chunk = new GameObject($"Chunk_{cx}_{cy}");
                    chunk.transform.parent = root.transform;

                    for (int x = cx; x < Mathf.Min(cx + chunkSize, w); x++)
                    {
                        for (int y = cy; y < Mathf.Min(cy + chunkSize, h); y++)
                        {
                            TileInfo t = tiles[x, y];

                            GameObject prefab =
                                registry.GetPrefab(t.PavingPattern, t.Rotation, BiomeType.Canyon);

                            Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);
                            GameObject inst = Object.Instantiate(prefab, pos, Quaternion.Euler(0, (int)t.Rotation * 90, 0), chunk.transform);
                        }
                    }

                    // Static batch
                    StaticBatchingUtility.Combine(chunk);
                }
            }

            return root;
        }

        public static async Task<GameObject> BuildChunksAsync(
            TileInfo[,] tiles,
            TilePrefabRegistry registry,
            float tileSize = 2f,
            int chunkSize = 20,
            int mainThreadBatchSize = DefaultMainThreadBatchSize)
        {
            var chunkBuildData = await Task.Run(() => PrepareChunkData(tiles, chunkSize));

            GameObject root = new GameObject("TileMap3D");

            foreach (var chunkData in chunkBuildData)
            {
                GameObject chunk = new GameObject($"Chunk_{chunkData.Cx}_{chunkData.Cy}");
                chunk.transform.parent = root.transform;

                int builtTiles = 0;

                foreach (var tile in chunkData.Tiles)
                {
                    GameObject prefab = registry.GetPrefab(tile.Pattern, tile.Rotation, BiomeType.Canyon);
                    Vector3 pos = new Vector3(tile.X * tileSize, 0, tile.Y * tileSize);
                    Object.Instantiate(
                        prefab,
                        pos,
                        Quaternion.Euler(0, (int)tile.Rotation * 90, 0),
                        chunk.transform);

                    builtTiles++;
                    if (builtTiles % mainThreadBatchSize == 0)
                        await Task.Yield();
                }

                StaticBatchingUtility.Combine(chunk);
                await Task.Yield();
            }

            return root;
        }

        private static IReadOnlyList<ChunkBuildData> PrepareChunkData(TileInfo[,] tiles, int chunkSize)
        {
            int w = tiles.GetLength(0);
            int h = tiles.GetLength(1);

            var chunkCoordinates = new List<(int cx, int cy)>();
            for (int cx = 0; cx < w; cx += chunkSize)
                for (int cy = 0; cy < h; cy += chunkSize)
                    chunkCoordinates.Add((cx, cy));

            var chunks = new ConcurrentBag<ChunkBuildData>();

            Parallel.ForEach(chunkCoordinates, coord =>
            {
                var placements = new List<TilePlacement>();
                for (int x = coord.cx; x < Mathf.Min(coord.cx + chunkSize, w); x++)
                {
                    for (int y = coord.cy; y < Mathf.Min(coord.cy + chunkSize, h); y++)
                    {
                        TileInfo t = tiles[x, y];
                        placements.Add(new TilePlacement(x, y, t.PavingPattern, t.Rotation));
                    }
                }

                chunks.Add(new ChunkBuildData(coord.cx, coord.cy, placements));
            });

            return chunks
                .OrderBy(c => c.Cx)
                .ThenBy(c => c.Cy)
                .ToList();
        }
    }
}