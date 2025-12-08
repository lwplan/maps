namespace maps.Map3D
{
    [System.Flags]
    public enum Neighbor4 : byte
    {
        None  = 0,
        North = 1 << 0,
        East  = 1 << 1,
        South = 1 << 2,
        West  = 1 << 3
    }

    [System.Flags]
    public enum Neighbor8 : ushort
    {
        None      = 0,
        North     = 1 << 0,
        NorthEast = 1 << 1,
        East      = 1 << 2,
        SouthEast = 1 << 3,
        South     = 1 << 4,
        SouthWest = 1 << 5,
        West      = 1 << 6,
        NorthWest = 1 << 7
    }
    
    public enum PavingPattern : byte
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
