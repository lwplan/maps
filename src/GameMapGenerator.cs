using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace maps
{
    public class GameMapGenerator
    {
        public GameMap GenerateMap(Vector2 regionSize, int numLevels, int minNodesPerLevel, int maxNodesPerLevel, float bifurcationFactor, int? minNodeDistance = null)
        {
            var map = new GameMap(regionSize, numLevels, minNodesPerLevel, maxNodesPerLevel, bifurcationFactor);
            map.MinNodeDistance = minNodeDistance;

            GenerateNodes(map);
            Triangulator.GenerateTriangulatedEdges(map.Nodes);
            EliminateDisconnectedNodes(map);
            EnforceConnectivityAndBifurcation(map);
            return map;
        }

        private void GenerateNodes(GameMap map)
        {
            if (map.RegionSize.X <= 0 || map.RegionSize.Y <= 0 || map.NumLevels < 3)
                return;

            map.Nodes = NodeGenerator.GenerateNodes(map.RegionSize, map.NumLevels, map.MinNodesPerLevel, map.MaxNodesPerLevel);
            if (map.Nodes.Count < 3)
            {
                Components.Map.Log.Info($"Map generation failed: Only {map.Nodes.Count} nodes generated (minimum 3 required)." );
                map.Nodes = null;
                return;
            }
            map.StartNode = map.Nodes.Find(n => n.Level == 0);
            map.EndNode = map.Nodes.Find(n => n.Level == map.NumLevels - 1);
        }

        private void FitRegionToNodes(GameMap map)
        {
            var minX = map.Nodes.Min(n => n.Coordinates.X);
            var maxX = map.Nodes.Max(n => n.Coordinates.X);
            var minY = map.Nodes.Min(n => n.Coordinates.Y);
            var maxY = map.Nodes.Max(n => n.Coordinates.Y);
            map.RegionSize = new Vector2(maxX - minX + 10f, maxY - minY + 10f);
            for (int i = 0; i < map.Nodes.Count; i++)
            {
                var n = map.Nodes[i];
                n.Coordinates = new Vector2(n.Coordinates.X - minX, n.Coordinates.Y - minY);
            }
            map.StartNode = map.Nodes.Find(n => n.Level == 0);
            map.EndNode = map.Nodes.Find(n => n.Level == map.NumLevels - 1);
        }

        private void EliminateDisconnectedNodes(GameMap map)
        {
            var toRemove = new List<Node>();
            foreach (var n in map.Nodes)
            {
                bool isStart = n.Level == 0;
                bool isEnd = n.Level == map.NumLevels - 1;
                if (!isStart && n.PrevLevelNodes.Count == 0) toRemove.Add(n);
                else if (!isEnd && n.NextLevelNodes.Count == 0) toRemove.Add(n);
            }
            foreach (var n in toRemove)
            {
                foreach (var other in map.Nodes)
                {
                    other.NextLevelNodes.Remove(n);
                    other.PrevLevelNodes.Remove(n);
                }
                map.Nodes.Remove(n);
            }
            map.StartNode = map.Nodes.Find(n => n.Level == 0);
            map.EndNode = map.Nodes.Find(n => n.Level == map.NumLevels - 1);
        }

        private void EnforceConnectivityAndBifurcation(GameMap map)
        {
            // Original implementation retained
            var level1Nodes = map.Nodes.FindAll(n => n.Level == 1);
            foreach (var n in level1Nodes)
            {
                if (!map.StartNode.NextLevelNodes.Contains(n))
                {
                    map.StartNode.NextLevelNodes.Add(n);
                    n.PrevLevelNodes.Add(map.StartNode);
                }
            }
            var penNodes = map.Nodes.FindAll(n => n.Level == map.NumLevels - 2);
            foreach (var n in penNodes)
            {
                if (!map.EndNode.PrevLevelNodes.Contains(n))
                {
                    n.NextLevelNodes.Add(map.EndNode);
                    map.EndNode.PrevLevelNodes.Add(n);
                }
            }
            // Bifurcation trimming logic
            bool edgesRemoved;
            int iteration = 0;
            do
            {
                edgesRemoved = false;
                foreach (var n in map.Nodes)
                {
                    if (n.Level == map.NumLevels - 1) continue;
                    var nexts = n.NextLevelNodes.ToList();
                    foreach (var next in nexts)
                    {
                        var temp = n.NextLevelNodes.ToList();
                        temp.Remove(next);
                        var prevTemp = next.PrevLevelNodes.ToList();
                        prevTemp.Remove(n);
                        bool safe1 = n.Level == 0 || temp.Count > 0;
                        bool safe2 = next.Level == map.NumLevels - 1 || prevTemp.Count > 0;
                        if (safe1 && safe2 && RandomUtil.Value() < map.BifurcationFactor)
                        {
                            n.NextLevelNodes.Remove(next);
                            next.PrevLevelNodes.Remove(n);
                            edgesRemoved = true;
                        }
                    }
                }
                iteration++;
            } while (edgesRemoved);
        }
    }
}
