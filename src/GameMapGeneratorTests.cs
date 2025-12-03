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
            var scaledMap = GameMapScaler.ScaleForRendering(map, pixelsPerUnit);

            // Assert
            Assert.That(scaledMap.Nodes, Is.Not.Null.And.Not.Empty);
            var scaledPositions = new List<Vector2>();
            foreach (var node in scaledMap.Nodes)
            {
                scaledPositions.Add(new Vector2(
                    node.Coordinates.X * pixelsPerUnit,
                    node.Coordinates.Y * pixelsPerUnit));
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
        public void ScaleForRendering_ClampsScaleFactor_WhenBitmapWouldExceedLimits()
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

            var scaledMap = GameMapScaler.ScaleForRendering(map, pixelsPerUnit: 3f);
            using Image image = BitmapMapRenderer.Render(scaledMap, pixelsPerUnit: 3f);

            Assert.That(image.Width, Is.LessThanOrEqualTo(32000));
            Assert.That(image.Height, Is.LessThanOrEqualTo(32000));
        }

        [Test]
        public void Render_ThrowsWhenBitmapWouldExceedLimitsWithoutScaling()
        {
            var map = new GameMap(new Vector2(20000f, 20000f), numLevels: 3, minNodesPerLevel: 1, maxNodesPerLevel: 1, bifurcationFactor: 0.5f);
            var start = new Node(new Vector2(0f, 0f), 0, NodeType.Start, null);
            var end = new Node(new Vector2(1f, 1f), 2, NodeType.End, null);
            start.NextLevelNodes.Add(end);
            end.PrevLevelNodes.Add(start);
            map.Nodes = new List<Node> { start, end };
            map.StartNode = start;
            map.EndNode = end;

            var ex = Assert.Throws<InvalidOperationException>(
                () => BitmapMapRenderer.Render(map, pixelsPerUnit: 3f));

            Assert.That(ex!.Message, Does.Contain("Projected bitmap exceeds configured maximum dimension"));
            Assert.That(ex.Message, Does.Contain(map.RegionSize.ToString()));
            Assert.That(ex.Message, Does.Contain("Width="));
            Assert.That(ex.Message, Does.Contain("Height="));
        }
    }
}
