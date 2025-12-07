namespace maps.Map3D
{
    public static class TileClassifier
    {
        public static PathShape ClassifyPathShape(Neighbor4 n)
        {
            int count = CountBits((int)n);

            if (count == 0) return PathShape.Center;
            if (count == 1) return PathShape.End;
            if (count == 4) return PathShape.Cross;

            if (count == 2)
            {
                if ((n & Neighbor4.North) != 0 && (n & Neighbor4.South) != 0) return PathShape.Straight;
                if ((n & Neighbor4.East) != 0 && (n & Neighbor4.West) != 0) return PathShape.Straight;
                return PathShape.Corner;
            }

            if (count == 3) return PathShape.TJunction;

            return PathShape.None;
        }

        public static (PavingPattern, Rotation) ClassifyPavingPattern(Neighbor8 m)
        {
            // Simplified stub — you fill in pattern grouping later.
            // For now:
            int count = CountBits((int)m);

            if (count == 0) return (PavingPattern.Center, Rotation.R0);
            if (count == 8) return (PavingPattern.Full, Rotation.R0);

            // TODO: link exact masks to:
            // EdgeStrip, InnerCorner, ChamferedEdge, OuterCorner, End, Straight, Corner...

            return (PavingPattern.Straight, Rotation.R0);
        }

        private static int CountBits(int x)
        {
            int c = 0;
            while (x != 0)
            {
                x &= (x - 1);
                c++;
            }
            return c;
        }
    }
}