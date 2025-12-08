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
        // The intended 3×3 autotile set is organized as follows (rotation R0 is "facing north"):
        //
        //  - No neighbors → None
        //  - All neighbors → Full
        //  - Single cardinal neighbor → End (R0: N, R90: E, R180: S, R270: W)
        //  - Two opposite cardinals → Straight (R0: N/S, R90: E/W)
        //  - Two adjacent cardinals → Corner (R0: NE, R90: SE, R180: SW, R270: NW)
        //  - Two adjacent cardinals with the shared diagonal missing → InnerCorner
        //      * R0: N+E, !NE   * R90: E+S, !SE   * R180: S+W, !SW   * R270: W+N, !NW
        //  - One cardinal plus its clockwise or counter-clockwise diagonal (and nothing else) → ChamferedEdge
        //      * R0: NE, N/E   * R90: SE, E/S   * R180: SW, S/W   * R270: NW, W/N
        //  - Three cardinals → TJunction (R0 opens north, then clockwise)
        //  - Four cardinals → Cross
        //  - Cardinal strip defined only by diagonals → EdgeStrip
        //      * R0: NW+NE   * R90: NE+SE   * R180: SE+SW   * R270: SW+NW
        //  - Single diagonal with no cardinals → OuterCorner (R0: NE, R90: SE, R180: SW, R270: NW)
        //  - Fallback → Center
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

            int cardinalCount = 0;
            if (n) cardinalCount++;
            if (e) cardinalCount++;
            if (s) cardinalCount++;
            if (w) cardinalCount++;

            //
            // ---- Diagonal-only configurations ----
            //
            if (cardinalCount == 0)
            {
                if (ne && !se && !sw && !nw)
                    return (PavingPattern.OuterCorner, Rotation.R0);
                if (se && !ne && !sw && !nw)
                    return (PavingPattern.OuterCorner, Rotation.R90);
                if (sw && !ne && !se && !nw)
                    return (PavingPattern.OuterCorner, Rotation.R180);
                if (nw && !ne && !se && !sw)
                    return (PavingPattern.OuterCorner, Rotation.R270);

                if (nw && ne && !se && !sw)
                    return (PavingPattern.EdgeStrip, Rotation.R0);
                if (ne && se && !sw && !nw)
                    return (PavingPattern.EdgeStrip, Rotation.R90);
                if (se && sw && !ne && !nw)
                    return (PavingPattern.EdgeStrip, Rotation.R180);
                if (sw && nw && !ne && !se)
                    return (PavingPattern.EdgeStrip, Rotation.R270);
            }

            //
            // ---- Inner corners (cardinal corner, missing diagonal) ----
            //
            if (n && e && !ne)
                return (PavingPattern.InnerCorner, Rotation.R0);
            if (e && s && !se)
                return (PavingPattern.InnerCorner, Rotation.R90);
            if (s && w && !sw)
                return (PavingPattern.InnerCorner, Rotation.R180);
            if (w && n && !nw)
                return (PavingPattern.InnerCorner, Rotation.R270);

            //
            // ---- Chamfered edges (cardinal + diagonal) ----
            //
            if (ne && (n ^ e) && !s && !w && !se && !sw && !nw)
                return (PavingPattern.ChamferedEdge, Rotation.R0);
            if (se && (e ^ s) && !n && !w && !ne && !sw && !nw)
                return (PavingPattern.ChamferedEdge, Rotation.R90);
            if (sw && (s ^ w) && !n && !e && !ne && !se && !nw)
                return (PavingPattern.ChamferedEdge, Rotation.R180);
            if (nw && (w ^ n) && !e && !s && !ne && !se && !sw)
                return (PavingPattern.ChamferedEdge, Rotation.R270);

            //
            //  ----  END tiles  ----
            //
            if (cardinalCount == 1)
            {
                if (n)
                    return (PavingPattern.End, Rotation.R0);
                if (e)
                    return (PavingPattern.End, Rotation.R90);
                if (s)
                    return (PavingPattern.End, Rotation.R180);
                if (w)
                    return (PavingPattern.End, Rotation.R270);
            }

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
            if (cardinalCount == 3)
            {
                if (!n && e && w && s)
                    return (PavingPattern.TJunction, Rotation.R0); // open north
                if (!e && n && s && w)
                    return (PavingPattern.TJunction, Rotation.R90);
                if (!s && e && w && n)
                    return (PavingPattern.TJunction, Rotation.R180);
                if (!w && n && s && e)
                    return (PavingPattern.TJunction, Rotation.R270);
            }

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
