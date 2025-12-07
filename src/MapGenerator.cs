using maps.GameMapPipeline;

namespace maps
{
    public static class MapGenerator
    {
        /// <summary>
        /// Runs the entire map generation pipeline and returns a fully built GameMap.
        /// Includes: 
        ///   - Node generation
        ///   - Start/End assignment
        ///   - Biome generation
        ///   - Route rasterization
        ///   - Paving + event region stamping
        ///   - TileInfo[,] construction for Unity
        /// </summary>
        public static GameMap Generate(MapGenParams p)
        {
            // Instantiate the pipeline
            var pipeline = new GameMapPipeline.GameMapPipeline()
                .AddStep(new GenerateRawNodesStep())
                .AddStep(new AssignStartEndStep())
                .AddStep(new ComputeWorldBoundsStep())  // NEW — sets bounds used by ALL tile systems
                .AddStep(new BiomeGenerationStep())     // uses GameMap.TileWidth/Height
                .AddStep(new RasterizeRoutesStep())     // uses same bounds
                .AddStep(new FillPavedRegionsStep())    // uses same bounds
                .AddStep(new TileArrayBuildStep());     // safe and aligned

            // Run pipeline and return result
            return pipeline.Execute(p);
        }
    }
}