using System;

namespace maps.Map3D
{
    public static class TileClassifier
    {
        //
        // CLASSIFY PATH SHAPE (4-direction Wang tiles)
        //
        public static PathShape ClassifyPathShape(Neighbor4 n)
        {
            int count = CountBits((int)n);

            return count switch
            {
                0 => PathShape.None,
                1 => PathShape.End,
                2 => IsStraight(n) ? PathShape.Straight : PathShape.Corner,
                3 => PathShape.TJunction,
                4 => PathShape.Cross,
                _ => PathShape.None
            };
        }

        private static bool IsStraight(Neighbor4 n)
        {
            bool ns = (n & Neighbor4.North) != 0 && (n & Neighbor4.South) != 0;
            bool ew = (n & Neighbor4.East)  != 0 && (n & Neighbor4.West)  != 0;
            return ns || ew;
        }


        //
        // CLASSIFY PAVING PATTERN (8-direction Wang tiles)
        //
        public static (PavingPattern, Rotation) ClassifyPavingPattern(Neighbor8 mask)
        {
            int count = CountBits((int)mask);

            // 1. no paving here → NONE tile
            if (count == 0) 
                return (PavingPattern.None, Rotation.R0);

            // 2. all neighbors paved → FULL (solid interior tile)
            if (count == 8)
                return (PavingPattern.Full, Rotation.R0);

            // 3. determine the configuration
            bool n  = mask.Has(Neighbor8.North);
            bool ne = mask.Has(Neighbor8.NorthEast);
            bool e  = mask.Has(Neighbor8.East);
            bool se = mask.Has(Neighbor8.SouthEast);
            bool s  = mask.Has(Neighbor8.South);
            bool sw = mask.Has(Neighbor8.SouthWest);
            bool w  = mask.Has(Neighbor8.West);
            bool nw = mask.Has(Neighbor8.NorthWest);

            //
            //  ----  END tiles  ----
            //
            if (!n && e && w && s)
                return (PavingPattern.End, Rotation.R0);   // open to North
            if (!e && n && s && w)
                return (PavingPattern.End, Rotation.R90);  // open to East
            if (!s && e && w && n)
                return (PavingPattern.End, Rotation.R180); // open to South
            if (!w && n && s && e)
                return (PavingPattern.End, Rotation.R270); // open to West

            //
            // ---- STRAIGHTS ----
            //
            if (n && s && !e && !w)
                return (PavingPattern.Straight, Rotation.R0);

            if (e && w && !n && !s)
                return (PavingPattern.Straight, Rotation.R90);

            //
            // ---- CORNERS ----
            //
            if (n && e && !s && !w)
                return (PavingPattern.Corner, Rotation.R0); // NE
            if (e && s && !n && !w)
                return (PavingPattern.Corner, Rotation.R90); // SE
            if (s && w && !n && !e)
                return (PavingPattern.Corner, Rotation.R180); // SW
            if (w && n && !s && !e)
                return (PavingPattern.Corner, Rotation.R270); // NW

            //
            // ---- TJunctions ----
            //
            if (!n && e && w && s)
                return (PavingPattern.TJunction, Rotation.R0); // open north
            if (!e && n && s && w)
                return (PavingPattern.TJunction, Rotation.R90);
            if (!s && e && w && n)
                return (PavingPattern.TJunction, Rotation.R180);
            if (!w && n && s && e)
                return (PavingPattern.TJunction, Rotation.R270);

            //
            // ---- CROSS ----
            //
            if (n && e && s && w)
                return (PavingPattern.Cross, Rotation.R0);

            //
            // Fallback: treat as center
            //
            return (PavingPattern.Center, Rotation.R0);
        }


        //
        // UTIL
        //
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

    public static class Neighbor8Extensions
    {
        public static bool Has(this Neighbor8 m, Neighbor8 flag)
            => (m & flag) != 0;
    }
}
