#nullable enable
using System.Collections.Generic;
using System.Numerics;

namespace maps
{
    
    public class Node
    {
        public const int MacroCellSize = 13; // 13×13 tiles per macro-grid cell
        
        //
        // Primary, integer tile coordinates
        public int TileX { get; set; }
        public int TileY { get; set; }

        // Derived macro-grid coordinates (3×3 footprints)
        public int MacroX => TileX / MacroCellSize;
        public int MacroY => TileY / MacroCellSize;
        


        //
        // === 2. Node attributes ===
        //
        
        public int Level { get; set; }
        public NodeType Type { get; set; }
        public string SceneName => Type switch
        {
            NodeType.Combat  => "Combat",
            NodeType.Trading => "Trading",
            NodeType.Start   => "Start",
            NodeType.End     => "End",
            NodeType.Event   => "Event",
            NodeType.Powerup => "Powerup",
            _ => "Unknown"
        };

        public List<Node> NextLevelNodes { get; set; } = new();
        public List<Node> PrevLevelNodes { get; set; } = new();

        public ICombatEncounter? CombatEncounter { get; set; }


        //
        // === 3. Constructor ===
        //
        
        /// <summary>
        /// Creates a node at tile coordinates (tileX, tileY) belonging to a given level.
        /// </summary>
        public Node(int tileX, int tileY, int level, NodeType type, ICombatEncounter? combatEncounter)
        {
            TileX = tileX;
            TileY = tileY;
            Level = level;
            Type = type;
            CombatEncounter = combatEncounter;
            
        }
    }

    public interface ICombatEncounter
    {
    }


    public enum NodeType
    {
        Trading,
        End,
        Combat,
        Start,
        Event,
        Powerup
    }
}