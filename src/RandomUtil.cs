using System;

namespace maps
{
    public static class RandomUtil
    {
        private static Random rng = new Random();

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
            rng = new Random(seed);
        }
    
        public static float Value()
        {
            return (float)rng.NextDouble(); // returns [0.0, 1.0)
        }

        public static bool Chance(float p0)
        {
            return rng.NextDouble() < p0;
        }
    }
}