using System.Numerics;
using GameBase.Model.Map;
using GameBase.UI.Util;

public interface ICombatEncounter {

}

class CombatEncounter : ICombatEncounter {

}


namespace maps
{
    public static class NodeGenerator
    {
        public static List<Node> GenerateNodes(Vector2 regionSize, int numLevels, int minNodesPerLevel, int maxNodesPerLevel)
        {
            List<Node> nodes = new List<Node>();
            for (int level = 0; level < numLevels; level++)
            {
                int numNodes = (level == 0 || level == numLevels - 1) ? 1 : RandomUtil.Range(minNodesPerLevel, maxNodesPerLevel + 1);
                float x = (float)level / (numLevels - 1);
                float spacing = 1f / (numNodes + 1);

                for (int i = 0; i < numNodes; i++)
                {
                    float y = (i + 1) * spacing + RandomUtil.Range(-0.05f, 0.05f);
                    y = Math.Clamp(y, 0f, 1f);
                    Vector2 coord = new Vector2(x, y);
                    NodeType type = (level == 0) ? NodeType.Trading :
                        (level == numLevels - 1) ? NodeType.End :
                        NodeType.Combat;
                        // (NodeType)Random.Range(2, 5);
                        CombatEncounter? combatEncounter = null;


                        nodes.Add(new Node(coord, level, type, new CombatEncounter()));
                }
            }
            return nodes;
        }
    }
}