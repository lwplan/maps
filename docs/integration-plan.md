# Unity integration plan: procedural maps + Tile World Creator

## Current state
- **Procedural generator:** The C# map generator library lives under `src/` (for example, `GameMapPipeline/GameMapPipeline.cs` builds a `GameMap` via pluggable steps). It runs outside Unity today and emits logical nodes, edges, and region metadata.
- **Tile World Creator (TWC):** The Unity project under `UnityProject/Assets/TileWorldCreator` includes the runtime `TileWorldCreatorManager` component and configuration assets for blueprint/build layers. The manager already exposes entry points such as `GenerateCompleteMap()`, `ExecuteBlueprintLayers()`, and `ExecuteBuildLayers()` to drive tile placement.
- **Legacy tiles:** The `UnityProject/Assets/TileW` directory contains older prefabs/scenes and a `TileMapDefinition` asset that are not aligned with the packaged TWC "Tiles URP" presets.

## Goal
Feed procedurally generated `GameMap` data into Tile World Creator so it can paint blueprint layers and build a Unity tilemap/mesh at runtime, replacing the outdated `TileW` assets.

## Integration milestones
### 1) Share map data with Unity
- Export the generator as a Unity-friendly assembly (e.g., `.dll` built from `maps.csproj` or source files pulled into an assembly definition) so Unity scripts can reference `GameMap`, `Node`, and pipeline steps.
- Add a small Unity shim (MonoBehaviour or ScriptableObject) that accepts `MapGenParams` and orchestrates `GameMapPipeline.Execute` to produce a `GameMap` instance in play mode.
- Decide on serialization: transient in-memory for runtime generation vs. optional asset serialization (e.g., ScriptableObject holding a baked `GameMap`).

#### Steps to execute this milestone
1. **Build or import the generator assembly**
   - In `maps.csproj`, confirm the target framework and ensure the `GameMapPipeline` and related namespaces are public.
   - Compile to a `.dll` and copy it into `UnityProject/Assets/Plugins` _or_ add the generator source files into a Unity assembly definition under `UnityProject/Assets/Scripts/MapGen` so Unity can reference them.
   - In Unity, verify the assembly definition includes references to any external dependencies already bundled with `maps.csproj`.
2. **Create a runtime shim**
   - Add a new `MonoBehaviour` (e.g., `GameMapGeneratorBehaviour.cs`) in `UnityProject/Assets/Scripts/MapGen` that exposes serialized fields for `MapGenParams` (size, seeds, biome options) and holds a `GameMapPipeline` instance.
   - Implement a `Generate()` method that constructs `GameMapPipeline`, calls `Execute`, and stores the resulting `GameMap` for downstream Tile World Creator usage.
   - Add editor buttons or a context menu method to trigger `Generate()` in play mode for quick validation.
3. **Decide on data lifetime and serialization**
   - Start with transient, in-memory `GameMap` instances to unblock runtime integration.
   - If baked assets are needed, create a `ScriptableObject` (e.g., `GameMapAsset`) that can serialize minimal `GameMap` data (nodes, edges, region metadata) to disk for reuse in scenes.
   - Document the chosen approach in project README notes and ensure the shim can load either live-generated or baked data.

#### Current runtime shim decisions (implemented)
- **Default: transient generation.** `GameMapGeneratorBehaviour` continues to build a pipeline and generates maps in memory at runtime.
- **Optional baked reuse:** `GameMapAsset` (ScriptableObject) serializes the minimum viable `GameMap` payload:
  - nodes (tile coordinates, level, type)
  - directed edges (next-level node indices)
  - start/end node indices
  - region metadata (region size, optional min node distance, optional biome map grid)
- **Interchangeability:** `GameMapGeneratorBehaviour` exposes a `MapSource` toggle. When set to **UseBakedAsset**, the shim pulls a `GameMap` from the assigned asset; otherwise it builds a fresh map via the pipeline. A missing asset automatically falls back to runtime generation to keep play mode unblocked.
- **Authoring baked data:** call `GameMapAsset.PopulateFrom(GameMap map)` from an editor script to capture a generated map into an asset for reuse in scenes or tests.

### 2) Translate `GameMap` to TWC blueprint layers
- Choose a blueprint layer schema (e.g., one layer per biome or per node type). Use `TileWorldCreatorManager.ResetConfiguration()` before painting.
- Convert node positions to grid cells in the active configuration. Honor `map.RegionSize` and `MinNodeDistance` so painted cells align with TWC's cell size.
- For each node/edge, call the appropriate blueprint layer API to mark occupied cells (e.g., floor, wall, path). Keep a mapping from logical `Node` to painted cells for later decoration.
- After painting, trigger `ExecuteBlueprintLayers()` followed by `ExecuteBuildLayers(ExecutionMode.FromScratch)` to spawn meshes/tiles.

### 3) Tile set alignment and replacement of legacy assets
- Audit `UnityProject/Assets/TileWorldCreator/"Tiles URP"` presets and associate them with the blueprint layers used above (paths vs. walls vs. void).
- Retire or migrate the `UnityProject/Assets/TileW` prefabs/scenes; point new scenes at the TWC configuration assets instead of the outdated `TileMapDefinition`.
- Add minimal sample scenes demonstrating runtime generation and the updated tile presets.

### 4) Biomes and metadata
- Use `BiomeMap` data from the generator to pick blueprint layers or clusters per biome. Map biome IDs to TWC cluster identifiers so decorations change per region.
- Surface hooks for spawn markers or interactables: after `ExecuteBuildLayers`, iterate the painted cell mapping and place Unity prefabs (enemies, chests) based on node tags.

### 5) Testing and validation
- Editor workflow: create an editor window or inspector button to run the generator and rebuild the TWC map in the currently open scene for quick iteration.
- Runtime validation: after build layers finish, compare painted cell counts to expected counts from `GameMap` to catch translation errors; optionally visualize debug gizmos.
- Add regression tests (play mode or editor) that generate small maps and assert TWC layers/build outputs exist before scenes load.

## Deliverables
- Unity assembly definition or import pipeline for the generator code.
- A runtime component that bridges `GameMap` → TWC blueprint layers → built tiles.
- Updated scenes/prefabs using TWC presets (legacy `TileW` assets deprecated).
- Documentation explaining coordinate conventions and how to trigger generation in editor/runtime.
