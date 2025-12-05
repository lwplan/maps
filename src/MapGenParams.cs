using System.Numerics;


    namespace maps
    {
        public record MapGenParams(
            int NumLevels,
            int MinNodesPerLevel,
            int MaxNodesPerLevel,
            float BifurcationFactor
        );
    }


