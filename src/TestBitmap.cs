// This console entry point is excluded from normal test builds to avoid
// clashing with the test runner's generated entry point. Define TESTBITMAP_APP
// to include it when you want to run the bitmap renderer as a standalone app.
#if TESTBITMAP_APP
using System;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace maps;

class Program
{
    static void Main(string[] args)
    {
        // Default parameters
        var regionWidth = 1f;
        var regionHeight = 1f;
        int numLevels = 4;
        int minNodesPerLevel = 1;
        int maxNodesPerLevel = 3;
        float bifurcationFactor = 0.5f;
        int? minDistance = null;
        float pixelsPerUnit = 3f;

        // Simple CLI parsing
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--region-width" when i + 1 < args.Length && float.TryParse(args[i + 1], out var rw):
                    regionWidth = rw;
                    i++;
                    break;
                case "--region-height" when i + 1 < args.Length && float.TryParse(args[i + 1], out var rh):
                    regionHeight = rh;
                    i++;
                    break;
                case "--num-levels" when i + 1 < args.Length && int.TryParse(args[i + 1], out var nl):
                    numLevels = nl;
                    i++;
                    break;
                case "--min-nodes" when i + 1 < args.Length && int.TryParse(args[i + 1], out var minN):
                    minNodesPerLevel = minN;
                    i++;
                    break;
                case "--max-nodes" when i + 1 < args.Length && int.TryParse(args[i + 1], out var maxN):
                    maxNodesPerLevel = maxN;
                    i++;
                    break;
                case "--bifurcation-factor" when i + 1 < args.Length && float.TryParse(args[i + 1], out var bf):
                    bifurcationFactor = bf;
                    i++;
                    break;
                case "--min-distance" when i + 1 < args.Length && int.TryParse(args[i + 1], out var d):
                    minDistance = d;
                    i++;
                    break;
                case "--pixels-per-unit" when i + 1 < args.Length && float.TryParse(args[i + 1], out var ppu):
                    pixelsPerUnit = ppu;
                    i++;
                    break;
            }
        }

        var generator = new GameMapGenerator();
        var regionSize = new Vector2(regionWidth, regionHeight);
        var map = generator.GenerateMap(regionSize, numLevels, minNodesPerLevel, maxNodesPerLevel, bifurcationFactor, minDistance);

        if (map.Nodes == null || map.Nodes.Count == 0)
        {
            Console.WriteLine("No nodes generated.");
            return;
        }

        var bmp = BitmapMapRenderer.Render(map, pixelsPerUnit: pixelsPerUnit);
        bmp.Save("/tmp/map.png", new PngEncoder());
        Console.WriteLine($"Bitmap rendered to /tmp/map.png using region {map.RegionSize}.");
    }
}
#endif