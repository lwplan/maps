# Unity integration plan: procedural maps + custom runtime builder

## Current state
- **Procedural generator:** The C# map generator library lives under `src/` (for example, `GameMapPipeline/GameMapPipeline.cs` builds a `GameMap` via pluggable steps). It runs outside Unity today and emits logical nodes, edges, and region metadata.
- **Unity shim:** `GameMapGeneratorBehaviour` generates maps at runtime or loads them from `GameMapAsset` ScriptableObjects. The assembly definition under `Assets/Scripts/MapGen` references the generator plugin but no longer depends on Tile World Creator.
- **Legacy tiles:** The `UnityProject/Assets/TileW` directory contains older prefabs/scenes. Tile World Creator assets remain in the repo as reference-only and should not be invoked by new runtime code.

## Goal
Drive scene construction directly from `GameMap` data using bespoke placement code and prefabs, replacing the previous Tile World Creator bridge.

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
   - Implement a `Generate()` method that constructs `GameMapPipeline`, calls `Execute`, and stores the resulting `GameMap` for downstream placement scripts.
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

### 2) Build a custom runtime placer
- Define how logical coordinates map to scene space (tile size, elevation, node spacing) so instantiation can be deterministic.
- Create a simple placement component that iterates nodes and edges from `GameMap` and instantiates prefabs/meshes for floors, walls, and connectors without invoking third-party tools.
- Keep a mapping from logical `Node` to spawned objects for future decoration, interaction hooks, or cleanup.

### 3) Replace legacy assets incrementally
- Start with lightweight placeholder meshes or gizmos to visualize the generated topology.
- Swap in production prefabs over time, using per-biome variants when `BiomeMap` data is available.
- Retire or migrate the `UnityProject/Assets/TileW` prefabs/scenes once the new placer covers the same functionality.

### 4) Biomes and metadata
- Use `BiomeMap` data from the generator to select prefab sets, materials, or lighting variants per region.
- Surface hooks for spawn markers or interactables: after placement, iterate node metadata to drop gameplay prefabs (enemies, chests) based on node tags.

### 5) Testing and validation
- Editor workflow: create an editor window or inspector button to run the generator and rebuild the TWC map in the currently open scene for quick iteration.
- Runtime validation: after build layers finish, compare painted cell counts to expected counts from `GameMap` to catch translation errors; optionally visualize debug gizmos.
- Add regression tests (play mode or editor) that generate small maps and assert TWC layers/build outputs exist before scenes load.

## Deliverables
- Unity assembly definition or import pipeline for the generator code.
- A runtime placement component that converts `GameMap` data into instantiated prefabs/meshes.
- Updated scenes/prefabs using the bespoke placer (legacy `TileW` and TWC assets deprecated for runtime).
- Documentation explaining coordinate conventions and how to trigger generation in editor/runtime.
