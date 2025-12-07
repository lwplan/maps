using maps;

public class BiomeMap
{
    public readonly BiomeType[,] Tiles;
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
        Tiles = new BiomeType[width, height];
    }

    public bool InBounds(int x, int y)
        => x >= 0 && y >= 0 && x < Width && y < Height;

    public BiomeType this[int x, int y]
    {
        get => Tiles[x, y];
        set => Tiles[x, y] = value;
    }
}