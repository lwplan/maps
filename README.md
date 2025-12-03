# Maps

Procedural map generation utilities for lightweight roguelike/encounter maps. The library can generate nodes, anneal their spacing, triangulate edges, and render quick visualizations to a bitmap.

## Bitmap rendering

`BitmapMapRenderer` now supports rendering directly from a `GameMap`, preserving the scale established by annealing and the fitted region size.

```csharp
var map = generator.GenerateMap(regionSize, numLevels, minNodesPerLevel, maxNodesPerLevel, bifurcationFactor, minDistance);
var image = BitmapMapRenderer.Render(map, pixelsPerUnit: 3f);
image.Save("/tmp/map.png");
```

Key details:
- Canvas dimensions derive from `map.RegionSize`, so the annealed spacing between nodes remains proportional in the bitmap.
- `pixelsPerUnit` controls pixel density for the region; increase it to magnify distances while retaining the same proportions.
- A configurable margin (via `marginBlocks`) keeps nodes from touching image edges while using the same block aesthetic as before.

For compatibility, the previous `Render(IEnumerable<Node> nodes, ...)` overload remains if you prefer manual normalization to an ASCII-style grid.

## Command-line bitmap rendering

When compiled with the `TESTBITMAP_APP` symbol, `TestBitmap.cs` provides a simple CLI for generating a sample map and writing `/tmp/map.png`.

Available options (defaults in parentheses):

- `--region-width <float>`: map region width before annealing (1).
- `--region-height <float>`: map region height before annealing (1).
- `--num-levels <int>`: number of node layers to generate (4).
- `--min-nodes <int>`: minimum nodes per level (1).
- `--max-nodes <int>`: maximum nodes per level (3).
- `--bifurcation-factor <float>`: bifurcation probability factor between levels (0.5).
- `--min-distance <int>`: optional minimum Manhattan distance between nodes (unset).
- `--pixels-per-unit <float>`: bitmap density relative to annealed units (3).

Example:

```bash
dotnet run -p maps.csproj --testbitmap -- --region-width 2 --region-height 3 \
    --num-levels 6 --min-nodes 2 --max-nodes 4 --bifurcation-factor 0.65 \
    --min-distance 2 --pixels-per-unit 4
```
