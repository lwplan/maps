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
            if (minNodeDistance.HasValue)
            {
                AnnealNodes(map, minNodeDistance.Value);
            }
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

        private void AnnealNodes(GameMap map, int minDistance)
        {
            float target = minDistance;
            float step = 0.1f;
            var nodes = map.Nodes;
            int iteration = 0;
            bool overlapsRemain;
            do
            {
                overlapsRemain = false;
                var dispX = new float[nodes.Count];
                var dispY = new float[nodes.Count];
                for (int a = 0; a < nodes.Count; a++)
                {
                    for (int b = a + 1; b < nodes.Count; b++)
                    {
                        var n1 = nodes[a];
                        var n2 = nodes[b];
                        float dx = n2.Coordinates.X - n1.Coordinates.X;
                        float dy = n2.Coordinates.Y - n1.Coordinates.Y;
                        float dxWorld = dx * map.RegionSize.X;
                        float dyWorld = dy * map.RegionSize.Y;
                        float distSqWorld = dxWorld * dxWorld + dyWorld * dyWorld;
                        if (distSqWorld < target * target && distSqWorld > 0f)
                        {
                            overlapsRemain = true;
                            var distWorld = MathF.Sqrt(distSqWorld);
                            var overlap = target - distWorld;
                            float dirXWorld = dxWorld / distWorld;
                            float dirYWorld = dyWorld / distWorld;
                            dispX[a] -= (dirXWorld * overlap * step) / map.RegionSize.X;
                            dispY[a] -= (dirYWorld * overlap * step) / map.RegionSize.Y;
                            dispX[b] += (dirXWorld * overlap * step) / map.RegionSize.X;
                            dispY[b] += (dirYWorld * overlap * step) / map.RegionSize.Y;
                        }
                    }
                }
                for (int a = 0; a < nodes.Count; a++)
                {
                    var n = nodes[a];
                    n.Coordinates = new Vector2(
                        Math.Clamp(n.Coordinates.X + dispX[a], 0f, 1f),
                        Math.Clamp(n.Coordinates.Y + dispY[a], 0f, 1f));
                }
                iteration++;
            } while (overlapsRemain && iteration < 50);

            // Final corrective pass to guarantee the minimum distance after annealing
            for (int a = 0; a < nodes.Count; a++)
            {
                for (int b = a + 1; b < nodes.Count; b++)
                {
                    var n1 = nodes[a];
                    var n2 = nodes[b];
                    float dxWorld = (n2.Coordinates.X - n1.Coordinates.X) * map.RegionSize.X;
                    float dyWorld = (n2.Coordinates.Y - n1.Coordinates.Y) * map.RegionSize.Y;
                    float distWorld = MathF.Sqrt(dxWorld * dxWorld + dyWorld * dyWorld);
                    if (distWorld < target && distWorld > 0f)
                    {
                        float correction = (target - distWorld) * 0.5f + 0.0001f;
                        float dirXWorld = dxWorld / distWorld;
                        float dirYWorld = dyWorld / distWorld;
                        float adjustX = (dirXWorld * correction) / map.RegionSize.X;
                        float adjustY = (dirYWorld * correction) / map.RegionSize.Y;
                        n1.Coordinates = new Vector2(
                            Math.Clamp(n1.Coordinates.X - adjustX, 0f, 1f),
                            Math.Clamp(n1.Coordinates.Y - adjustY, 0f, 1f));
                        n2.Coordinates = new Vector2(
                            Math.Clamp(n2.Coordinates.X + adjustX, 0f, 1f),
                            Math.Clamp(n2.Coordinates.Y + adjustY, 0f, 1f));
                    }
                }
            }
            foreach (var a in nodes)
                foreach (var b in nodes)
                    if (a != b)
                    {
                        float dx = (b.Coordinates.X - a.Coordinates.X) * map.RegionSize.X;
                        float dy = (b.Coordinates.Y - a.Coordinates.Y) * map.RegionSize.Y;
                        float dist = MathF.Sqrt(dx * dx + dy * dy);
                        if (dist < target)
                            Console.Error.WriteLine($"Violation: Nodes at {a.Coordinates} & {b.Coordinates} closer than {target} ({dist})");
                    }
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
