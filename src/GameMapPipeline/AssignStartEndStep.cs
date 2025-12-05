using System.Linq;

namespace maps.GameMapPipeline
{
    public class AssignStartEndStep : IMapGenStep
    {
        public void Execute(GameMap map, MapGenParams p)
        {
            map.StartNode = map.Nodes.First(n => n.Level == 0);
            map.EndNode   = map.Nodes.First(n => n.Level == p.NumLevels - 1);
        }
    }
}