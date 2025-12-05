#if TESTBITMAP_APP
using System;
using System.Numerics;
using System.Text;
using System.IO;
using maps.GameMapPipeline;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace maps
{
    class Program
    {
        static void Main(string[] args)
        {
            // Default parameters
            int numLevels = 5;
            int minNodesPerLevel = 1;
            int maxNodesPerLevel = 3;
            float bifurcationFactor = 0.5f;
            string? yamlOutputPath = null;
            int? rngSeed = null;

            // CLI parsing
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--num-levels" when i + 1 < args.Length && int.TryParse(args[i + 1], out var nl):
                        numLevels = nl; i++; break;

                    case "--min-nodes" when i + 1 < args.Length && int.TryParse(args[i + 1], out var minN):
                        minNodesPerLevel = minN; i++; break;

                    case "--max-nodes" when i + 1 < args.Length && int.TryParse(args[i + 1], out var maxN):
                        maxNodesPerLevel = maxN; i++; break;

                    case "--bifurcation-factor" when i + 1 < args.Length && float.TryParse(args[i + 1], out var bf):
                        bifurcationFactor = bf; i++; break;

                    case "--yaml-output" when i + 1 < args.Length:
                        yamlOutputPath = args[i + 1]; i++; break;

                    case "--seed" when i + 1 < args.Length && int.TryParse(args[i + 1], out var seed):
                        rngSeed = seed; i++; break;
                }
            }

            if (rngSeed.HasValue)
                RandomUtil.SetSeed(rngSeed.Value);

            //
            // Build tile-space pipeline
            //
            var pipeline = new GameMapPipeline.GameMapPipeline()
                .AddStep(new GenerateRawNodesStep())            // tile coords
                .AddStep(new TriangulationStep())               // bifurcation branching
                .AddStep(new AssignStartEndStep())             // mark start / end
                .AddStep(new BiomeGenerationStep());             // mark start / end

            var mapParams = new MapGenParams(
                NumLevels: numLevels,
                MinNodesPerLevel: minNodesPerLevel,
                MaxNodesPerLevel: maxNodesPerLevel,
                BifurcationFactor: bifurcationFactor
            );

            //
            // Run pipeline
            //
            var map = pipeline.Execute(mapParams);

            if (map.Nodes == null || map.Nodes.Count == 0)
            {
                Console.WriteLine("No nodes generated.");
                return;
            }

            //
            // YAML output
            //
            if (!string.IsNullOrEmpty(yamlOutputPath))
            {
                WriteMapYaml(map, yamlOutputPath);
            }

            //
            // Render to PNG
            //
            using var bmp = BitmapMapRenderer.Render(map);
            bmp.Save("/tmp/map.png", new PngEncoder());
            Console.WriteLine($"Bitmap rendered to /tmp/map.png");
        }

        private static void WriteMapYaml(GameMap map, string path)
        {
            var sb = new StringBuilder();

            sb.AppendLine("nodes:");

            foreach (var node in map.Nodes)
            {
                sb.AppendLine("  - level: " + node.Level);
                sb.AppendLine("    type: " + node.Type);

                sb.AppendLine("    tile:");
                sb.AppendLine($"      x: {node.TileX}");
                sb.AppendLine($"      y: {node.TileY}");

                sb.AppendLine("    macro:");
                sb.AppendLine($"      x: {node.MacroX}");
                sb.AppendLine($"      y: {node.MacroY}");

                sb.AppendLine("    connections:");
                foreach (var next in node.NextLevelNodes)
                {
                    sb.AppendLine($"      - to: L{next.Level} ({next.TileX},{next.TileY})");
                }

                sb.AppendLine();
            }

            File.WriteAllText(path, sb.ToString());
            Console.WriteLine($"Map YAML written to {path}");
        }
    }
}
#endif
