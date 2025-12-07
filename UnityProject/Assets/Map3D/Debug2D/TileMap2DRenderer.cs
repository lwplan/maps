using maps.Map3D;
using UnityEngine;
using UnityEngine.UI;   // IMPORTANT

public class TileMap2DRenderer : MonoBehaviour
{
    public TileColorScheme ColorScheme;
    public RawImage TargetImage;   // ← Instead of Renderer
    public float PixelsPerTile = 4;

    private Texture2D texture;

    public void Render(TileInfo[,] tiles)
    {
        int width  = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        int texW = Mathf.Max(1, Mathf.FloorToInt(width  * PixelsPerTile));
        int texH = Mathf.Max(1, Mathf.FloorToInt(height * PixelsPerTile));

        texture = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            Color c = ColorScheme.GetColorForTile(tiles[x, y]);

            for (int dx = 0; dx < PixelsPerTile; dx++)
            for (int dy = 0; dy < PixelsPerTile; dy++)
            {
                texture.SetPixel(
                    x * (int)PixelsPerTile + dx,
                    y * (int)PixelsPerTile + dy,
                    c
                );
            }
        }

        texture.Apply();
        TargetImage.texture = texture;   // KEY LINE
    }
}