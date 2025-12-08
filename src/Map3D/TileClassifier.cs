using System;

namespace maps.Map3D
{
    public static class TileClassifier
    {

        private static bool IsStraight(Neighbor4 n)
        {
            bool ns = (n & Neighbor4.North) != 0 && (n & Neighbor4.South) != 0;
            bool ew = (n & Neighbor4.East)  != 0 && (n & Neighbor4.West)  != 0;
            return ns || ew;
        }


        //
        // CLASSIFY PAVING PATTERN (8-direction Wang tiles)
        //
public static (PavingPattern, Rotation) ClassifyPaving(Neighbor8 n)
{
    bool N  = n.Has(Neighbor8.North);
    bool E  = n.Has(Neighbor8.East);
    bool S  = n.Has(Neighbor8.South);
    bool W  = n.Has(Neighbor8.West);

    bool NE = n.Has(Neighbor8.NorthEast);
    bool SE = n.Has(Neighbor8.SouthEast);
    bool SW = n.Has(Neighbor8.SouthWest);
    bool NW = n.Has(Neighbor8.NorthWest);

    int card = Count(N,E,S,W);
    int diag = Count(NE,SE,SW,NW);

    // ============================================================
    // 0. Completely isolated - empty tile
    // ============================================================
    if (card == 0)
        return (PavingPattern.None, Rotation.R0);

    // ============================================================
    // 1. FULL BLOCK (3×3 solid)
    // ============================================================
    if (card == 4 && diag == 4)
        return (PavingPattern.Full, Rotation.R0);

    // ============================================================
    // 2. BASIC CARDINAL SHAPES:
    //    End, Straight, Corner, TJunction, Cross
    // ============================================================

    // -------- END (1 direction) --------
    if (card == 1)
    {
        if (N) return (PavingPattern.End, Rotation.R0);
        if (E) return (PavingPattern.End, Rotation.R90);
        if (S) return (PavingPattern.End, Rotation.R180);
        return (PavingPattern.End, Rotation.R270);
    }

    // -------- STRAIGHT --------
    if (N && S && !E && !W)
        return (PavingPattern.Straight, Rotation.R0);
    if (E && W && !N && !S)
        return (PavingPattern.Straight, Rotation.R90);

    // -------- CORNER --------
    if (card == 2)
    {
        if (N && E) return (PavingPattern.Corner, Rotation.R0);
        if (E && S) return (PavingPattern.Corner, Rotation.R90);
        if (S && W) return (PavingPattern.Corner, Rotation.R180);
        if (W && N) return (PavingPattern.Corner, Rotation.R270);
    }

    // -------- T-JUNCTION --------
    if (card == 3)
    {
        if (!N) return (PavingPattern.TJunction, Rotation.R0);
        if (!E) return (PavingPattern.TJunction, Rotation.R90);
        if (!S) return (PavingPattern.TJunction, Rotation.R180);
        return (PavingPattern.TJunction, Rotation.R270);
    }

    // -------- CROSS (all four cardinal directions) --------
    if (card == 4)
    {
        // This is where FULL vs CENTER VS CORNER VARIANTS are decided
        if (diag == 0)
            return (PavingPattern.Cross, Rotation.R0);
    }

    // ============================================================
    // 3. EDGE STRIP (block on one side)
    // ============================================================

    // vertical strip on west
    if (W && !E && !N && !S)
        return (PavingPattern.EdgeStrip, Rotation.R270);

    // vertical strip on east
    if (E && !W && !N && !S)
        return (PavingPattern.EdgeStrip, Rotation.R90);

    // horizontal strip north
    if (N && !S && !E && !W)
        return (PavingPattern.EdgeStrip, Rotation.R0);

    // horizontal strip south
    if (S && !N && !E && !W)
        return (PavingPattern.EdgeStrip, Rotation.R180);

    // ============================================================
    // 4. INNER CORNERS (solid block corner)
    // ============================================================
    if (S && E && SE)
        return (PavingPattern.InnerCorner, Rotation.R90);
    if (E && N && NE)
        return (PavingPattern.InnerCorner, Rotation.R0);
    if (N && W && NW)
        return (PavingPattern.InnerCorner, Rotation.R270);
    if (W && S && SW)
        return (PavingPattern.InnerCorner, Rotation.R180);

    // ============================================================
    // 5. OUTER CORNERS (empty diagonal next to two filled)
    // ============================================================
    if (!NE && N && E)
        return (PavingPattern.OuterCorner, Rotation.R0);
    if (!SE && E && S)
        return (PavingPattern.OuterCorner, Rotation.R90);
    if (!SW && S && W)
        return (PavingPattern.OuterCorner, Rotation.R180);
    if (!NW && W && N)
        return (PavingPattern.OuterCorner, Rotation.R270);

    // ============================================================
    // 6. CHAMFERED EDGE (bevel smoothing)
    //
    // Pattern described by:
    //   #@@
    //   @@@
    //   #@@
    // ============================================================
    if (N && S && E && !W && !NE && !SE)
        return (PavingPattern.ChamferedEdge, Rotation.R90);

    if (E && W && S && !N && !SE && !SW)
        return (PavingPattern.ChamferedEdge, Rotation.R180);

    if (S && N && W && !E && !SW && !NW)
        return (PavingPattern.ChamferedEdge, Rotation.R270);

    if (W && E && N && !S && !NW && !NE)
        return (PavingPattern.ChamferedEdge, Rotation.R0);

    // ============================================================
    // 7. CENTER (anything that doesn't fit above rules)
    // ============================================================
    return (PavingPattern.Center, Rotation.R0);
}


private static int Count(params bool[] b)
{
    int c = 0;
    foreach (bool x in b)
        if (x) c++;
    return c;
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
