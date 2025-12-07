# Scene integration guide: building runtime maps in Unity

This guide explains how to wire the map generation pipeline into Unity scenes and translate the produced `GameMap` data into live GameObjects. It assumes you already published or referenced the generator plugin (see `docs/Unity.md`) and are ready to place prefabs based on nodes and edges.

## High-level flow
1. **Generate or load a `GameMap`** at scene load using `GameMapGeneratorBehaviour` or a custom MonoBehaviour that invokes `GameMapPipeline.Execute` with `MapGenParams`.
2. **Convert logical coordinates to world space** using a consistent tile size and origin so placement is deterministic across sessions.
3. **Instantiate floor/wall/connectors** per node/edge, retaining a lookup table from `Node.Id` to spawned objects for later decoration or cleanup.
4. **Decorate with metadata** such as biomes, spawn markers, or per-level variations once the base layout exists.
5. **Tear down or rebuild** when the player restarts a run or when design tweaks require regenerating the map.

## Preparing the scene
- Create a dedicated GameObject (e.g., `MapRuntime`) with the following components:
  - **`GameMapGeneratorBehaviour`** – exposes `MapSource` (runtime generation vs. baked asset) and the serialized `MapGenParams` fields. Keep this script as the single source of truth for the active `GameMap` instance.
  - **`MapPlacer` (new MonoBehaviour)** – subscribes to generator events or polls `GameMapGeneratorBehaviour.GeneratedMap` after `Start()` to build the scene. Place this alongside the generator for clarity.
- Add prefab references for floors, walls, connectors, and optional biome variants to the placer component via serialized fields.
- Define a **tile size** (e.g., `Vector2 tileSize = new Vector2(2f, 2f);`) and a world **origin** (e.g., `Vector3.zero` or a staging offset). All placement uses these constants to keep edge spacing uniform.

## Driving generation
Use the generator shim to produce a map when the scene loads or when a restart occurs.

```csharp
using maps;
using UnityEngine;

public class MapRuntimeController : MonoBehaviour
{
    [SerializeField] private GameMapGeneratorBehaviour generator;
    [SerializeField] private MapPlacer placer;

    private void Awake()
    {
        generator.OnMapGenerated += HandleMapGenerated; // hook before Start() fires
    }

    private void Start()
    {
        generator.Generate(); // triggers runtime generation or loads the baked asset
    }

    private void HandleMapGenerated(GameMap map)
    {
        placer.Build(map);
    }
}
```

### Runtime vs. baked assets
- **Runtime generation (default):** `GameMapGeneratorBehaviour` builds the pipeline and returns a fresh `GameMap` on demand.
- **Baked reuse:** Assign a `GameMapAsset` ScriptableObject and set `MapSource` to `UseBakedAsset` to load a pre-generated map. This keeps play mode predictable while still letting designers regenerate as needed.
- Always handle the **fallback case** where no asset is present by regenerating at runtime; this prevents broken scenes when assets are missing.

## Converting coordinates to world space
`Node.Position` is a logical 2D coordinate expressed in map units. Convert it to Unity space as follows:

```csharp
Vector3 ToWorld(Node node, Vector2 tileSize, Vector3 origin)
{
    return origin + new Vector3(node.Position.X * tileSize.x, 0f, node.Position.Y * tileSize.y);
}
```

Guidelines:
- Keep **uniform spacing** between nodes and honor `MinDistance` used during generation. The same `tileSize` should be used for edges so connectors align.
- If you scale the map for visuals (e.g., `GameMapScaler.ScaleForRendering`), apply equivalent scaling to the world tile size.
- Consider a **height function** (e.g., `y = levelIndex * elevationStep`) to separate layers when `Node.Level` indicates vertical progression.

## Placing content from nodes and edges
Implement a `MapPlacer.Build(GameMap map)` method that iterates nodes and edges:

```csharp
public class MapPlacer : MonoBehaviour
{
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject connectorPrefab;
    [SerializeField] private Vector2 tileSize = new Vector2(2f, 2f);
    [SerializeField] private Vector3 origin = Vector3.zero;

    private readonly Dictionary<int, GameObject> spawnedByNode = new();

    public void Build(GameMap map)
    {
        Clear();

        foreach (var node in map.Nodes)
        {
            var worldPos = ToWorld(node, tileSize, origin);
            var go = Instantiate(floorPrefab, worldPos, Quaternion.identity, transform);
            spawnedByNode[node.Id] = go;
        }

        foreach (var edge in map.Edges)
        {
            var from = map.Nodes[edge.From];
            var to = map.Nodes[edge.To];
            var midpoint = (ToWorld(from, tileSize, origin) + ToWorld(to, tileSize, origin)) * 0.5f;
            Instantiate(connectorPrefab, midpoint, Quaternion.identity, transform);
        }
    }

    public void Clear()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        spawnedByNode.Clear();
    }

    private static Vector3 ToWorld(Node node, Vector2 tileSize, Vector3 origin)
    {
        return origin + new Vector3(node.Position.X * tileSize.x, 0f, node.Position.Y * tileSize.y);
    }
}
```

Notes:
- Maintain `spawnedByNode` so later systems (AI spawners, loot, VFX) can attach to specific nodes by ID.
- Use the `GameMap` edge list to place connectors, bridges, or doors. The example uses a midpoint, but you can also align rotation and scale by subtracting node positions.
- Keep placement deterministic by seeding any random decoration with the same seed used in `MapGenParams`.

## Biomes, metadata, and extensions
- **Biomes:** `BiomeMap` entries can select prefab palettes. For each node, read `map.BiomeMap?.BiomeAt(node.Position)` (or your helper) to choose materials or lighting.
- **Level metadata:** Use `Node.Level` to gate encounters or elevate floors for multi-level scenes.
- **Spawn markers:** After base placement, run a pass that inspects node tags or IDs to drop enemies, treasure, or interactables.
- **Debugging:** Draw gizmos for node positions and edges to verify spacing before committing art-heavy prefabs.

## Lifecycle and cleanup
- Call `MapPlacer.Clear()` before rebuilding to avoid orphaned objects.
- When unloading a scene, unsubscribe from generator events to prevent leaks (`generator.OnMapGenerated -= HandleMapGenerated`).
- Provide an in-editor context menu or hotkey to regenerate the map during play mode for rapid iteration.

## Quick checklist
- [ ] Generator plugin or source available to Unity (`MapGenParams`, `GameMapPipeline`, `GameMap` types resolve).
- [ ] `GameMapGeneratorBehaviour` present in the scene and configured for runtime or baked input.
- [ ] `MapPlacer` (or equivalent) instantiated and wired to generator events.
- [ ] Consistent tile size/origin defined; coordinate conversion verified.
- [ ] Prefab references assigned for floors, connectors, and biome variants.
- [ ] Cleanup path ensures old runs are destroyed before rebuilding.

With these pieces in place, the map builder becomes a self-contained scene service: generate or load a `GameMap`, translate coordinates to world space, spawn prefabs per node/edge, and decorate using metadata for a deterministic, reproducible layout.
