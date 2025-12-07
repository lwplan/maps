using maps.Map3D;
using UnityEngine;

namespace Runtime
{
    public class TileChunkBuilder
    {
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
                                registry.GetPavingPrefab(t.PavingPattern, t.Rotation);

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
    }
}