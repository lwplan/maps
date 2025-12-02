using System;

namespace GameBase.UI.Util
{
    public static class RandomUtil
    {
        private static readonly Random rng = new Random();

        // Inclusive min, exclusive max — matches System.Random
        public static int Range(int minInclusive, int maxExclusive)
        {
            return rng.Next(minInclusive, maxExclusive);
        }

        // Inclusive float range — matches Unity behavior
        public static float Range(float minInclusive, float maxInclusive)
        {
            return (float)(rng.NextDouble() * (maxInclusive - minInclusive) + minInclusive);
        }

        public static void SetSeed(int seed)
        {
            // For deterministic maps/tests
            typeof(Random).GetField("_seedArray", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(rng, new Random(seed));
        }
    
        public static float Value()
        {
            return (float)rng.NextDouble(); // returns [0.0, 1.0)
        }
    }
}