using maps.Map3D;
using UnityEngine;

namespace Runtime
{
    public class Map3DSpawner : MonoBehaviour
    {
        public TilePrefabRegistry Registry;

        public async void BuildFromTileInfo(TileInfo[,] tiles)
        {
            var root = await TileChunkBuilder.BuildChunksAsync(tiles, Registry);
            root.transform.SetParent(this.transform);
        }
    }
}