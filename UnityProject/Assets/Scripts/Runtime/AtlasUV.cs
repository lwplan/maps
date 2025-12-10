using UnityEngine;

namespace Runtime
{
    public static class AtlasUV
    {
        public const int COLS = 4;
        public const int ROWS = 4;

        public static Rect GetRect(int index)
        {
            int x = index % COLS;
            int y = index / COLS;

            float w = 1f / COLS;
            float h = 1f / ROWS;

            return new Rect(x * w, y * h, w, h);
        }
    }

}