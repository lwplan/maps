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
