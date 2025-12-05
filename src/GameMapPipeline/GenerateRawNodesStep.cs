using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace maps.GameMapPipeline
{
    public class GenerateRawNodesStep : IMapGenStep
    {
        private const float TileSizeMeters = 1.8f;

        // World spacing ranges
        private const float MinLevelSpacingMeters = 50;
        private const float MaxLevelSpacingMeters = 150f;

        private const float MinVerticalSpacingMeters = 50;
        private const float MaxVerticalSpacingMeters = 150f;

        public void Execute(GameMap map, MapGenParams p)
        {
            var nodes = new List<Node>();

            int minLevelTiles = MetersToTiles(MinLevelSpacingMeters);
            int maxLevelTiles = MetersToTiles(MaxLevelSpacingMeters);

            int minVertTiles = MetersToTiles(MinVerticalSpacingMeters);
            int maxVertTiles = MetersToTiles(MaxVerticalSpacingMeters);

            // Compute world X tile coordinate per level
            int[] levelX = new int[p.NumLevels];
            int accumX = 0;

            for (int level = 0; level < p.NumLevels; level++)
            {
                if (level > 0)
                    accumX += RandomUtil.Range(minLevelTiles, maxLevelTiles);

                levelX[level] = accumX;
            }

            int centerY = 0; // vertical center of world

            // Generate nodes per level
            for (int level = 0; level < p.NumLevels; level++)
            {
                int numNodes =
                    (level == 0 || level == p.NumLevels - 1)
                        ? 1
                        : RandomUtil.Range(p.MinNodesPerLevel, p.MaxNodesPerLevel + 1);

                var yPositions = GenerateVerticalTilePositions(
                    numNodes,
                    centerY,
                    minVertTiles,
                    maxVertTiles
                );

                for (int i = 0; i < numNodes; i++)
                {
                    NodeType type =
                        (level == 0) ? NodeType.Start :
                        (level == p.NumLevels - 1) ? NodeType.End :
                        NodeType.Combat;

                    var node = new Node(levelX[level], yPositions[i], level, type, new CombatEncounter());

                    nodes.Add(node);
                }
            }

            map.Nodes = nodes;
        }

        private int MetersToTiles(float meters)
            => (int)MathF.Round(meters / TileSizeMeters);

        /// <summary>
        /// Generates vertically spaced tile Y coordinates centered on centerY.
        /// Keeps ordering but avoids overlapping.
        /// </summary>
        private List<int> GenerateVerticalTilePositions(
            int count,
            int centerY,
            int minSpacing,
            int maxSpacing)
        {
            if (count == 1)
                return new List<int> { centerY };

            var ys = new List<int>();

            int baseY = centerY + RandomUtil.Range(-minSpacing / 2, minSpacing / 2);
            ys.Add(baseY);

            int lastUp = baseY;
            int lastDown = baseY;

            for (int i = 1; i < count; i++)
            {
                bool goUp = (i % 2 == 1);

                if (goUp)
                {
                    lastUp += RandomUtil.Range(minSpacing, maxSpacing);
                    ys.Add(lastUp);
                }
                else
                {
                    lastDown -= RandomUtil.Range(minSpacing, maxSpacing);
                    ys.Add(lastDown);
                }
            }

            ys.Sort();
            return ys;
        }
    }

    public class CombatEncounter : ICombatEncounter
    {
    }
}
