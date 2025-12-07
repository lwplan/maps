using UnityEngine;

namespace maps.Unity
{
    public class UnityMapGenerator : MonoBehaviour
    {
        [Header("Map Generation Parameters")]
        public UnityMapGenParams Parameters;

        [Header("2D Preview Renderer")]
        public TileMap2DRenderer PreviewRenderer;

        private GameMap lastMap;

        public void GenerateAndRender()
        {
            if (Parameters == null)
            {
                Debug.LogError("Missing Parameters");
                return;
            }

            var p = Parameters.ToMapGenParams();
            lastMap = MapGenerator.Generate(p);

            if (PreviewRenderer != null)
                PreviewRenderer.Render(lastMap.TileInfo);
        }

        // Optional: generate on Start
        void Start()
        {
            GenerateAndRender();
        }
    }
}
