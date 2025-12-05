using System;
using System.Collections.Generic;
using System.Linq;
using maps;
using UnityEngine;
using NumericsVector2 = System.Numerics.Vector2;
using UnityVector2 = UnityEngine.Vector2;

[CreateAssetMenu(fileName = "GameMapAsset", menuName = "Maps/Game Map Asset", order = 0)]
public class GameMapAsset : ScriptableObject
{
    [Serializable]
    public struct NodeRecord
    {
        public int TileX;
        public int TileY;
        public int Level;
        public NodeType Type;
        public int[] NextNodeIndices;
    }

    [Serializable]
    public struct BiomeRecord
    {
        public int Width;
        public int Height;
        public int OffsetX;
        public int OffsetY;
        public List<int> Values;
    }

    [Header("Map metadata")]
    [SerializeField] private int numLevels = 1;
    [SerializeField] private int minNodesPerLevel = 1;
    [SerializeField] private int maxNodesPerLevel = 1;
    [SerializeField] private float bifurcationFactor = 0f;
    [SerializeField] private bool hasMinNodeDistance = false;
    [SerializeField] private int minNodeDistance = 0;
    [SerializeField] private UnityVector2 regionSize = UnityVector2.one;

    [Header("Map data")] 
    [SerializeField] private int startNodeIndex = -1;
    [SerializeField] private int endNodeIndex = -1;
    [SerializeField] private List<NodeRecord> nodes = new();
    [SerializeField] private BiomeRecord? biomeData = null;

    public GameMap ToGameMap()
    {
        var map = new GameMap(numLevels, minNodesPerLevel, maxNodesPerLevel, bifurcationFactor)
        {
            RegionSize = new NumericsVector2(regionSize.x, regionSize.y),
            MinNodeDistance = hasMinNodeDistance ? minNodeDistance : null
        };

        // Materialize nodes
        var nodeInstances = new List<Node>();
        foreach (var record in nodes)
        {
            var node = new Node(record.TileX, record.TileY, record.Level, record.Type, combatEncounter: null);
            nodeInstances.Add(node);
        }

        // Wire up edges
        for (var i = 0; i < nodes.Count; i++)
        {
            var record = nodes[i];
            var source = nodeInstances[i];
            if (record.NextNodeIndices == null)
            {
                continue;
            }

            foreach (var nextIndex in record.NextNodeIndices)
            {
                if (nextIndex < 0 || nextIndex >= nodeInstances.Count)
                {
                    Debug.LogWarning($"GameMapAsset: next node index {nextIndex} out of bounds");
                    continue;
                }

                var target = nodeInstances[nextIndex];
                source.NextLevelNodes.Add(target);
                target.PrevLevelNodes.Add(source);
            }
        }

        map.Nodes = nodeInstances;
        map.StartNode = (startNodeIndex >= 0 && startNodeIndex < nodeInstances.Count) ? nodeInstances[startNodeIndex] : null;
        map.EndNode = (endNodeIndex >= 0 && endNodeIndex < nodeInstances.Count) ? nodeInstances[endNodeIndex] : null;

        if (biomeData.HasValue && biomeData.Value.Values != null && biomeData.Value.Values.Count == biomeData.Value.Width * biomeData.Value.Height)
        {
            var data = biomeData.Value;
            var biomeMap = new BiomeMap(data.Width, data.Height, data.OffsetX, data.OffsetY);
            for (var y = 0; y < data.Height; y++)
            {
                for (var x = 0; x < data.Width; x++)
                {
                    var index = y * data.Width + x;
                    biomeMap[x, y] = (Biome)data.Values[index];
                }
            }

            map.Biomes = biomeMap;
        }

        return map;
    }

    public void PopulateFrom(GameMap map)
    {
        if (map == null)
        {
            throw new ArgumentNullException(nameof(map));
        }

        numLevels = map.NumLevels;
        minNodesPerLevel = map.MinNodesPerLevel;
        maxNodesPerLevel = map.MaxNodesPerLevel;
        bifurcationFactor = map.BifurcationFactor;
        hasMinNodeDistance = map.MinNodeDistance.HasValue;
        minNodeDistance = map.MinNodeDistance ?? 0;
        regionSize = new UnityVector2(map.RegionSize.X, map.RegionSize.Y);

        nodes = new List<NodeRecord>(map.Nodes.Count);
        var indexLookup = new Dictionary<Node, int>();
        for (var i = 0; i < map.Nodes.Count; i++)
        {
            indexLookup[map.Nodes[i]] = i;
        }

        foreach (var node in map.Nodes)
        {
            var nextIndices = node.NextLevelNodes.Select(n => indexLookup.GetValueOrDefault(n, -1)).Where(i => i >= 0).ToArray();
            nodes.Add(new NodeRecord
            {
                TileX = node.TileX,
                TileY = node.TileY,
                Level = node.Level,
                Type = node.Type,
                NextNodeIndices = nextIndices
            });
        }

        startNodeIndex = map.StartNode != null && indexLookup.ContainsKey(map.StartNode) ? indexLookup[map.StartNode] : -1;
        endNodeIndex = map.EndNode != null && indexLookup.ContainsKey(map.EndNode) ? indexLookup[map.EndNode] : -1;

        biomeData = null;
        if (map.Biomes != null)
        {
            var values = new List<int>(map.Biomes.Width * map.Biomes.Height);
            for (var y = 0; y < map.Biomes.Height; y++)
            {
                for (var x = 0; x < map.Biomes.Width; x++)
                {
                    values.Add((int)map.Biomes[x, y]);
                }
            }

            biomeData = new BiomeRecord
            {
                Width = map.Biomes.Width,
                Height = map.Biomes.Height,
                OffsetX = map.Biomes.OffsetX,
                OffsetY = map.Biomes.OffsetY,
                Values = values
            };
        }
    }
}
