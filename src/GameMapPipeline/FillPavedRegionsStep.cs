using System.Collections.Generic;
using System.Linq;

namespace maps.GameMapPipeline
{
    public class FillPavedRegionsStep : IMapGenStep
    {
        public void Execute(GameMap map, MapGenParams p)
        {
            // Build paved mask same size as biome map
            int width = map.TileWidth;
            int height = map.TileHeight;
            
            bool[,] paved = new bool[width, height];
            bool[,] eventMask = new bool[width, height];

            // Stamp minimal paved region for paths
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (map.PathMask[x, y])
                    paved[x, y] = true;

            // Stamp event node areas
            foreach (var node in map.Nodes)
            {
                int cx = node.TileX - map.Biomes.OffsetX;
                int cy = node.TileY - map.Biomes.OffsetY;

                StampDisk(paved, eventMask, cx, cy, radius: 9);
            }

            map.PavedMask = paved;
            map.EventMask = eventMask;
        }

        private void StampDisk(bool[,] paved, bool[,] eventMask, int cx, int cy, int radius)
        {
            int w = paved.GetLength(0);
            int h = paved.GetLength(1);
            int r2 = radius * radius;

            for (int x = cx - radius; x <= cx + radius; x++)
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                if (x < 0 || y < 0 || x >= w || y >= h) continue;

                if ((x - cx)*(x - cx) + (y - cy)*(y - cy) <= r2)
                {
                    paved[x, y] = true;
                    eventMask[x, y] = true;
                }
            }
        }
    }
}