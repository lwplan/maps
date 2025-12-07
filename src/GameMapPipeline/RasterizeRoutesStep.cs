using System;
using System.Collections.Generic;
using System.Linq;

namespace maps.GameMapPipeline
{
    public class RasterizeRoutesStep : IMapGenStep
    {
        public void Execute(GameMap map, MapGenParams p)
        {
            int w = map.TileWidth;
            int h = map.TileHeight;

            bool[,] isPath = new bool[w, h];

            int ox = map.OffsetX;
            int oy = map.OffsetY;

            // Group nodes by level
            var grouped = map.Nodes
                .GroupBy(n => n.Level)
                .OrderBy(g => g.Key)
                .ToList();

            // Connect each level to the next
            for (int i = 0; i < grouped.Count - 1; i++)
            {
                foreach (var a in grouped[i])
                foreach (var b in grouped[i + 1])
                {
                    DrawManhattan(a, b, isPath, ox, oy);
                }
            }

            map.PathMask = isPath;
        }

        private void DrawManhattan(Node a, Node b, bool[,] grid, int offsetX, int offsetY)
        {
            int ax = a.TileX - offsetX;
            int ay = a.TileY - offsetY;
            int bx = b.TileX - offsetX;
            int by = b.TileY - offsetY;

            // Horizontal leg
            if (ax != bx)
            {
                int stepX = Math.Sign(bx - ax);
                int x = ax;

                while (x != bx)
                {
                    grid[x, ay] = true;
                    x += stepX;
                }
            }

            // Vertical leg
            if (ay != by)
            {
                int stepY = Math.Sign(by - ay);
                int y = ay;

                while (y != by)
                {
                    grid[bx, y] = true;
                    y += stepY;
                }
            }

            // Endpoint
            grid[bx, by] = true;
        }
    }
}