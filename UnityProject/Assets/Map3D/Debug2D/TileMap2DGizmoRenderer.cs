using UnityEngine;
using maps.Map3D;

public class TileMap2DGizmoRenderer : MonoBehaviour
{
    public TileInfo[,] Tiles;
    public float TileSize = 1f;

    public void OnDrawGizmos()
    {
        if (Tiles == null) return;

        int w = Tiles.GetLength(0);
        int h = Tiles.GetLength(1);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var t = Tiles[x, y];

                Gizmos.color = t.IsPath ? Color.green : 
                                t.IsPaved ? Color.yellow : 
                                Color.gray;

                Vector3 pos = new Vector3(x * TileSize, 0, y * TileSize);
                Gizmos.DrawWireCube(pos, Vector3.one * TileSize);
            }
        }
    }
}
