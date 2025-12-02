using System;
using System.Collections.Generic;
using System.Numerics;
using DelaunatorSharp;
using GameBase.Model.Map;
using GameBase.UI.Core.Implementations;
using maps;

namespace GameBase.UI.Components.Map
{
    public static class Triangulator
    {
        public static void GenerateTriangulatedEdges(List<Node> nodes)
        {
            // Log.Info("Starting triangulation...");
            List<IPoint> pointList = new List<IPoint>();
            Dictionary<Vector2, int> pointIndexLookup = new Dictionary<Vector2, int>();

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                pointList.Add(new DelaunayPoint(node.Coordinates.X, node.Coordinates.Y));
                pointIndexLookup[node.Coordinates] = i;
                // Log.Info($"Node {i}: {node.Coordinates}, Level {node.Level}");
            }

            try
            {
                Delaunator delaunator = new Delaunator(pointList.ToArray());
                // Log.Info($"Delaunator triangles length: {delaunator.Triangles.Length}");
                if (delaunator.Triangles.Length == 0)
                {
                    Log.Error("Delaunator produced no triangles.");
                    return;
                }

                for (int i = 0; i < delaunator.Triangles.Length; i += 3)
                {
                    int a = delaunator.Triangles[i];
                    int b = delaunator.Triangles[i + 1];
                    int c = delaunator.Triangles[i + 2];

                    if (a < nodes.Count && b < nodes.Count && c < nodes.Count)
                    {
                        ConnectNodesIfAdjacent(nodes[a], nodes[b]);
                        ConnectNodesIfAdjacent(nodes[b], nodes[c]);
                        ConnectNodesIfAdjacent(nodes[c], nodes[a]);
                    }
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"Triangulation exception: {e.Message}");
            }
        }

        private static void ConnectNodesIfAdjacent(Node node1, Node node2)
        {
            if (Math.Abs(node1.Level - node2.Level) != 1) return;

            if (node1.Level < node2.Level)
            {
                if (!node1.NextLevelNodes.Contains(node2))
                {
                    node1.NextLevelNodes.Add(node2);
                    node2.PrevLevelNodes.Add(node1);
                    // Log.Info($"Connected: {node1.Coordinates} (Level {node1.Level}) -> {node2.Coordinates} (Level {node2.Level})");
                }
            }
            else
            {
                if (!node2.NextLevelNodes.Contains(node1))
                {
                    node2.NextLevelNodes.Add(node1);
                    node1.PrevLevelNodes.Add(node2);
                    // Log.Info($"Connected: {node2.Coordinates} (Level {node2.Level}) -> {node1.Coordinates} (Level {node1.Level})");
                }
            }
        }

        private struct DelaunayPoint : IPoint
        {
            public double X { get; set; }
            public double Y { get; set; }

            public DelaunayPoint(double x, double y)
            {
                X = x;
                Y = y;
            }
        }
    }
}