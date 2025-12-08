# Maps

Procedural map generation utilities for lightweight roguelike/encounter maps. The library can generate nodes, anneal their spacing, triangulate edges, and render quick visualizations to a bitmap.

## Architecture

- **Core generator (maps library, `netstandard2.1`)** – produces graph geometry and metadata only. All consumers (Unity plugin, CLI renderer) reference this assembly. Dependencies: none beyond the BCL.
- **Unity plugin (generation-only)** – precompiled `netstandard2.1` assemblies that Unity loads from `UnityProject/Assets/Plugins`. No third-party image libraries are included so the plugin remains lightweight and IL2CPP-friendly. Rendering must be handled by Unity-side code if needed.
- **Renderer CLI (desktop-only)** – the `TestBitmap` console app that links the generator with SixLabors ImageSharp to produce PNGs/YAML exports for desktop workflows.

## Component setup and dependencies

- **Core generator**: Build or test the shared library with `dotnet build maps.csproj` or `dotnet test`. No external dependencies are required.
- **Unity plugin**: Publish the generator-only plugin for Unity with `./scripts/publish-unity-plugin.sh`, which outputs `netstandard2.1` assemblies under `UnityProject/Assets/Plugins`. Keep the plugin free of third-party image libraries to avoid editor bloat and ensure IL2CPP compatibility. At runtime, handle any texture upload or rendering via Unity APIs in your scripts.
- **Renderer CLI**: Run or publish the desktop-only console app with `dotnet run -p TestBitmap/TestBitmap.csproj -- ...` or `dotnet publish TestBitmap/TestBitmap.csproj -c Release -r <RID>`. This target depends on SixLabors ImageSharp; those dependencies do **not** apply to the Unity plugin.

## Bitmap rendering

`BitmapMapRenderer` (used by the renderer CLI) expects the map to be pre-scaled. Use `GameMapScaler.ScaleForRendering` to enforce minimum spacing and clamp the canvas before handing the map to the renderer.

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

## Chunked tile streaming

`ChunkBuilder` exposes a lightweight worker thread that can build `TileInfo` blocks in map-aligned chunks without blocking your main thread. The generator still emits the full `TileInfo[,]` synchronously for existing flows, but you can also stream tiles lazily for runtime worlds.

- The service is wired up automatically in `MapGenerator.Generate` and seeds its request queue with the chunk that contains the start node.
- Chunk coordinates are expressed in world tile space; the helper `ChunkBuilder.GetChunkCoordForTile(tile, chunkSize)` floors the coordinate so negative tiles land in the expected chunk.
- Call `RequestChunkForTile` or `RequestChunk` for neighboring regions, then poll `TryDequeueBuiltChunk` from your update loop. Each result carries the chunk coordinates plus the trimmed `TileInfo[,]` payload.
- Dispose the builder (or call `Cancel`) when shutting down to release the worker thread and wake any waiters.

Example usage during gameplay:

```csharp
var map = MapGenerator.Generate(parameters);
var chunkBuilder = map.ChunkBuilder!; // initialized during Generate

// Request neighbors around the start node's chunk
chunkBuilder.RequestChunkForTile(map.StartNode.TileX + ChunkBuilder.DefaultChunkSize, map.StartNode.TileY);
chunkBuilder.RequestChunkForTile(map.StartNode.TileX - ChunkBuilder.DefaultChunkSize, map.StartNode.TileY);

// In your main loop
if (chunkBuilder.TryDequeueBuiltChunk(out var built))
{
    UploadTilesToUnity(built.ChunkX, built.ChunkY, built.Tiles);
}
```

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
