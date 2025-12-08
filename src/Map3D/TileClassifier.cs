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
            int neighborCount = CountBits((int)m);

            bool n  = (m & Neighbor8.North)     != 0;
            bool ne = (m & Neighbor8.NorthEast) != 0;
            bool e  = (m & Neighbor8.East)      != 0;
            bool se = (m & Neighbor8.SouthEast) != 0;
            bool s  = (m & Neighbor8.South)     != 0;
            bool sw = (m & Neighbor8.SouthWest) != 0;
            bool w  = (m & Neighbor8.West)      != 0;
            bool nw = (m & Neighbor8.NorthWest) != 0;

            int cardinalCount = (n ? 1 : 0) + (e ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0);

            // Empty / fully surrounded
            if (neighborCount == 0) return (PavingPattern.Center, Rotation.R0);
            if (neighborCount == 8) return (PavingPattern.Full, Rotation.R0);

            // Simple 4-neighbor tiling
            if (cardinalCount == 4) return (PavingPattern.Cross, Rotation.R0);

            if (cardinalCount == 3)
            {
                if (!s) return (PavingPattern.TJunction, Rotation.R0);
                if (!w) return (PavingPattern.TJunction, Rotation.R90);
                if (!n) return (PavingPattern.TJunction, Rotation.R180);
                if (!e) return (PavingPattern.TJunction, Rotation.R270);
            }

            if (cardinalCount == 2)
            {
                if (n && s) return (PavingPattern.Straight, Rotation.R0);
                if (e && w) return (PavingPattern.Straight, Rotation.R90);

                if (n && e) return (PavingPattern.Corner, Rotation.R0);
                if (e && s) return (PavingPattern.Corner, Rotation.R90);
                if (s && w) return (PavingPattern.Corner, Rotation.R180);
                if (w && n) return (PavingPattern.Corner, Rotation.R270);
            }

            if (cardinalCount == 1)
            {
                if (n) return (PavingPattern.End, Rotation.R0);
                if (e) return (PavingPattern.End, Rotation.R90);
                if (s) return (PavingPattern.End, Rotation.R180);
                if (w) return (PavingPattern.End, Rotation.R270);
            }

            // Diagonal-only tiles: treat as outer corners/edges
            if (ne && !n && !e) return (PavingPattern.OuterCorner, Rotation.R0);
            if (se && !s && !e) return (PavingPattern.OuterCorner, Rotation.R90);
            if (sw && !s && !w) return (PavingPattern.OuterCorner, Rotation.R180);
            if (nw && !n && !w) return (PavingPattern.OuterCorner, Rotation.R270);

            // Fallback: default to full so tiling remains seamless even with irregular masks
            return (PavingPattern.Full, Rotation.R0);
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