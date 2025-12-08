using System;
using System.Collections.Generic;
using UnityEngine;
using maps.Map3D;

[CreateAssetMenu(fileName = "TilePrefabRegistry", menuName = "Map3D/Tile Prefab Registry")]
public class TilePrefabRegistry : ScriptableObject
{
    [Serializable]
    public class TilePrefabEntry
    {
        public PavingPattern Pattern;
        public Rotation Rotation;
        public BiomeType Biome;      // Optional for future use
        public GameObject Prefab;
    }

    [Header("Default ground/fallback material")]
    public Material DefaultMaterial;

    [Header("Tile prefab mappings")]
    public List<TilePrefabEntry> Entries = new();

    private Dictionary<(PavingPattern, Rotation, BiomeType), GameObject> _lookup;

    void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        _lookup = new Dictionary<(PavingPattern, Rotation, BiomeType), GameObject>();

        foreach (var e in Entries)
        {
            var key = (e.Pattern, e.Rotation, e.Biome);
            if (!_lookup.ContainsKey(key))
                _lookup.Add(key, e.Prefab);
        }
    }

    /// <summary>
    /// Get a prefab for a tile. Biome is optional for future phases.
    /// </summary>
    public GameObject GetPrefab(PavingPattern pattern, Rotation rot, BiomeType biome)
    {
        // Try exact match (pattern + rotation + biome)
        if (_lookup.TryGetValue((pattern, rot, biome), out var prefab))
            return prefab;

        // Fallback: ignore biome
        if (_lookup.TryGetValue((pattern, rot, default), out prefab))
            return prefab;

        return null;
    }


}