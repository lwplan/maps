using UnityEngine;

namespace Runtime
{
    public static class TileMeshFactory
    {
        public static Mesh QuadTile(float size = 2f)
        {
            Mesh m = new Mesh();

            float s = size;

            m.vertices = new Vector3[]
            {
                new Vector3(0,0,0),
                new Vector3(s,0,0),
                new Vector3(0,0,s),
                new Vector3(s,0,s),
            };

            m.triangles = new int[]
            {
                0,2,1,
                2,3,1
            };

            m.uv = new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1)
            };

            m.RecalculateNormals();
            m.RecalculateBounds();

            return m;
        }
    }
}