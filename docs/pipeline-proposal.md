# GameMap pipeline proposal

This proposal treats `GameMap` as the canonical model produced by a deterministic pipeline. The pipeline emphasizes geometric fidelity (planarity and relative distances), grid-aligned quantization for downstream systems, and optional rendering as a derived artifact.

## Goals
- Preserve topology: adjacency, planarity, and ordering of regions/levels.
- Minimize metric distortion: keep edge lengths and shortest paths close to pre-quantized values.
- Produce grid-aligned coordinates suitable for discrete systems (AI, pathfinding, tile maps).
- Keep rendering concerns downstream and lossless relative to the pipeline output.

## Proposed pipeline
1. **Input assembly**: collect seed parameters (region size, level counts, bifurcation factors, min distance) and semantic tags for nodes/regions.
2. **Node generation & annealing**: generate layered nodes (`GameMapGenerator`, `NodeGenerator`) and run spacing/annealing to respect min-distance and boundary constraints.
3. **Triangulated graph construction**: build a constrained triangulation (`Triangulator`) that preserves level adjacency and avoids crossings; tag edges by type (vertical/horizontal/diagonal, region boundaries).
4. **Scaling & quantization**: normalize coordinates to a chosen grid resolution, then snap nodes to integers while optimizing for low distortion.
5. **Validation pass**: assert planarity, edge length bounds, and semantic invariants; compute distortion metrics for reporting.
6. **Outputs & artifacts**: emit the finalized `GameMap` (grid-aligned), adjacency matrices, and optional renders (bitmap/ASCII) derived from the pipeline output.

## Step details and considerations
### 1) Node generation & annealing
- Use the existing generator to create layered nodes and bifurcations, emitting grid-aligned integer coordinates.
- The generator scales and quantizes positions so the horizontal and vertical spacing between nodes exceeds the configured minimum, capturing iteration stats if spacing refinement runs.

### 2) Triangulated graph construction
- Apply constrained triangulation so that edges do not cross existing region boundaries or semantic obstacles.
- Prefer near-Delaunay characteristics for uniformity, but allow overrides to enforce level-to-level connectivity.
- Record edge classes (structural vs. decorative) to help downstream pruning or rendering.

### 3) Scaling & quantization
- Choose a target grid size (e.g., width/height in integer units) based on region extents and minimum spacing requirements.
- Apply an affine transform to fit the annealed coordinates into the grid bounds while preserving aspect ratio.
- Optimize snapping by minimizing a weighted error function: preserve edge lengths/angles, prevent node overlap, and maintain planarity.
- Enforce constraints during snapping: no zero-length edges, no duplicate node positions, and respect reserved cells for obstacles or metadata.

### 4) Validation pass
- Verify planarity after quantization; rerun a lightweight crossing check on edges.
- Calculate per-edge and aggregate distortion (e.g., mean/95th-percentile relative error) and surface them in logs.
- Confirm semantic invariants (e.g., spawn points remain in their regions; level ordering is preserved).

### 5) Outputs & artifacts
- The canonical artifact is the quantized `GameMap`, ready for pathfinding or simulation.
- Emit adjacency matrices or serialized edge lists for AI consumption.
- Rendering (bitmap/ASCII) consumes the quantized map without altering it, ensuring visual outputs reflect the canonical model.

## Open questions to finalize the pipeline
- Target grid policy: fixed resolution (e.g., 128x128) vs. adaptive per map?
- Distortion tolerance: acceptable per-edge and per-path error thresholds after snapping?
- Obstacles/reservations: do specific grid cells need to remain empty or tagged?
- Metadata handling: which semantic labels must survive quantization (spawn, loot, zone types)?
- Performance budget: constraints on annealing/optimization iterations for runtime vs. batch generation?
