// using UnityEngine;
using maps.Map3D;

public static class TestMapGenerator
{
    public static TileInfo[,] GenerateTestMap()
    {
        const int W = 16;
        const int H = 16;

        TileInfo[,] tiles = new TileInfo[W, H];

        // Initialize all as empty ground
        for (int x = 0; x < W; x++)
        {
            for (int y = 0; y < H; y++)
            {
                tiles[x, y] = new TileInfo
                {
                    IsPaved = false,
                    IsEventNode = false,
                    Biome = BiomeType.Desert,
                    ElevationLevel = 0,
                    ElevationPattern = ElevationPattern.Flat,
                    // Neighbor4 not used
                    // Neighbor8 not used
                    PavingPattern = PavingPattern.None,
                    Rotation = Rotation.R0,
                };
            }
        }

        // SECTION 1 — Horizontal straight test (row = 2)
        for (int x = 1; x < 15; x++)
            tiles[x, 2].IsPaved = true;

        // SECTION 2 — Vertical straight test (column = 2)
        for (int y = 1; y < 15; y++)
            tiles[2, y].IsPaved = true;

        // SECTION 3 — Cross test at (8,8)
        for (int x = 4; x <= 12; x++)
            tiles[x, 8].IsPaved = true;
        for (int y = 4; y <= 12; y++)
            tiles[8, y].IsPaved = true;

        // SECTION 4 — Corner tests
        tiles[4, 4].IsPaved = true;
        tiles[5, 4].IsPaved = true;
        tiles[4, 5].IsPaved = true;

        tiles[12, 4].IsPaved = true;
        tiles[11, 4].IsPaved = true;
        tiles[12, 5].IsPaved = true;

        // SECTION 5 — Block region (solid 4×4)
        for (int x = 1; x <= 4; x++)
            for (int y = 10; y <= 13; y++)
                tiles[x, y].IsPaved = true;

        // SECTION 6 — Chamfer stress test
        tiles[10,10].IsPaved = true;
        tiles[11,10].IsPaved = true;
        tiles[12,10].IsPaved = true;
        tiles[12,11].IsPaved = true;
        tiles[12,12].IsPaved = true;

        // After marking filled tiles, compute neighbor masks + classifier
        ComputeNeighborsAndPatterns(tiles);

        return tiles;
    }

    private static void ComputeNeighborsAndPatterns(TileInfo[,] tiles)
    {
        int W = tiles.GetLength(0);
        int H = tiles.GetLength(1);

        for (int x = 0; x < W; x++)
        {
            for (int y = 0; y < H; y++)
            {
                var t = tiles[x, y];

                Neighbor8 mask = 0;

                bool Filled(int ix, int iy)
                {
                    if (ix < 0 || iy < 0 || ix >= W || iy >= H) return false;
                    return tiles[ix, iy].IsPaved;
                }

                if (Filled(x, y+1))  mask |= Neighbor8.North;
                if (Filled(x+1, y+1)) mask |= Neighbor8.NorthEast;
                if (Filled(x+1, y))   mask |= Neighbor8.East;
                if (Filled(x+1, y-1)) mask |= Neighbor8.SouthEast;
                if (Filled(x, y-1))   mask |= Neighbor8.South;
                if (Filled(x-1, y-1)) mask |= Neighbor8.SouthWest;
                if (Filled(x-1, y))   mask |= Neighbor8.West;
                if (Filled(x-1, y+1)) mask |= Neighbor8.NorthWest;

                t.PavingMask8 = mask;

                // Run classifier
                (t.PavingPattern, t.Rotation) = TileClassifier.ClassifyPaving(mask);
            }
        }
    }
}
