using maps.Map3D;
using UnityEngine;


[CreateAssetMenu(fileName = "TileColorScheme", menuName = "Map3D/Tile Color Scheme")]
public class TileColorScheme : ScriptableObject
{
    public Color SandColor       = new Color(0.9f, 0.8f, 0.6f);
    public Color PathColor       = new Color(0.3f, 0.3f, 0.3f);
    public Color EventColor      = new Color(1f, 0.5f, 0f);
    public Color BiomeCanyon     = new Color(0.6f, 0.3f, 0.2f);
    public Color BiomeDune       = new Color(1f, 0.9f, 0.6f);
    public Color BiomeSea        = new Color(0.2f, 0.4f, 0.85f);
    public Color ElevationColor  = new Color(0.5f, 0.8f, 0.5f);

    public Color GetColorForTile(TileInfo t)
    {
        // Priority: event → path → paved → biome → default
        if (t.IsEventNode)
            return EventColor;
        

        if (t.IsPaved)
        {
            // Paved, but not path – same color or lighter?
            return PathColor * 1.3f;
        }

        // biome fallback
        return t.Biome switch
        {
            BiomeType.Canyon => BiomeCanyon,
            BiomeType.Dune   => BiomeDune,
            BiomeType.Sea    => BiomeSea,
            _                => SandColor
        };
    }
}
