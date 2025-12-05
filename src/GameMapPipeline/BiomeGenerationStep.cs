using System;
using System.Collections.Generic;
using System.Linq;


namespace maps.GameMapPipeline
{
    public class BiomeGenerationStep : IMapGenStep
    {
        public void Execute(GameMap map, MapGenParams p)
        {
            // Infer world bounds from tile coordinates
            int minX = map.Nodes.Min(n => n.TileX) - 200;
            int maxX = map.Nodes.Max(n => n.TileX) + 200;
            int minY = map.Nodes.Min(n => n.TileY) - 200;
            int maxY = map.Nodes.Max(n => n.TileY) + 200;

            int width  = maxX - minX + 1;
            int height = maxY - minY + 1;

            var biomeMap = new BiomeMap(width, height, minX, minY);

            //
            // 1. Towns
            //
            foreach (var node in ChooseTownNodes(map.Nodes))
                StampTown(biomeMap, node, radiusTiles: 60);

            //
            // 2. Battlements
            //
            foreach (var node in ChooseBattlementNodes(map.Nodes))
                StampBattlement(biomeMap, node, halfSize: 70);

            //
            // 3. Broad biomes (Dunes, Canyon, Mountain)
            //
            FillBroadBiomes(biomeMap, map.Nodes, seedCount: 12);

            //
            // 4. Sea edges
            //
            FillSeaEdges(biomeMap);

            // Save biome map into GameMap
            map.Biomes = biomeMap;
        }

        // ------------------------------------------------------------
        // Towns
        // ------------------------------------------------------------
        private IEnumerable<Node> ChooseTownNodes(List<Node> nodes)
        {
            // Example: choose 1–2 nodes near mid-level
            return nodes
                .Where(n => n.Level > 0 && n.Level < nodes.Max(x => x.Level))
                .OrderBy(_ => RandomUtil.Range(0f, 1f))
                .Take(RandomUtil.Range(1, 3));
        }

        private void StampTown(BiomeMap map, Node node, int radiusTiles)
        {
            int cx = node.TileX - map.OffsetX;
            int cy = node.TileY - map.OffsetY;

            int r2 = radiusTiles * radiusTiles;

            for (int x = cx - radiusTiles; x <= cx + radiusTiles; x++)
            for (int y = cy - radiusTiles; y <= cy + radiusTiles; y++)
            {
                if (!map.InBounds(x, y)) continue;

                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r2)
                    map[x, y] = Biome.Town;
            }
        }

        // ------------------------------------------------------------
        // Battlements
        // ------------------------------------------------------------
        private IEnumerable<Node> ChooseBattlementNodes(List<Node> nodes)
        {
            // Example: choose 0–1 battlements near higher levels
            return nodes
                .Where(n => n.Level >= nodes.Max(x => x.Level) - 2)
                .OrderBy(_ => RandomUtil.Range(1f, 3f))
                .Take(RandomUtil.Range(2, 2));
        }

        private void StampBattlement(BiomeMap map, Node node, int halfSize)
        {
            int cx = node.TileX - map.OffsetX;
            int cy = node.TileY - map.OffsetY;

            for (int x = cx - halfSize; x <= cx + halfSize; x++)
            for (int y = cy - halfSize; y <= cy + halfSize; y++)
            {
                if (map.InBounds(x, y))
                    map[x, y] = Biome.Battlement;
            }
        }

        // ------------------------------------------------------------
        // Broad biomes (Voronoi fill)
        // ------------------------------------------------------------
        private void FillBroadBiomes(BiomeMap map, List<Node> nodes, int seedCount)
        {
            var rand = new Random();
            var seeds = new List<(int x, int y, Biome biome)>();

            // Choose biomes for seeds
            Biome[] candidates = { Biome.Dunes, Biome.Canyon, Biome.Mountain };

            for (int i = 0; i < seedCount; i++)
            {
                int x = rand.Next(0, map.Width);
                int y = rand.Next(0, map.Height);
                Biome b = candidates[rand.Next(candidates.Length)];
                seeds.Add((x, y, b));
            }

            // Voronoi fill
            for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                if (map[x, y] != Biome.None)
                    continue;

                int best = int.MaxValue;
                Biome chosen = Biome.Dunes;

                foreach (var s in seeds)
                {
                    int dx = x - s.x;
                    int dy = y - s.y;
                    float nx = Perlin.Noise(x * 0.01f, y * 0.01f) * 6f;
                    float ny = Perlin.Noise(x * 0.01f + 1000, y * 0.01f + 1000) * 6f;

                    float dxn = (x + nx) - s.x;
                    float dyn = (y + ny) - s.y;

                    float d2 = dxn * dxn + dyn * dyn;

                    if (d2 < best)
                    {
                        best = (int)d2;
                        chosen = s.biome;
                    }
                }

                map[x, y] = chosen;
            }
        }

        // ------------------------------------------------------------
        // Sea edges
        // ------------------------------------------------------------
        private void FillSeaEdges(BiomeMap map)
        {
            bool northSea = RandomUtil.Chance(0.3f);
            bool southSea = RandomUtil.Chance(0.3f);
            bool westSea  = RandomUtil.Chance(0.3f);
            bool eastSea  = RandomUtil.Chance(0.3f);

            if (northSea)
                FloodSeaFromEdge(map, 0, +1, isVertical: true);
            if (southSea)
                FloodSeaFromEdge(map, map.Height - 1, -1, isVertical: true);
            if (westSea)
                FloodSeaFromEdge(map, 0, +1, isVertical: false);
            if (eastSea)
                FloodSeaFromEdge(map, map.Width - 1, -1, isVertical: false);
        }

        private void FloodSeaFromEdge(BiomeMap map, int start, int step, bool isVertical)
        {
            // Flood until hitting any non-None biome
            if (isVertical)
            {
                for (int y = start; y >= 0 && y < map.Height; y += step)
                {
                    bool stop = false;
                    for (int x = 0; x < map.Width; x++)
                    {
                        if (map[x, y] != Biome.None)
                        {
                            stop = true;
                            break;
                        }
                        map[x, y] = Biome.Sea;
                    }
                    if (stop) break;
                }
            }
            else
            {
                for (int x = start; x >= 0 && x < map.Width; x += step)
                {
                    bool stop = false;
                    for (int y = 0; y < map.Height; y++)
                    {
                        if (map[x, y] != Biome.None)
                        {
                            stop = true;
                            break;
                        }
                        map[x, y] = Biome.Sea;
                    }
                    if (stop) break;
                }
            }
        }
    }
}
