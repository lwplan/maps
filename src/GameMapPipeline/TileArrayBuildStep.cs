
using maps.Map3D; // Your non-unity tile system namespace

namespace maps.GameMapPipeline
{
    public class TileArrayBuildStep : IMapGenStep
    {
        public void Execute(GameMap map, MapGenParams p)
        {
            // Convert biome map into a 2D BiomeType array
            int w = map.Biomes.Width;
            int h = map.Biomes.Height;

            Map3D.BiomeType[,] biomeArray = new Map3D.BiomeType[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                biomeArray[x, y] = ConvertBiome(map.Biomes[x, y]);

            // For now elevation = 0 everywhere
            int[,] elevation = new int[w, h];

            // Build final tiles
            map.TileInfo = TileMapBuilder.Build(
                map.PavedMask,
                map.PathMask,
                map.EventMask,
                biomeArray,
                elevation);
        }

        private Map3D.BiomeType ConvertBiome(BiomeType b)
        {
            return b switch {
                BiomeType.Dunes => Map3D.BiomeType.Dune,
                BiomeType.Canyon => Map3D.BiomeType.Canyon,
                BiomeType.Mountain => Map3D.BiomeType.Mountain,
                BiomeType.Sea => Map3D.BiomeType.Sea,
                BiomeType.Town => Map3D.BiomeType.City,
                BiomeType.Battlement => Map3D.BiomeType.Ruins,
                _ => Map3D.BiomeType.Desert
            };
        }
    }
}
