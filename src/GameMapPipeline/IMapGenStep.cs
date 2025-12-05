using System.Collections.Generic;

namespace maps.GameMapPipeline
{
    public interface IMapGenStep
    {
        void Execute(GameMap map, MapGenParams p);
    }
}
