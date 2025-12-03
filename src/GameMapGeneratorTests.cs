using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;

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
    }
}
