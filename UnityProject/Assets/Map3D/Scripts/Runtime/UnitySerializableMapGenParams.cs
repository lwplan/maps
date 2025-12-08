using System;
using UnityEngine;
using NumericsVector2 = System.Numerics.Vector2;

namespace maps.Unity
{
    [Serializable]
    public class UnityMapGenParams
    {
        [Header("Levels")]
        public int NumLevels = 8;
        public int MinNodesPerLevel = 1;
        public int MaxNodesPerLevel = 3;

        [Header("Branching")]
        [Range(0f, 2f)]
        public float BifurcationFactor = 1f;

        [Header("Other")]
        public int? MinNodeDistance = null;

        // Convert to the real non-Unity params model
        public MapGenParams ToMapGenParams()
        {
            return new MapGenParams(
                NumLevels,
                MinNodesPerLevel,
                MaxNodesPerLevel,
                BifurcationFactor,
                MinNodeDistance,
                new NumericsVector2(500, 500)
            );
        }
    }
}
