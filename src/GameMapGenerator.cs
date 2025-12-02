using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameBase.Model.Map;
using GameBase.UI.Core.Implementations;
using GameBase.UI.Util;
using maps;


namespace GameBase.UI.Components.Map
{
    public class GameMapGenerator
    {
        public GameMap GenerateMap(Vector2 regionSize, int numLevels, int minNodesPerLevel, int maxNodesPerLevel, float bifurcationFactor)
        {
            GameMap map = new GameMap(regionSize, numLevels, minNodesPerLevel, maxNodesPerLevel, bifurcationFactor);
            GenerateMap(map);
            return map;
        }

        private void GenerateMap(GameMap map)
        {
            if (map.RegionSize.X <= 0 || map.RegionSize.Y <= 0 || map.NumLevels < 3)
            {
                return;
            }

            map.Nodes = NodeGenerator.GenerateNodes(map.RegionSize, map.NumLevels, map.MinNodesPerLevel, map.MaxNodesPerLevel);
            // Log.Info($"Generated {map.Nodes.Count} nodes across {map.NumLevels} levels.");
            if (map.Nodes.Count < 3)
            {
                Log.Info($"Map generation failed: Only {map.Nodes.Count} nodes generated (minimum 3 required).");
                map.Nodes = null;
                return;
            }
            map.StartNode = map.Nodes.Find(n => n.Level == 0);
            map.EndNode = map.Nodes.Find(n => n.Level == map.NumLevels - 1);
            // Log.Info($"Start node: {map.StartNode.Coordinates}, End node: {map.EndNode.Coordinates}");

            Triangulator.GenerateTriangulatedEdges(map.Nodes);
            int totalConnections = map.Nodes.Sum(n => n.NextLevelNodes.Count);
            // Log.Info($"Triangulation completed with {totalConnections} connections.");
            if (totalConnections == 0)
            {
                Log.Error("No connections generated. Check triangulation or level adjacency.");
            }

            EliminateDisconnectedNodes(map);
            EnforceConnectivityAndBifurcation(map);
            totalConnections = map.Nodes.Sum(n => n.NextLevelNodes.Count);
            // Log.Info($"Final connections after enforcement: {totalConnections}");
            if (totalConnections == 0)
            {
                Log.Error("Enforcement resulted in no connections.");
            }
        }

        private void EliminateDisconnectedNodes(GameMap map)
        {
            // Log.Info("Eliminating disconnected nodes...");
            List<Node> nodesToRemove = new List<Node>();

            foreach (var node in map.Nodes)
            {
                bool isStart = node.Level == 0;
                bool isEnd = node.Level == map.NumLevels - 1;

                if (!isStart && node.PrevLevelNodes.Count == 0)
                {
                    nodesToRemove.Add(node);
                    // Log.Info($"Marked for removal (no prev): {node.Coordinates} (Level {node.Level})");
                }
                else if (!isEnd && node.NextLevelNodes.Count == 0)
                {
                    nodesToRemove.Add(node);
                 //   Log.Info($"Marked for removal (no next): {node.Coordinates} (Level {node.Level})");
                }
            }

            foreach (var node in nodesToRemove)
            {
                foreach (var otherNode in map.Nodes)
                {
                    otherNode.NextLevelNodes.Remove(node);
                    otherNode.PrevLevelNodes.Remove(node);
                }
                map.Nodes.Remove(node);
                // Log.Info($"Removed disconnected node: {node.Coordinates} (Level {node.Level})");
            }

            map.StartNode = map.Nodes.Find(n => n.Level == 0);
            map.EndNode = map.Nodes.Find(n => n.Level == map.NumLevels - 1);
            // Log.Info($"Remaining nodes: {map.Nodes.Count}");
        }

