
using System.Collections.Generic;

namespace maps
{
    public interface IMapGenStep
    {
        void Execute(GameMap map, MapGenParams p);
    }
}