using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;

namespace maps
{
    public static class BitmapMapRenderer
    {
        private const int BlockSize = 3;
        private const int MarginBlocks = 1;
        private const int MaxBitmapDimension = 32000;

        private static readonly Dictionary<NodeType, Color> NodeColors = new()
        {
            { NodeType.Start, Color.Yellow },
            { NodeType.End, Color.Magenta },
            { NodeType.Combat, Color.Red },
            { NodeType.Trading, Color.Cyan },
            { NodeType.Event, Color.Blue },
            { NodeType.Powerup, Color.Green },
        };

        public static Image<Rgba32> Render(IEnumerable<Node> nodes, int asciiWidth = 80, int asciiHeight = 24)
        {
            var nodeList = nodes.ToList();
            if (!nodeList.Any())
                return Image.Load<Rgba32>("empty.png");

            // coordinate bounds
            float minX = nodeList.Min(n => n.Coordinates.X);
            float maxX = nodeList.Max(n => n.Coordinates.X);
            float minY = nodeList.Min(n => n.Coordinates.Y);
            float maxY = nodeList.Max(n => n.Coordinates.Y);

            float xRange = Math.Max(maxX - minX, 0.01f);
            float yRange = Math.Max(maxY - minY, 0.01f);

            int width = (asciiWidth + 2 * MarginBlocks) * BlockSize;
            int height = (asciiHeight + 2 * MarginBlocks) * BlockSize;

            var img = new Image<Rgba32>(width, height);

            var pen = new SolidPen(Color.White, 1);

            img.Mutate(ctx =>
            {
                ctx.Fill(Color.Black);

                //
                // Draw edges
                //
                foreach (var from in nodeList)
                {
                    foreach (var to in from.NextLevelNodes)
                    {
                        var p1 = ToPixel(from, minX, xRange, minY, yRange, asciiWidth, asciiHeight);
                        var p2 = ToPixel(to, minX, xRange, minY, yRange, asciiWidth, asciiHeight);

                        // center within node block
                        p1.X += BlockSize * 0.5f;
                        p1.Y += BlockSize * 0.5f;
                        p2.X += BlockSize * 0.5f;
                        p2.Y += BlockSize * 0.5f;

                        var pathBuilder = new PathBuilder();
                        if (p1.X == p2.X || p1.Y == p2.Y)
                        {
                            pathBuilder.AddLine(p1, p2);
                        }
                        else
                        {
                            var bend = new PointF(p2.X, p1.Y);
                            pathBuilder.AddLine(p1, bend);
                            pathBuilder.AddLine(bend, p2);
                        }
                        var line = pathBuilder.Build();

                        ctx.Draw(pen, line);
                    }
                }

                //
                // Draw nodes
                //
                foreach (var node in nodeList)
                {
                    var p = ToPixel(node, minX, xRange, minY, yRange, asciiWidth, asciiHeight);

                    var color = NodeColors.TryGetValue(node.Type, out var c)
                        ? c
                        : Color.Gray;

                    IPath rect = new RectangularPolygon(p.X, p.Y, BlockSize, BlockSize);
                    ctx.Fill(color, rect);
                }
            });

            return img;
        }

        public static Image<Rgba32> Render(GameMap map, float pixelsPerUnit = BlockSize, int marginBlocks = MarginBlocks)
        {
            var nodeList = map.Nodes?.ToList();
            if (nodeList == null || nodeList.Count == 0)
                return Image.Load<Rgba32>("empty.png");

            var regionSize = map.RegionSize;
            if (regionSize.X <= 0 || regionSize.Y <= 0)
                return Image.Load<Rgba32>("empty.png");

            float scaleFactor = CalculateScaleFactor(nodeList, pixelsPerUnit, map.MinNodeDistance);
            int marginPixels = marginBlocks * BlockSize;
            scaleFactor = ClampScaleFactor(regionSize, pixelsPerUnit, marginPixels, scaleFactor);
            int width = (int)MathF.Ceiling(regionSize.X * pixelsPerUnit * scaleFactor) + marginPixels * 2;
            int height = (int)MathF.Ceiling(regionSize.Y * pixelsPerUnit * scaleFactor) + marginPixels * 2;

            var img = new Image<Rgba32>(width, height);
            var pen = new SolidPen(Color.White, 1);

            img.Mutate(ctx =>
            {
                ctx.Fill(Color.Black);

                foreach (var from in nodeList)
                {
                    foreach (var to in from.NextLevelNodes)
                    {
                        var p1 = ToPixel(from, pixelsPerUnit, marginPixels, scaleFactor);
                        var p2 = ToPixel(to, pixelsPerUnit, marginPixels, scaleFactor);

                        p1.X += BlockSize * 0.5f;
                        p1.Y += BlockSize * 0.5f;
                        p2.X += BlockSize * 0.5f;
                        p2.Y += BlockSize * 0.5f;

                        var pathBuilder = new PathBuilder();
                        if (p1.X == p2.X || p1.Y == p2.Y)
                        {
                            pathBuilder.AddLine(p1, p2);
                        }
                        else
                        {
                            var bend = new PointF(p2.X, p1.Y);
                            pathBuilder.AddLine(p1, bend);
                            pathBuilder.AddLine(bend, p2);
                        }
                        var line = pathBuilder.Build();

                        ctx.Draw(pen, line);
                    }
                }

                foreach (var node in nodeList)
                {
                    var p = ToPixel(node, pixelsPerUnit, marginPixels, scaleFactor);

                    var color = NodeColors.TryGetValue(node.Type, out var c)
                        ? c
                        : Color.Gray;

                    IPath rect = new RectangularPolygon(p.X, p.Y, BlockSize, BlockSize);
                    ctx.Fill(color, rect);
                }
            });

            return img;
        }

