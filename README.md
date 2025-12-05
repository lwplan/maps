# Maps

Procedural map generation utilities for lightweight roguelike/encounter maps. The library can generate nodes, anneal their spacing, triangulate edges, and render quick visualizations to a bitmap.

## Bitmap rendering

`BitmapMapRenderer` now expects the map to be pre-scaled. Use `GameMapScaler.ScaleForRendering` to enforce minimum spacing and clamp the canvas before handing the map to the renderer.

```csharp
var map = generator.GenerateMap(regionSize, numLevels, minNodesPerLevel, maxNodesPerLevel, bifurcationFactor, minDistance);
var scaledMap = GameMapScaler.ScaleForRendering(map, pixelsPerUnit: 3f);
var image = BitmapMapRenderer.Render(scaledMap, pixelsPerUnit: 3f);
image.Save("/tmp/map.png");
```

Key details:
- Scaling happens on the map data via `GameMapScaler`, which adjusts node coordinates and region size to satisfy `MinNodeDistance` without relying on the renderer.
- Canvas dimensions derive from `map.RegionSize`, so the annealed spacing between nodes remains proportional in the bitmap.
- `pixelsPerUnit` controls pixel density for the region; increase it to magnify distances while retaining the same proportions.
- A configurable margin (via `marginBlocks`) keeps nodes from touching image edges while using the same block aesthetic as before.

For compatibility, the previous `Render(IEnumerable<Node> nodes, ...)` overload remains if you prefer manual normalization to an ASCII-style grid.

## Command-line bitmap rendering

The `TestBitmap` console app (see `TestBitmap/TestBitmap.csproj`) lives in a separate target that references the core library and SixLabors ImageSharp rendering dependencies. It publishes as a self-contained single-file executable for Linux, macOS, or Windows, but you can also run it directly with `dotnet run` during development.

Available options (defaults in parentheses):

- `--num-levels <int>`: number of node layers to generate (5).
- `--min-nodes <int>`: minimum nodes per level (1).
- `--max-nodes <int>`: maximum nodes per level (3).
- `--bifurcation-factor <float>`: bifurcation probability factor between levels (0.5).
- `--yaml-output <path>`: optional YAML export of the generated node list (no file written if omitted).
- `--png-output <path>`: output path for the rendered map PNG (`map.png`).
- `--seed <int>`: optional RNG seed for repeatable layouts.

Development run example (prints the absolute PNG/YAML paths on completion):

```bash
dotnet run -p TestBitmap/TestBitmap.csproj -- --num-levels 6 --min-nodes 2 --max-nodes 4 \
    --bifurcation-factor 0.65 --png-output ./artifacts/map.png --yaml-output ./artifacts/map.yaml
```

Publish a self-contained executable for your platform (RID must be one of `linux-x64`, `osx-x64`, or `win-x64`):

```bash
dotnet publish TestBitmap/TestBitmap.csproj -c Release -r linux-x64
```

Published binaries land under `TestBitmap/bin/Release/net8.0/<RID>/publish/`.

## Unity plugin publishing

The Unity project consumes the compiled map generation plugin from `UnityProject/Assets/Plugins`. To produce the assemblies locally (built for `netstandard2.1` so Unity can load them), run:

```bash
./scripts/publish-unity-plugin.sh
```

Binary outputs are ignored in version control; publish locally before opening the Unity project to ensure the precompiled references in `UnityProject/Assets/Scripts/MapGen/MapGen.asmdef` resolve correctly.

For examples on calling the generator from Unity and uploading rendered maps with `Texture2D.LoadImage` or `LoadRawTextureData`, see [`docs/Unity.md`](docs/Unity.md).
