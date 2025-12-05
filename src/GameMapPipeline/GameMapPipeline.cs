using System.Collections.Generic;

namespace maps.GameMapPipeline
{
    public class GameMapPipeline
    {
        private readonly List<IMapGenStep> steps = new();

        public GameMapPipeline AddStep(IMapGenStep step)
        {
            steps.Add(step);
            return this;
        }

        public GameMap Execute(MapGenParams p)
        {
            // New simplified GameMap constructor
            var map = new GameMap(
                numLevels: p.NumLevels,
                minNodesPerLevel: p.MinNodesPerLevel,
                maxNodesPerLevel: p.MaxNodesPerLevel,
                bifurcationFactor: p.BifurcationFactor
            );

            // Run each pipeline step in order
            foreach (var step in steps)
                step.Execute(map, p);

            return map;
        }
    }
}