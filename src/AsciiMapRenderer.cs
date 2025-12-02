// using ConsoleApp3.Components.Ux.Map;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// 
// namespace ConsoleApp3.UI.Util;
// 
// public static class ScaledGraphRenderer
// {
//     public static List<string> Render(List<Node> nodes, int width = 80, int height = 24)
//     {
//         if (nodes.Count == 0)
//             return new List<string> { "No nodes." };
// 
//         // Step 1: Normalize and scale node coordinates
//         float minX = nodes.Min(n => n.Coordinates.X);
//         float maxX = nodes.Max(n => n.Coordinates.X);
//         float minY = nodes.Min(n => n.Coordinates.Y);
//         float maxY = nodes.Max(n => n.Coordinates.Y);
// 
//         float xRange = Math.Max(maxX - minX, 0.01f);
//         float yRange = Math.Max(maxY - minY, 0.01f);
// 
//         var scaled = new Dictionary<Node, (int x, int y)>();
//         foreach (var node in nodes)
//         {
//             int x = (int)((node.Coordinates.X - minX) / xRange * (width - 4));
//             int y = (int)((node.Coordinates.Y - minY) / yRange * (height - 1));
//             scaled[node] = (x, y);
//         }
// 
//         var buffer = new AsciiLineBuffer(width, height);
// 
//         // Step 2: Draw edges
//         foreach (var node in nodes)
//         {
//             if (!scaled.TryGetValue(node, out var from)) continue;
//             int x1 = from.x + 1;
// 
//             foreach (var target in node.NextLevelNodes)
//             {
//                 if (!scaled.TryGetValue(target, out var to)) continue;
//                 int x2 = to.x + 1;
//                 int y1 = from.y;
//                 int y2 = to.y;
// 
//                 int bendX = (x1 + x2) / 2;
// 
//                 // Horizontal to bend
//                 for (int x = Math.Min(x1, bendX); x <= Math.Max(x1, bendX); x++)
//                 {
//                     buffer.AddLine(x, y1, LineDirection.East | LineDirection.West);
//                 }
// 
//                 // Vertical at bend
//                 if (y1 != y2)
//                 {
//                     int step = y1 < y2 ? 1 : -1;
//                     for (int y = y1 + step; y != y2; y += step)
//                     {
//                         buffer.AddLine(bendX, y, LineDirection.North | LineDirection.South);
//                     }
// 
//                     // Corner at entry
//                     buffer.AddLine(bendX, y1, y2 > y1
//                         ? LineDirection.South | LineDirection.West
//                         : LineDirection.North | LineDirection.West);
// 
//                     // Corner at exit
//                     buffer.AddLine(bendX, y2, y2 > y1
//                         ? LineDirection.North | LineDirection.East
//                         : LineDirection.South | LineDirection.East);
//                 }
// 
//                 // Final horizontal
//                 int xStart = Math.Min(bendX, x2);
//                 int xEnd = Math.Max(bendX, x2);
// 
// // draw only between bendX+1 and x2-1
//                 for (int x = xStart + 1; x < xEnd; x++)
//                 {
//                     buffer.AddLine(x, y2, LineDirection.East | LineDirection.West);
//                 }
// 
//             }
//         }
// 
//         // Step 3: Draw nodes as -C-
//         foreach (var node in nodes)
//         {
//             var (x, y) = scaled[node];
//             char symbol = node.Type switch
//             {
//                 NodeType.Combat => 'C',
//                 NodeType.Trading => 'T',
//                 NodeType.End => 'E',
//                 _ => '?'
//             };
// 
//             buffer.SetOverlay(x, y, '-');
//             buffer.SetOverlay(x + 1, y, symbol);
//             buffer.SetOverlay(x + 2, y, '-');
//         }
// 
//         return buffer.ToLines().ToList();
//     }
// }
