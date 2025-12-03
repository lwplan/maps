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
        var regionSize = new Vector2(1f, 1f);
        int numLevels = 4;
        int minNodesPerLevel = 1;
        int maxNodesPerLevel = 3;
        float bifurcationFactor = 0.5f;
        int? minDistance = null;

        // Simple CLI parsing
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--min-distance" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out var d))
                    minDistance = d;
                i++; // skip value
            }
        }

        var generator = new GameMapGenerator();
        var map = generator.GenerateMap(regionSize, numLevels, minNodesPerLevel, maxNodesPerLevel, bifurcationFactor, minDistance);

        if (map.Nodes == null || map.Nodes.Count == 0)
        {
            Console.WriteLine("No nodes generated.");
            return;
        }

        var bmp = BitmapMapRenderer.Render(map, pixelsPerUnit: 3f);
        bmp.Save("/tmp/map.png", new PngEncoder());
        Console.WriteLine($"Bitmap rendered to /tmp/map.png using region {map.RegionSize}.");
    }
}