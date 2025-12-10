namespace maps.Map3D 
{
    
    public struct TileInfo
    {
        public bool IsPaved;      
        public bool IsEventNode;  

        public BiomeType Biome;
        public int ElevationLevel;


        public Neighbor8 PavingMask8;
        public PavingPattern PavingPattern;
        public Rotation Rotation;

        public ElevationPattern ElevationPattern;
    }
}
