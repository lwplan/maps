using System;

namespace maps.Map3D
{
    public static class TileClassifier
    {
        

        //
        // CLASSIFY PAVING PATTERN (8-direction Wang tiles)
        //
public static (PavingPattern, Rotation) ClassifyPaving(Neighbor8 n)
{
    bool C = n.Has(Neighbor8.Center);
    
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
    int total = diag + card + 1;

    // No center point: empty
    if (!C)
        return (PavingPattern.None, Rotation.R0);
    
    if (card == 0)
        return (PavingPattern.None, Rotation.R0);

    // ============================================================
    // 1. FULL BLOCK (3×3 solid)
    // ============================================================
    if (total == 9)
        return (PavingPattern.Full, Rotation.R0);

    if (total == 8)
    {
        if (!NE)  return (PavingPattern.OuterCorner, Rotation.R180);
        if (!SE)  return (PavingPattern.OuterCorner, Rotation.R270);
        if (!SW)  return (PavingPattern.OuterCorner, Rotation.R0);
        if (!NW)  return (PavingPattern.OuterCorner, Rotation.R90);
    }
    
    // ============================================================
    // 6. CHAMFERED EDGE (bevel smoothing)
    //
    // Pattern described by:
    //   #@@
    //   @@@
    //   #@@
    // ============================================================
    if (N && S && E && W && !NW && !SW  && NE && SE) // West side
        return (PavingPattern.ChamferedEdge, Rotation.R90);

    if (N && E && S && W && !NE && !NW  && SE && SW) // North Side
        return (PavingPattern.ChamferedEdge, Rotation.R180);

    if (N && E && S && W && !SE && !SW  && NE && NW) // South Side
        return (PavingPattern.ChamferedEdge, Rotation.R0);

    if (N && E && S && W && !NE && !SE && NW && SW) // East Side
        return (PavingPattern.ChamferedEdge, Rotation.R270);

    // ============================================================
    // 3. EDGE STRIP (block on one side)
    // ============================================================

    // vertical strip on west
    if (W && NW && SW && !E && N && S)
        return (PavingPattern.EdgeStrip, Rotation.R180);

    // vertical strip on east
    if (E  && NE && SE && !W && N && S)
        return (PavingPattern.EdgeStrip, Rotation.R0);

    // horizontal strip north
    if (N && NW && NE && !S && E && W)
        return (PavingPattern.EdgeStrip, Rotation.R270);

    // horizontal strip south
    if (S && SE && SW && !N && E && W)
        return (PavingPattern.EdgeStrip, Rotation.R90);
    
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
    // 2. BASIC CARDINAL SHAPES:
    //    End, Straight, Corner, TJunction, Cross
    // ============================================================

    
    // -------- CROSS (all four cardinal directions) --------
    if (card == 4)
    {
        return (PavingPattern.Cross, Rotation.R0);
    }
    
    // -------- T-JUNCTION --------
    if (card == 3)
    {
        if (E && S) return (PavingPattern.TJunction, Rotation.R0);
        if (W && S) return (PavingPattern.TJunction, Rotation.R90);
        if (N && W) return (PavingPattern.TJunction, Rotation.R180);
        return (PavingPattern.TJunction, Rotation.R270);
    }
    
    // -------- STRAIGHT --------
    if (N && S && !E && !W)
        return (PavingPattern.Straight, Rotation.R0);
    if (E && W && !N && !S)
        return (PavingPattern.Straight, Rotation.R90);
    
    // -------- CORNER --------
    if (card == 2)
    {
        if (N && E) return (PavingPattern.Corner, Rotation.R270);
        if (E && S) return (PavingPattern.Corner, Rotation.R0);
        if (S && W) return (PavingPattern.Corner, Rotation.R90);
        if (W && N) return (PavingPattern.Corner, Rotation.R180);
    }
    
    if (card == 1)
    {
        if (N) return (PavingPattern.End, Rotation.R180);
        if (E) return (PavingPattern.End, Rotation.R270);
        if (S) return (PavingPattern.End, Rotation.R0);
        return (PavingPattern.End, Rotation.R90);
    }
    
    // ============================================================
    // 7. CENTER (anything that doesn't fit above rules)
    // ============================================================
    return (PavingPattern.Full, Rotation.R0); // Try to ensure connectivity
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
