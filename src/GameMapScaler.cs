using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace maps
{
    public static class GameMapScaler
    {
        public static GameMap ScaleForRendering(GameMap sourceMap, float pixelsPerUnit, int marginBlocks = BitmapMapRenderer.MarginBlocks)
        {
            if (sourceMap == null)
            {
                throw new ArgumentNullException(nameof(sourceMap));
            }

            if (sourceMap.Nodes == null || sourceMap.Nodes.Count == 0)
            {
                return sourceMap;
            }

            float scaleFactor = CalculateScaleFactor(sourceMap.Nodes, pixelsPerUnit, sourceMap.MinNodeDistance, out _);
            int marginPixels = marginBlocks * BitmapMapRenderer.BlockSize;
            scaleFactor = ClampScaleFactor(sourceMap.RegionSize, pixelsPerUnit, marginPixels, scaleFactor);

            if (MathF.Abs(scaleFactor - 1f) < float.Epsilon)
            {
                return CloneWithScale(sourceMap, 1f);
            }

            return CloneWithScale(sourceMap, scaleFactor);
        }

        public static float CalculateScaleFactor(IEnumerable<Node> nodes, float pixelsPerUnit, int? minNodeDistance, out float minAxisDistancePixels)
        {
            minAxisDistancePixels = 0f;
            if (!minNodeDistance.HasValue)
            {
                return 1f;
            }

            var list = nodes.ToList();
            if (list.Count < 2)
            {
                return 1f;
            }

            minAxisDistancePixels = float.MaxValue;
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

        private static float ClampScaleFactor(Vector2 regionSize, float pixelsPerUnit, int marginPixels, float scaleFactor)
        {
            float usableWidth = MathF.Max(BitmapMapRenderer.MaxBitmapDimension - marginPixels * 2, BitmapMapRenderer.BlockSize);
            float usableHeight = MathF.Max(BitmapMapRenderer.MaxBitmapDimension - marginPixels * 2, BitmapMapRenderer.BlockSize);

            float widthCap = usableWidth / MathF.Max(regionSize.X * pixelsPerUnit, float.Epsilon);
            float heightCap = usableHeight / MathF.Max(regionSize.Y * pixelsPerUnit, float.Epsilon);

            var cappedScale = MathF.Min(scaleFactor, MathF.Min(widthCap, heightCap));

            float projectedWidth = MathF.Ceiling(regionSize.X * pixelsPerUnit * cappedScale) + marginPixels * 2;
            float projectedHeight = MathF.Ceiling(regionSize.Y * pixelsPerUnit * cappedScale) + marginPixels * 2;

            if (projectedWidth > BitmapMapRenderer.MaxBitmapDimension || projectedHeight > BitmapMapRenderer.MaxBitmapDimension)
            {
                float adjustedWidthCap = (usableWidth - 1f) / MathF.Max(regionSize.X * pixelsPerUnit, float.Epsilon);
                float adjustedHeightCap = (usableHeight - 1f) / MathF.Max(regionSize.Y * pixelsPerUnit, float.Epsilon);
                cappedScale = MathF.Min(cappedScale, MathF.Min(adjustedWidthCap, adjustedHeightCap));
            }

            return cappedScale;
        }

        private static GameMap CloneWithScale(GameMap sourceMap, float scaleFactor)
        {
            var scaledRegion = new Vector2(sourceMap.RegionSize.X * scaleFactor, sourceMap.RegionSize.Y * scaleFactor);
            var scaledMap = new GameMap(scaledRegion, sourceMap.NumLevels, sourceMap.MinNodesPerLevel, sourceMap.MaxNodesPerLevel, sourceMap.BifurcationFactor)
            {
                MinNodeDistance = sourceMap.MinNodeDistance
            };

            var nodeMap = new Dictionary<Node, Node>();
            foreach (var node in sourceMap.Nodes!)
            {
                var scaledNode = new Node(node.Coordinates * scaleFactor, node.Level, node.Type, node.CombatEncounter)
                {
                    SceneName = node.SceneName
                };
                nodeMap[node] = scaledNode;
                scaledMap.Nodes.Add(scaledNode);
            }

            foreach (var original in sourceMap.Nodes)
            {
                var scaled = nodeMap[original];
                foreach (var next in original.NextLevelNodes)
                {
                    scaled.NextLevelNodes.Add(nodeMap[next]);
                }
                foreach (var prev in original.PrevLevelNodes)
                {
                    scaled.PrevLevelNodes.Add(nodeMap[prev]);
                }
            }

            scaledMap.StartNode = sourceMap.StartNode != null && nodeMap.ContainsKey(sourceMap.StartNode)
                ? nodeMap[sourceMap.StartNode]
                : null;
            scaledMap.EndNode = sourceMap.EndNode != null && nodeMap.ContainsKey(sourceMap.EndNode)
                ? nodeMap[sourceMap.EndNode]
                : null;

            foreach (var visited in sourceMap.VisitedNodes)
            {
                if (nodeMap.TryGetValue(visited, out var scaledVisited))
                {
                    scaledMap.VisitNode(scaledVisited);
                }
            }

            return scaledMap;
        }
    }
}
