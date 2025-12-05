using System;
using System.Collections.Generic;
using System.Linq;

namespace maps.GameMapPipeline
{
    public class TriangulationStep : IMapGenStep
    {
        public void Execute(GameMap map, MapGenParams p)
        {
            var grouped = map.Nodes
                .GroupBy(n => n.Level)
                .OrderBy(g => g.Key)
                .ToList();

            for (int i = 0; i < grouped.Count - 1; i++)
            {
                var levelA = grouped[i].OrderBy(n => n.TileY).ToList();
                var levelB = grouped[i + 1].OrderBy(n => n.TileY).ToList();

                ConnectLevels(levelA, levelB, p.BifurcationFactor);
            }
        }

        private void ConnectLevels(List<Node> levelA, List<Node> levelB, float bifurcationFactor)
        {
            foreach (var nodeA in levelA)
            {
                // 1. Always connect to nearest node
                var nearest = FindKthNearest(nodeA, levelB, 0);
                AddDirectedEdge(nodeA, nearest);

                // 2. Optional bifurcation: connect to second-nearest node
                if (RandomUtil.Range(0f, 1f) < bifurcationFactor &&
                    levelB.Count > 1)
                {
                    var second = FindKthNearest(nodeA, levelB, 1);
                    AddDirectedEdge(nodeA, second);
                }
            }

            // 3. Ensure no isolated level B nodes
            foreach (var nodeB in levelB)
            {
                if (nodeB.PrevLevelNodes.Count == 0)
                {
                    var nearest = FindKthNearest(nodeB, levelA, 0);
                    AddDirectedEdge(nearest, nodeB);
                }
            }
        }

        /// <summary>
        /// Returns the k-th nearest node in the candidate list by vertical tile distance.
        /// k=0 => nearest
        /// k=1 => second nearest, etc.
        /// </summary>
        private Node FindKthNearest(Node origin, List<Node> candidates, int k)
        {
            return candidates
                .OrderBy(n => Math.Abs(n.TileY - origin.TileY))
                .Skip(k)
                .First();
        }

        private void AddDirectedEdge(Node from, Node to)
        {
            if (!from.NextLevelNodes.Contains(to))
            {
                from.NextLevelNodes.Add(to);
                to.PrevLevelNodes.Add(from);
            }
        }
    }
}
