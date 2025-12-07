using System.Linq;

namespace maps.GameMapPipeline
{
    public class ComputeWorldBoundsStep : IMapGenStep
    {
        public int ExtraPadding = 200; // large enough for biomes + routes

        public void Execute(GameMap map, MapGenParams p)
        {
            int minX = map.Nodes.Min(n => n.TileX) - ExtraPadding;
            int maxX = map.Nodes.Max(n => n.TileX) + ExtraPadding;
            int minY = map.Nodes.Min(n => n.TileY) - ExtraPadding;
            int maxY = map.Nodes.Max(n => n.TileY) + ExtraPadding;

            map.OffsetX = minX;
            map.OffsetY = minY;
            map.TileWidth  = maxX - minX + 1;
            map.TileHeight = maxY - minY + 1;

            // Allocate elevation now (safe default)
            map.Elevation = new int[map.TileWidth, map.TileHeight];
        }
    }
}
