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

            // Act
            var map = generator.GenerateMap(new Vector2(20f, 20f), numLevels: 5, minNodesPerLevel: 2, maxNodesPerLevel: 3, bifurcationFactor: 0.5f, minNodeDistance: 4);

            // Assert
            Assert.That(map.Nodes, Is.Not.Null.And.Not.Empty);
            var scaledPositions = new List<Vector2>();
            foreach (var node in map.Nodes)
            {
                scaledPositions.Add(new Vector2(node.Coordinates.X * map.RegionSize.X, node.Coordinates.Y * map.RegionSize.Y));
            }

            for (int i = 0; i < scaledPositions.Count; i++)
            {
                for (int j = i + 1; j < scaledPositions.Count; j++)
                {
                    var distance = Vector2.Distance(scaledPositions[i], scaledPositions[j]);
                    Assert.That(distance, Is.GreaterThanOrEqualTo(4f),
                        $"Nodes at indices {i} and {j} are {distance} units apart.");
                }
            }
        }
    }
}
