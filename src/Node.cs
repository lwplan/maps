#nullable enable
using System.Numerics;


namespace maps
{
    public class Node
    {
        public Vector2 Coordinates { get; set; } // Normalized [0, 1]
        public int Level { get; set; }
        public NodeType Type { get; set; }
        public string SceneName { get; set; }
        public List<Node> NextLevelNodes { get; set; } = new List<Node>();
        public List<Node> PrevLevelNodes { get; set; } = new List<Node>();

        public ICombatEncounter? CombatEncounter { get; set; }

        //CombatDefinition = GameManager.Instance.Assets.Combats[RandomUtil.Range(0, GameManager.Instance.Assets.Combats.Count)];
        public Node(Vector2 coordinates, int level, NodeType type, ICombatEncounter? combatEncounter)
        {
            CombatEncounter = combatEncounter;
            Coordinates = coordinates;
            Level = level;
            Type = type;
            if (Type == NodeType.Combat) {
                  
                SceneName = "Combat";
            }
            else
            {
                SceneName = "Trading";
            }
        }
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