        private void EnforceConnectivityAndBifurcation(GameMap map)
        {
            // Log.Info("Enforcing connectivity and trimming excess edges with bifurcation factor...");

            // Step 1: Enforce Start node connects to all Level 1 nodes
            var level1Nodes = map.Nodes.FindAll(n => n.Level == 1);
            // Log.Info($"Level 1 nodes: {level1Nodes.Count}");
            foreach (var level1Node in level1Nodes)
            {
                if (!map.StartNode.NextLevelNodes.Contains(level1Node))
                {
                    map.StartNode.NextLevelNodes.Add(level1Node);
                    level1Node.PrevLevelNodes.Add(map.StartNode);
                    // Log.Info($"Added Start -> Level 1: {map.StartNode.Coordinates} -> {level1Node.Coordinates}");
                }
            }

            // Step 2: Enforce End node connects to all penultimate level nodes
            var penultimateLevelNodes = map.Nodes.FindAll(n => n.Level == map.NumLevels - 2);
            // Log.Info($"Penultimate level nodes: {penultimateLevelNodes.Count}");
            foreach (var penNode in penultimateLevelNodes)
            {
                if (!map.EndNode.PrevLevelNodes.Contains(penNode))
                {
                    penNode.NextLevelNodes.Add(map.EndNode);
                    map.EndNode.PrevLevelNodes.Add(penNode);
                    // Log.Info($"Added Level {map.NumLevels - 2} -> End: {penNode.Coordinates} -> {map.EndNode.Coordinates}");
                }
            }

            // Step 3: Eliminate excess edges with bifurcation probability
            bool edgesRemoved;
            int iteration = 0;
            do
            {
                edgesRemoved = false;
                foreach (var node in map.Nodes)
                {
                    if (node.Level == map.NumLevels - 1) continue;

                    var nextNodes = node.NextLevelNodes.ToList();
                    foreach (var nextNode in nextNodes)
                    {
                        var nodeNextTemp = node.NextLevelNodes.ToList();
                        nodeNextTemp.Remove(nextNode);
                        var nextPrevTemp = nextNode.PrevLevelNodes.ToList();
                        nextPrevTemp.Remove(node);

                        bool nodeSafe = node.Level == 0 || nodeNextTemp.Count > 0;
                        bool nextSafe = nextNode.Level == map.NumLevels - 1 || nextPrevTemp.Count > 0;

                        if (nodeSafe && nextSafe && RandomUtil.Value() < map.BifurcationFactor)
                        {
                            node.NextLevelNodes.Remove(nextNode);
                            nextNode.PrevLevelNodes.Remove(node);
                            edgesRemoved = true;
                            // Log.Info($"Removed edge (bifurcation factor {map.BifurcationFactor:F2}): {node.Coordinates} (Level {node.Level}) -> {nextNode.Coordinates} (Level {nextNode.Level})");
                        }
                    }
                }
                iteration++;
                // Log.Info($"Iteration {iteration}: Edges removed this pass = {edgesRemoved}");
            } while (edgesRemoved);

            float branchedCount = map.Nodes.Count(n => n.Level != map.NumLevels - 1 && n.NextLevelNodes.Count > 1);
            float singleCount = map.Nodes.Count(n => n.Level != map.NumLevels - 1 && n.NextLevelNodes.Count == 1);
            float finalRatio = singleCount > 0 ? branchedCount / (branchedCount + singleCount) : branchedCount > 0 ? 1f : 0f;
            // Log.Info($"Final bifurcation - branched: {branchedCount}, single: {singleCount}, ratio: {finalRatio:F2} (factor: {map.BifurcationFactor:F2})");

            foreach (var node in map.Nodes)
            {
                if (node.Level != 0 && node.PrevLevelNodes.Count == 0)
                    Log.Warning($"Node {node.Coordinates} (Level {node.Level}) has no PrevLevelNodes after trimming.");
                if (node.Level != map.NumLevels - 1 && node.NextLevelNodes.Count == 0)
                    Log.Warning($"Node {node.Coordinates} (Level {node.Level}) has no NextLevelNodes after trimming.");
            }
        }
    }
}