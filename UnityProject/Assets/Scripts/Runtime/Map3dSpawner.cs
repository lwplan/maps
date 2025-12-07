using maps.Map3D;
using UnityEngine;

namespace Runtime
{
    public class Map3DSpawner : MonoBehaviour
    {
        public TilePrefabRegistry Registry;

        public void BuildFromTileInfo(TileInfo[,] tiles)
        {
            var root = TileChunkBuilder.BuildChunks(tiles, Registry);
            root.transform.SetParent(this.transform);
        }
    }
}