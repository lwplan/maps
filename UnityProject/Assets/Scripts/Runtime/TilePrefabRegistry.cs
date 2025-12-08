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
        public Rotation Rotation;       // Optional override — usually R0
        public BiomeType Biome;
        public GameObject Prefab;
    }

    [Header("Default ground/fallback material")]
    public Material DefaultMaterial;

    [Header("Tile prefab mappings")]
    public List<TilePrefabEntry> Entries = new();

    private Dictionary<PavingPattern, GameObject> _patternLookup;
    private HashSet<PavingPattern> _hasRotationOverrides = new();

    void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        _patternLookup = new Dictionary<PavingPattern, GameObject>();

        foreach (var e in Entries)
        {
            // Register default prefab for this pattern (first entry wins)
            if (!_patternLookup.ContainsKey(e.Pattern))
                _patternLookup[e.Pattern] = e.Prefab;

            // Track if user explicitly provided rotated prefab variations
            if (e.Rotation != Rotation.R0)
                _hasRotationOverrides.Add(e.Pattern);
        }
    }

    /// <summary>
    /// Returns a prefab for the given pattern. Rotation is applied at mesh combine time.
    /// </summary>
    public GameObject GetPrefab(PavingPattern pattern, Rotation rot, BiomeType biome)
    {
        // Everything currently defaults to desert
        if (biome != BiomeType.Desert)
        {
            // Debug.LogWarning($"Biome {biome} not supported yet. Using Desert prefab.");
        }

        // Basic lookup (pattern only)
        if (_patternLookup.TryGetValue(pattern, out var prefab))
            return prefab;

        // Missing pattern
        Debug.LogError($"TilePrefabRegistry: No prefab found for pattern {pattern}. " +
                       $"(Rotation {rot} requested, but registry stores pattern-only.)");

        return null;
    }

    /// <summary>
    /// Indicates if this pattern has rotation-specific assets supplied.
    /// </summary>
    public bool HasRotationOverride(PavingPattern pattern)
    {
        return _hasRotationOverrides.Contains(pattern);
    }
}