        public static float CalculateScaleFactor(IEnumerable<Node> nodes, float pixelsPerUnit, int? minNodeDistance)
        {
            if (!minNodeDistance.HasValue)
            {
                return 1f;
            }

            var list = nodes.ToList();
            if (list.Count < 2)
            {
                return 1f;
            }

            float minAxisDistancePixels = float.MaxValue;
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    var dx = MathF.Abs(list[j].Coordinates.X - list[i].Coordinates.X) * pixelsPerUnit;
                    var dy = MathF.Abs(list[j].Coordinates.Y - list[i].Coordinates.Y) * pixelsPerUnit;
                    var axisDistance = dx == 0f ? dy : dy == 0f ? dx : MathF.Min(dx, dy);
                    minAxisDistancePixels = MathF.Min(minAxisDistancePixels, axisDistance);
                }
            }

            if (minAxisDistancePixels <= 0f)
            {
                minAxisDistancePixels = float.Epsilon;
            }

            return minAxisDistancePixels >= minNodeDistance.Value
                ? 1f
                : minNodeDistance.Value / minAxisDistancePixels;
        }

        private static PointF ToPixel(
            Node node,
            float minX,
            float xRange,
            float minY,
            float yRange,
            int asciiWidth,
            int asciiHeight)
        {
            float nx = (node.Coordinates.X - minX) / xRange;
            float ny = (node.Coordinates.Y - minY) / yRange;

            int ax = (int)(nx * asciiWidth);
            int ay = (int)(ny * asciiHeight);

            return new PointF(
                (ax + MarginBlocks) * BlockSize,
                (ay + MarginBlocks) * BlockSize
            );
        }

        private static PointF ToPixel(Node node, float pixelsPerUnit, int marginPixels)
        {
            return new PointF(
                node.Coordinates.X * pixelsPerUnit + marginPixels,
                node.Coordinates.Y * pixelsPerUnit + marginPixels);
        }

        private static PointF ToPixel(Node node, float pixelsPerUnit, int marginPixels, float scaleFactor)
        {
            return new PointF(
                node.Coordinates.X * pixelsPerUnit * scaleFactor + marginPixels,
                node.Coordinates.Y * pixelsPerUnit * scaleFactor + marginPixels);
        }

        private static float ClampScaleFactor(Vector2 regionSize, float pixelsPerUnit, int marginPixels, float scaleFactor)
        {
            float usableWidth = MathF.Max(MaxBitmapDimension - marginPixels * 2, BlockSize);
            float usableHeight = MathF.Max(MaxBitmapDimension - marginPixels * 2, BlockSize);

            float widthCap = usableWidth / MathF.Max(regionSize.X * pixelsPerUnit, float.Epsilon);
            float heightCap = usableHeight / MathF.Max(regionSize.Y * pixelsPerUnit, float.Epsilon);

            var cappedScale = MathF.Min(scaleFactor, MathF.Min(widthCap, heightCap));

            float projectedWidth = MathF.Ceiling(regionSize.X * pixelsPerUnit * cappedScale) + marginPixels * 2;
            float projectedHeight = MathF.Ceiling(regionSize.Y * pixelsPerUnit * cappedScale) + marginPixels * 2;

            if (projectedWidth > MaxBitmapDimension || projectedHeight > MaxBitmapDimension)
            {
                float adjustedWidthCap = (usableWidth - 1f) / MathF.Max(regionSize.X * pixelsPerUnit, float.Epsilon);
                float adjustedHeightCap = (usableHeight - 1f) / MathF.Max(regionSize.Y * pixelsPerUnit, float.Epsilon);
                cappedScale = MathF.Min(cappedScale, MathF.Min(adjustedWidthCap, adjustedHeightCap));
            }

            return cappedScale;
        }
    }
}
