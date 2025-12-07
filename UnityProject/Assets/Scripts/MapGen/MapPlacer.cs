using System.Collections.Generic;
using maps;
using UnityEngine;

public class MapPlacer : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject connectorPrefab;
    [SerializeField] private GameObject startNodePrefab;
    [SerializeField] private GameObject endNodePrefab;

    [Header("Placement settings")]
    [SerializeField] private Vector2 tileSize = new(2f, 2f);
    [SerializeField] private Vector3 origin = Vector3.zero;
    [SerializeField] private float levelHeightStep = 1.5f;
    [SerializeField, Tooltip("Thickness used for fallback primitives when no prefabs are provided.")]
    private float fallbackThickness = 0.35f;

    private readonly Dictionary<Node, GameObject> spawnedByNode = new();

    public void Build(GameMap map)
    {
        Clear();

        if (map == null)
        {
            Debug.LogWarning("MapPlacer: no map supplied to build.");
            return;
        }

        for (var i = 0; i < map.Nodes.Count; i++)
        {
            var node = map.Nodes[i];
            var prefab = SelectPrefab(node);
            var spawned = SpawnNode(prefab, node, i);
            spawnedByNode[node] = spawned;
        }

        foreach (var node in map.Nodes)
        {
            foreach (var next in node.NextLevelNodes)
            {
                PlaceConnector(node, next);
            }
        }
    }

    public void Clear()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(child.gameObject);
                continue;
            }
#endif
            Destroy(child.gameObject);
        }

        spawnedByNode.Clear();
    }

    private GameObject SelectPrefab(Node node)
    {
        return node.Type switch
        {
            NodeType.Start => startNodePrefab != null ? startNodePrefab : nodePrefab,
            NodeType.End => endNodePrefab != null ? endNodePrefab : nodePrefab,
            _ => nodePrefab
        };
    }

    private GameObject SpawnNode(GameObject prefab, Node node, int index)
    {
        var world = ToWorld(node);
        var rotation = Quaternion.identity;

        if (prefab != null)
        {
            return Instantiate(prefab, world, rotation, transform);
        }

        var fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fallback.name = $"Node_{index}_{node.Type}";
        fallback.transform.SetParent(transform, worldPositionStays: false);
        fallback.transform.SetPositionAndRotation(world, rotation);
        fallback.transform.localScale = new Vector3(tileSize.x * 0.5f, fallbackThickness, tileSize.y * 0.5f);
        return fallback;
    }

    private void PlaceConnector(Node from, Node to)
    {
        var fromPos = ToWorld(from);
        var toPos = ToWorld(to);
        var direction = toPos - fromPos;

        if (direction == Vector3.zero)
        {
            return;
        }

        var midpoint = (fromPos + toPos) * 0.5f;
        var rotation = Quaternion.LookRotation(direction, Vector3.up);

        if (connectorPrefab != null)
        {
            Instantiate(connectorPrefab, midpoint, rotation, transform);
            return;
        }

        var fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fallback.name = $"Connector_{from.Type}_to_{to.Type}";
        fallback.transform.SetParent(transform, worldPositionStays: false);
        fallback.transform.SetPositionAndRotation(midpoint, rotation);

        var length = direction.magnitude;
        fallback.transform.localScale = new Vector3(fallbackThickness, fallbackThickness * 0.6f, length);
    }

    private Vector3 ToWorld(Node node)
    {
        var height = node.Level * levelHeightStep;
        return origin + new Vector3(node.TileX * tileSize.x, height, node.TileY * tileSize.y);
    }
}
