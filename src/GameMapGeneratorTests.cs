using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using SixLabors.ImageSharp;

namespace maps.Tests
{
    [TestFixture]
    public class GameMapGeneratorTests
    {
        [Test]
        public void GenerateMap_WithMinimumDistance_KeepsNodesSeparated()
        {
            // Arrange
            RandomUtil.SetSeed(12345);
            var generator = new GameMapGenerator();
            float pixelsPerUnit = 3f;

            // Act
            var map = generator.GenerateMap(new Vector2(20f, 20f), numLevels: 5, minNodesPerLevel: 2, maxNodesPerLevel: 3, bifurcationFactor: 0.5f, minNodeDistance: 4);

            // Assert
            Assert.That(map.Nodes, Is.Not.Null.And.Not.Empty);
            var scale = BitmapMapRenderer.CalculateScaleFactor(map.Nodes, pixelsPerUnit, map.MinNodeDistance);
            var scaledPositions = new List<Vector2>();
            foreach (var node in map.Nodes)
            {
                scaledPositions.Add(new Vector2(
                    node.Coordinates.X * pixelsPerUnit * scale,
                    node.Coordinates.Y * pixelsPerUnit * scale));
            }

            for (int i = 0; i < scaledPositions.Count; i++)
            {
                for (int j = i + 1; j < scaledPositions.Count; j++)
                {
                    var dx = MathF.Abs(scaledPositions[i].X - scaledPositions[j].X);
                    var dy = MathF.Abs(scaledPositions[i].Y - scaledPositions[j].Y);
                    var axisDistance = dx == 0f ? dy : dy == 0f ? dx : MathF.Min(dx, dy);
                    Assert.That(axisDistance, Is.GreaterThanOrEqualTo(4f),
                        $"Nodes at indices {i} and {j} are {axisDistance} pixels apart on an axis.");
                }
            }
        }

        [Test]
        public void Render_ClampsScaleFactor_WhenBitmapWouldExceedLimits()
        {
            var map = new GameMap(new Vector2(1000f, 1000f), numLevels: 3, minNodesPerLevel: 1, maxNodesPerLevel: 1, bifurcationFactor: 0.5f)
            {
                MinNodeDistance = 5000
            };

            var start = new Node(new Vector2(0f, 0f), 0, NodeType.Start, null);
            var middle = new Node(new Vector2(0f, 0.000001f), 1, NodeType.Combat, null);
            var end = new Node(new Vector2(1f, 1f), 2, NodeType.End, null);

            start.NextLevelNodes.Add(middle);
            middle.PrevLevelNodes.Add(start);
            middle.NextLevelNodes.Add(end);
            end.PrevLevelNodes.Add(middle);

            map.Nodes = new List<Node> { start, middle, end };
            map.StartNode = start;
            map.EndNode = end;

            using Image image = BitmapMapRenderer.Render(map, pixelsPerUnit: 3f);

            Assert.That(image.Width, Is.LessThanOrEqualTo(32000));
            Assert.That(image.Height, Is.LessThanOrEqualTo(32000));
        }

        [Test]
        public void Render_ReportsActualDimensions_WithUserProvidedParameters()
        {
            RandomUtil.SetSeed(9876);
            var generator = new GameMapGenerator();
            var map = generator.GenerateMap(new Vector2(100f, 100f), numLevels: 5, minNodesPerLevel: 2, maxNodesPerLevel: 5, bifurcationFactor: 0.5f, minNodeDistance: 4);

            float pixelsPerUnit = 3f;
            float scale = BitmapMapRenderer.CalculateScaleFactor(map.Nodes, pixelsPerUnit, map.MinNodeDistance);
            int marginPixels = 3 * 1; // BlockSize * MarginBlocks
            const int maxDimension = 32000;

            float projectedWidth = MathF.Ceiling(map.RegionSize.X * pixelsPerUnit * scale) + marginPixels * 2;
            float projectedHeight = MathF.Ceiling(map.RegionSize.Y * pixelsPerUnit * scale) + marginPixels * 2;

            using Image image = BitmapMapRenderer.Render(map, pixelsPerUnit: pixelsPerUnit);

            Assert.That(map.Nodes, Is.Not.Null.And.Not.Empty);
            Assert.That(scale, Is.GreaterThanOrEqualTo(1f));
            Assert.That(image.Width, Is.EqualTo((int)MathF.Min(projectedWidth, maxDimension)));
            Assert.That(image.Height, Is.EqualTo((int)MathF.Min(projectedHeight, maxDimension)));
            Assert.That(image.Width, Is.GreaterThan(60), "Bitmap width is larger than the expected ~60px, investigate node spacing and scale factor");
            Assert.That(image.Height, Is.GreaterThan(60), "Bitmap height is larger than the expected ~60px, investigate node spacing and scale factor");
        }
    }
}
