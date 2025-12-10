using UnityEngine;


namespace Runtime
{
    public static class MeshUVTools
    {
        /// <summary>
        /// Creates a new mesh with UVs remapped into atlas cell, including UV rotation.
        /// </summary>
        public static Mesh CreateUVRemappedCopy(
            Mesh source,
            int atlasIndex,
            int rotationSteps // 0,1,2,3 → 0°,90°,180°,270°
        )
        {
            Mesh m = Object.Instantiate(source);

            var uvs = m.uv;
            Rect r = AtlasUV.GetRect(atlasIndex);

            for (int i = 0; i < uvs.Length; i++)
            {
                Vector2 uv = uvs[i];

                // Rotate UV inside its local [0..1] tile
                uv = RotateUV(uv, rotationSteps);

                // Scale into atlas region
                uv = new Vector2(
                    Mathf.Lerp(r.xMin, r.xMax, uv.x),
                    Mathf.Lerp(r.yMin, r.yMax, uv.y)
                );

                uvs[i] = uv;
            }

            m.uv = uvs;
            return m;
        }

        /// <summary>
        /// Rotates UV around tile center.
        /// </summary>
        private static Vector2 RotateUV(Vector2 uv, int steps)
        {
            steps = steps % 4;
            if (steps == 0) return uv;

            uv -= new Vector2(0.5f, 0.5f);

            for (int i = 0; i < steps; i++)
                uv = new Vector2(-uv.y, uv.x); // 90° rotation

            uv += new Vector2(0.5f, 0.5f);
            return uv;
        }
    }

}