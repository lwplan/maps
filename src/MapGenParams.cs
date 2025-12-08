namespace maps
{
    using System.Numerics;

    public record MapGenParams(
        int NumLevels,
        int MinNodesPerLevel,
        int MaxNodesPerLevel,
        float BifurcationFactor,
        int? MinNodeDistance = null,
        Vector2? RegionSize = null
    );
}


