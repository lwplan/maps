using System.Collections.Generic;
using System.Numerics;
using maps.Map3D;

namespace maps
{
    public class GameMap
    {
        // -------------------------------------------------
        // High-level graph structure
        // -------------------------------------------------
        public List<Node> Nodes { get; set; } = new();
        public Node StartNode { get; set; }
        public Node EndNode { get; set; }
        public List<Node> VisitedNodes { get; private set; } = new();

        // Generation parameters
        public int NumLevels { get; private set; }
        public int MinNodesPerLevel { get; private set; }
        public int MaxNodesPerLevel { get; private set; }
        public float BifurcationFactor { get; private set; }

        public int? MinNodeDistance { get; set; }

        // -------------------------------------------------
        // Biome information
        // -------------------------------------------------
        public BiomeMap Biomes { get; set; }
        public BiomeType[,] BiomeTiles { get; set; }

        // -------------------------------------------------
        // Tile masks (intermediate representations)
        // -------------------------------------------------
        public bool[,] PathMask { get; set; }
        public bool[,] PavedMask { get; set; }
        public bool[,] EventMask { get; set; }
        public int[,] Elevation { get; set; }

        // -------------------------------------------------
        // Final tile-level representation
        // -------------------------------------------------
        public TileInfo[,] TileInfo { get; set; }

        // Grid bounds
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }


        // -------------------------------------------------
        // Constructor
        // -------------------------------------------------
        public GameMap(
            int numLevels, 
            int minNodesPerLevel,
            int maxNodesPerLevel,
            float bifurcationFactor)
        {
            NumLevels = numLevels;
            MinNodesPerLevel = minNodesPerLevel;
            MaxNodesPerLevel = maxNodesPerLevel;
            BifurcationFactor = bifurcationFactor;
        }

        // -------------------------------------------------
        // Utility
        // -------------------------------------------------
        public void VisitNode(Node node)
        {
            if (node != null && !VisitedNodes.Contains(node))
                VisitedNodes.Add(node);
        }
    }
}
