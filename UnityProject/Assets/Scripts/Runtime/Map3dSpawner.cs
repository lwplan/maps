using UnityEngine;
using maps;
using maps.Unity;

public class Map3DSpawner : MonoBehaviour
{
    public TilePrefabRegistry Registry;
    public UnityMapGenParams Parameters;
    public TileMap2DRenderer PreviewRenderer;

    private GameObject worldRoot;

    public void Generate()
    {
        if (worldRoot != null)
            DestroyImmediate(worldRoot);

        var map = MapGenerator.Generate(Parameters.ToMapGenParams());

        // optional 2D preview
        PreviewRenderer?.Render(map.TileInfo);

        // build 3D map
        worldRoot = TileChunkBuilder.BuildChunks(map.TileInfo, Registry);
        worldRoot.transform.SetParent(this.transform);
    }

    private void Start()
    {
        Generate();
    }
}