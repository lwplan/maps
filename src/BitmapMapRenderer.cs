// Conditional compilation ensures the bitmap renderer only participates in builds
// that explicitly opt in (e.g., the TestBitmap CLI app).
#if TESTBITMAP_APP
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace maps
{
    public static class BitmapMapRenderer
    {
        public const int PxPerTile = 3;
        public const int Margin = 20;
        public const int ArenaSizeTiles = 9;   // Full arena size in tiles
        public const int HalfArena = ArenaSizeTiles / 2;

        private static readonly Dictionary<NodeType, Color> NodeColors = new()
        {
            { NodeType.Start, Color.Yellow },
            { NodeType.End, Color.Magenta },
            { NodeType.Combat, Color.Red },
            { NodeType.Trading, Color.Cyan },
            { NodeType.Event, Color.Blue },
            { NodeType.Powerup, Color.Green },
        };

        private static readonly Dictionary<Biome, Color> BiomeColors = new()
        {
            { Biome.None, Color.Black },
            { Biome.Town, Color.Brown },
            { Biome.Battlement, Color.DarkSlateGray },
            { Biome.Dunes, Color.SandyBrown },
            { Biome.Canyon, Color.Peru },
            { Biome.Mountain, Color.Gray },
            { Biome.Sea, Color.DarkBlue }
        };

        public static Image<Rgba32> Render(GameMap map)
        {
            var nodes = map.Nodes;
            if (nodes.Count == 0)
                return new Image<Rgba32>(32, 32);

            var biomes = map.Biomes;
            if (biomes == null)
                return new Image<Rgba32>(32, 32);

            //
            // --- Compute rendering bounds ---
            //
            int minX = nodes.Min(n => n.TileX) - HalfArena - 2;
            int maxX = nodes.Max(n => n.TileX) + HalfArena + 2;
            int minY = nodes.Min(n => n.TileY) - HalfArena - 2;
            int maxY = nodes.Max(n => n.TileY) + HalfArena + 2;

            int widthPx  = (maxX - minX + 1) * PxPerTile + Margin * 2;
            int heightPx = (maxY - minY + 1) * PxPerTile + Margin * 2;

            var img = new Image<Rgba32>(widthPx, heightPx);
            var pen = new SolidPen(Color.White, 1);

            img.Mutate(ctx =>
            {
                ctx.Fill(Color.Black);

                //
                // --- 1. Draw biome background tiles ---
                //
                for (int x = 0; x < biomes.Width; x++)
                for (int y = 0; y < biomes.Height; y++)
                {
                    Biome b = biomes[x, y];
                    if (b == Biome.None)
                        continue;

                    var px = (x + biomes.OffsetX - minX) * PxPerTile + Margin;
                    var py = (y + biomes.OffsetY - minY) * PxPerTile + Margin;

                    if (px < 0 || py < 0 || px >= widthPx || py >= heightPx)
                        continue;

                    IPath tile = new RectangularPolygon(px, py, PxPerTile, PxPerTile);
                    ctx.Fill(BiomeColors[b], tile);
                }

                //
                // --- 2. Draw routes ---
                //
                foreach (var from in nodes)
                {
                    foreach (var to in from.NextLevelNodes)
                    {
                        int midX = (from.TileX + to.TileX) / 2;

                        var pA    = TileToPx(from.TileX, from.TileY, minX, minY);
                        var pMidH = TileToPx(midX, from.TileY, minX, minY);
                        var pMidV = TileToPx(midX, to.TileY, minX, minY);
                        var pB    = TileToPx(to.TileX, to.TileY, minX, minY);

                        var pb = new PathBuilder();
                        pb.AddLine(pA, pMidH);
                        pb.AddLine(pMidH, pMidV);
                        pb.AddLine(pMidV, pB);

                        ctx.Draw(pen, pb.Build());
                    }
                }

                //
                // --- 3. Draw arenas on top ---
                //
                foreach (var node in nodes)
                {
                    var color = NodeColors.TryGetValue(node.Type, out var c) ? c : Color.Gray;

                    int left  = node.TileX - HalfArena;
                    int top   = node.TileY - HalfArena;

                    var p = TileToPx(left, top, minX, minY);

                    ctx.Fill(
                        color,
                        new RectangularPolygon(
                            p.X,
                            p.Y,
                            ArenaSizeTiles * PxPerTile,
                            ArenaSizeTiles * PxPerTile
                        )
                    );
                }
            });

            return img;
        }

        private static PointF TileToPx(int tx, int ty, int minX, int minY)
        {
            return new PointF(
                (tx - minX) * PxPerTile + Margin,
                (ty - minY) * PxPerTile + Margin
            );
        }
    }
}
#endif
