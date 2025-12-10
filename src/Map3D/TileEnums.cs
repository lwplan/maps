namespace maps.Map3D
{


    [System.Flags]
    public enum Neighbor8 : ushort
    {
        None      = 0,
        Center    = 1 << 0,
        North     = 1 << 1,
        NorthEast = 1 << 2,
        East      = 1 << 3,
        SouthEast = 1 << 4,
        South     = 1 << 5,
        SouthWest = 1 << 6,
        West      = 1 << 7,
        NorthWest = 1 << 8
    }
    
    public enum PavingPattern : int
    {
        None,
        Center,
        End,
        Straight,
        Corner,
        TJunction,
        Cross,
        Full,
        EdgeStrip,
        InnerCorner,
        ChamferedEdge,
        OuterCorner
    }

    public enum Rotation : byte
    {
        R0   = 0,
        R90  = 1,
        R180 = 2,
        R270 = 3
    }

    public enum ElevationPattern : byte
    {
        Flat,
        RaisedCorner,
        LoweredCorner,
        RaisedEdge,
        LoweredEdge
    }

    public enum BiomeType : byte
    {
        Desert,
        Dune,
        Canyon,
        Grassland,
        Mountain,
        Sea,
        Coast,
        City,
        Ruins,
        Dungeon,
        Cave
    }
}
