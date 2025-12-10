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
    /// Indicates if this pattern has rotation-specific assets supplied.
    /// </summary>
    public bool HasRotationOverride(PavingPattern pattern)
    {
        return _hasRotationOverrides.Contains(pattern);
    }

    public int GetAtlasIndex(PavingPattern tPavingPattern, BiomeType tBiome)
    {
        switch (tPavingPattern)
        {
            case PavingPattern.None:
                return 13;
            case PavingPattern.Center:
                return 10;
            case PavingPattern.End:
                return 9;
            case PavingPattern.Straight:
                return 8;
            case PavingPattern.Corner:
                return 14;
            case PavingPattern.TJunction:
                return 4;
            case PavingPattern.Cross:
                return 5;
            case PavingPattern.Full:
                return 0;
            case PavingPattern.EdgeStrip:
                return 15;
            case PavingPattern.InnerCorner:
                return 6;
            case PavingPattern.ChamferedEdge:
                return 12;
            case PavingPattern.OuterCorner:
                return 11;
            default:
                throw new ArgumentOutOfRangeException(nameof(tPavingPattern), tPavingPattern, null);
        }
    }
}
