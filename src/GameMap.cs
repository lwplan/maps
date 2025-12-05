using System.Collections.Generic;
using System.Numerics;

namespace maps
{
    public class GameMap
    {
        public List<Node> Nodes { get; set; }
        public Node StartNode { get; set; }
        public Node EndNode { get; set; }
        public List<Node> VisitedNodes { get; private set; } // Track visited nodes
        public Vector2 RegionSize { get; set; }
        public int NumLevels { get; private set; }
        public int MinNodesPerLevel { get; private set; }
        public int MaxNodesPerLevel { get; private set; }
        public float BifurcationFactor { get; private set; }
        public int? MinNodeDistance { get; set; }
        public BiomeMap Biomes { get; set; }
        public GameMap(int numLevels, int minNodesPerLevel, int maxNodesPerLevel, float bifurcationFactor)
        {
            NumLevels = numLevels;
            MinNodesPerLevel = minNodesPerLevel;
            MaxNodesPerLevel = maxNodesPerLevel;
            BifurcationFactor = bifurcationFactor;
            Nodes = new List<Node>();
            VisitedNodes = new List<Node>(); // Initialize empty
        }


        // Add a method to mark a node as visited
        public void VisitNode(Node node)
        {
            if (node != null && !VisitedNodes.Contains(node))
            {
                VisitedNodes.Add(node);
                // Debug.Log($"Node visited: {node.Coordinates} (Level {node.Level})");
            }
        }
    }
}