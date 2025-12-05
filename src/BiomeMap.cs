using maps;

public class BiomeMap
{
    public readonly Biome[,] Tiles;
    public readonly int Width;
    public readonly int Height;

    // Offsets so world coords map to the array
    public readonly int OffsetX;
    public readonly int OffsetY;

    public BiomeMap(int width, int height, int offsetX, int offsetY)
    {
        Width = width;
        Height = height;
        OffsetX = offsetX;
        OffsetY = offsetY;
        Tiles = new Biome[width, height];
    }

    public bool InBounds(int x, int y)
        => x >= 0 && y >= 0 && x < Width && y < Height;

    public Biome this[int x, int y]
    {
        get => Tiles[x, y];
        set => Tiles[x, y] = value;
    }
}