using System.Threading.Tasks;
using maps;
using maps.Map3D;
using maps.Unity;
using UnityEngine;

namespace Runtime
{
    public class Map3DSpawner : MonoBehaviour
    {
        public TilePrefabRegistry Registry;

        [Tooltip("Optional reference to the map generator used to produce tile data.")]
        public UnityMapGenerator Generator;

        private GameObject currentMapRoot;

        public async Task<GameObject> BuildFromTileInfo(TileInfo[,] tiles)
        {
            var root = await TileChunkBuilder.BuildChunksAsync(tiles, Registry);
            root.transform.SetParent(this.transform);

            currentMapRoot = root;
            return root;
        }

        public async void Generate()
        {
            if (Registry == null)
            {
                Debug.LogError("Map3DSpawner.Generate: TilePrefabRegistry is not assigned.");
                return;
            }

            if (Generator == null)
                Generator = GetComponent<UnityMapGenerator>();

            if (Generator == null)
            {
                Debug.LogError("Map3DSpawner.Generate: No UnityMapGenerator found to produce map data.");
                return;
            }

            Generator.GenerateAndRender();

            if (Generator.LastMap?.TileInfo == null)
            {
                Debug.LogError("Map3DSpawner.Generate: Map generation did not produce tile data.");
                return;
            }

            if (currentMapRoot != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(currentMapRoot);
                else
#endif
                    Destroy(currentMapRoot);
            }

            await BuildFromTileInfo(Generator.LastMap.TileInfo);
        }
    }
}