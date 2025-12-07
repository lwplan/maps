namespace maps.Map3D {
    public static class TileNeighbors
    {
        public static Neighbor4 GetPathNeighbors(bool[,] path, int x, int y)
        {
            int w = path.GetLength(0);
            int h = path.GetLength(1);

            Neighbor4 n = Neighbor4.None;

            if (y + 1 < h && path[x, y + 1]) n |= Neighbor4.North;
            if (x + 1 < w && path[x + 1, y]) n |= Neighbor4.East;
            if (y - 1 >= 0 && path[x, y - 1]) n |= Neighbor4.South;
            if (x - 1 >= 0 && path[x - 1, y]) n |= Neighbor4.West;

            return n;
        }

        public static Neighbor8 GetPavingMask(bool[,] paved, int x, int y)
        {
            int w = paved.GetLength(0);
            int h = paved.GetLength(1);

            Neighbor8 m = Neighbor8.None;

            bool N(int dx, int dy)
            {
                int xx = x + dx, yy = y + dy;
                if (xx < 0 || yy < 0 || xx >= w || yy >= h) return false;
                return paved[xx, yy];
            }

            if (N(0, 1))  m |= Neighbor8.North;
            if (N(1, 1))  m |= Neighbor8.NorthEast;
            if (N(1, 0))  m |= Neighbor8.East;
            if (N(1, -1)) m |= Neighbor8.SouthEast;
            if (N(0, -1)) m |= Neighbor8.South;
            if (N(-1, -1))m |= Neighbor8.SouthWest;
            if (N(-1, 0)) m |= Neighbor8.West;
            if (N(-1, 1)) m |= Neighbor8.NorthWest;

            return m;
        }
    }
}
