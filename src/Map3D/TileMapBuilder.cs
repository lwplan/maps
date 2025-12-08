namespace maps.Map3D {
    public static class TileMapBuilder
    {
        public static TileInfo[,] Build(
            bool[,] paved,
            bool[,] path,
            bool[,] events,
            BiomeType[,] biomes,
            int[,] elevation)
        {
            int w = paved.GetLength(0);
            int h = paved.GetLength(1);

            TileInfo[,] tiles = new TileInfo[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    TileInfo t = new TileInfo();

                    t.IsPaved = paved[x, y];
                    t.IsEventNode = events[x, y];
                    t.Biome = biomes[x, y];
                    t.ElevationLevel = elevation[x, y];

                    t.PathNeighbors4 = TileNeighbors.GetPathNeighbors(path, x, y);


                    t.PavingMask8 = TileNeighbors.GetPavingMask(paved, x, y);
                    (t.PavingPattern, t.Rotation) = TileClassifier.ClassifyPaving(t.PavingMask8);

                    tiles[x, y] = t;
                }
            }

            return tiles;
        }
    }
}
