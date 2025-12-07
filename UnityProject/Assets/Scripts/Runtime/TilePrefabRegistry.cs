using System.Collections.Generic;
using maps.Map3D;
using UnityEngine;

namespace Runtime
{
    [CreateAssetMenu(fileName = "TilePrefabRegistry", menuName = "Map3D/Tile Prefab Registry")]
    public class TilePrefabRegistry : ScriptableObject
    {
        public GameObject DefaultSandTile;

        public List<PathPrefabEntry> PathPrefabs;
        public List<PavingPrefabEntry> PavingPrefabs;

        [System.Serializable]
        public struct PathPrefabEntry
        {
            public PathShape Shape;
            public Rotation Rotation;
            public GameObject Prefab;
        }

        [System.Serializable]
        public struct PavingPrefabEntry
        {
            public PavingPattern Pattern;
            public Rotation Rotation;
            public GameObject Prefab;
        }

        public GameObject GetPavingPrefab(PavingPattern p, Rotation r)
        {
            foreach (var e in PavingPrefabs)
                if (e.Pattern == p && e.Rotation == r)
                    return e.Prefab;

            return DefaultSandTile;
        }
    }
}
