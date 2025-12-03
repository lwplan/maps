using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;

namespace maps
{
    /// <summary>
    /// Renders a collection of <see cref="Node"/>s as a monochrome bitmap.
    /// Each node is rendered as a 3×3 pixel solid block, coloured according to its type.
    /// Edges are drawn as 1‑pixel wide lines between node centres.
    /// </summary>
    public static class BitmapMapRenderer
    {
        private const int BlockSize = 3;          // pixels per ASCII "pixel"
        private const int MarginBlocks = 1;       // one block margin around the map

        /// <summary>
        /// Map node types to colours for the bitmap output.
        /// </summary>
        private static readonly Dictionary<NodeType, Color> NodeColors = new()
        {
            { NodeType.Start,   Color.Yellow },
            { NodeType.End,     Color.Magenta },
            { NodeType.Combat,  Color.Red },
            { NodeType.Trading, Color.Cyan },
            { NodeType.Event,   Color.Blue },
            { NodeType.Powerup, Color.Green },
            // If a new type is added, make sure to add a colour here.
        };

        /// <summary>
        /// Render nodes and edges to a <see cref="Bitmap"/>.
        /// Parameters width/height refer to the ASCII width/height that the original
        /// AsciiMapRenderer would produce. They are used to normalise coordinates.
        /// </summary>
        public static Bitmap Render(IEnumerable<Node> nodes, int asciiWidth = 80, int asciiHeight = 24)
        {
            var nodeList = nodes.ToList();
            if (!nodeList.Any())
            {
                // Return an empty bitmap with a single pixel of black.
                return new Bitmap(1, 1);
            }

            // 1. Determine bounds of the node coordinates
            float minX = nodeList.Min(n => n.Coordinates.X);
            float maxX = nodeList.Max(n => n.Coordinates.X);
            float minY = nodeList.Min(n => n.Coordinates.Y);
            float maxY = nodeList.Max(n => n.Coordinates.Y);

            float xRange = Math.Max(maxX - minX, 0.01f);
            float yRange = Math.Max(maxY - minY, 0.01f);

            // 2. Compute bitmap size in pixels
            int bitmapWidth  = (asciiWidth  + 2 * MarginBlocks) * BlockSize;
            int bitmapHeight = (asciiHeight + 2 * MarginBlocks) * BlockSize;

            var bitmap = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.Clear(Color.Black);

                // Draw edges first so they appear under nodes
                foreach (var from in nodeList)
                {
                    foreach (var to in from.NextLevelNodes)
                    {
                        var fromPixel = ToPixelCoords(from, minX, maxX, minY, maxY, asciiWidth, asciiHeight, blockSize: BlockSize, marginBlocks: MarginBlocks, xRange: xRange, yRange: yRange);
                        var toPixel   = ToPixelCoords(to,   minX, maxX, minY, maxY, asciiWidth, asciiHeight, blockSize: BlockSize, marginBlocks: MarginBlocks, xRange: xRange, yRange: yRange);

                        var pen = Pens.White; // simple white lines for edges
                        g.DrawLine(pen, fromPixel.Item1 + BlockSize / 2, fromPixel.Item2 + BlockSize / 2,
                                   toPixel.Item1   + BlockSize / 2, toPixel.Item2   + BlockSize / 2);
                    }
                }

                // Draw nodes as solid blocks
                foreach (var node in nodeList)
                {
                    var (px, py) = ToPixelCoords(node, minX, maxX, minY, maxY, asciiWidth, asciiHeight, BlockSize, MarginBlocks, xRange, yRange);
                    var color = NodeColors.ContainsKey(node.Type) ? NodeColors[node.Type] : Color.Gray;
                    using var brush = new SolidBrush(color);
                    g.FillRectangle(brush, px, py, BlockSize, BlockSize);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Convert a node's world coordinates into pixel coordinates on the bitmap.
        /// </summary>
        private static (int, int) ToPixelCoords(Node node,
                                                 float minX, float maxX, float minY, float maxY,
                                                 int asciiWidth, int asciiHeight,
                                                 int blockSize, int marginBlocks,
                                                 float xRange, float yRange)
        {
            // Normalise to [0,1] based on overall bounds
            float normX = (node.Coordinates.X - minX) / xRange;
            float normY = (node.Coordinates.Y - minY) / yRange;

            // Scale to ascii grid, subtracting borders as ascii renderer does
            int asciiX = (int)Math.Floor(normX * (asciiWidth  - 4));
            int asciiY = (int)Math.Floor(normY * (asciiHeight - 1));

            // Convert grid space to pixel space, adding margin
            int px = asciiX * blockSize + marginBlocks * blockSize;
            int py = asciiY * blockSize + marginBlocks * blockSize;
            return (px, py);
        }
    }
}
