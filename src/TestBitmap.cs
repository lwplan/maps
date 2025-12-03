// Added simple test harness to validate bitmap rendering
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace maps
{
    class Program
    {
        static void Main()
        {
            // Create a tiny map with two nodes and one edge
            var nodes = new List<Node>
            {
                new Node(new Vector2(0.1f,0.1f), 0, NodeType.Combat, null),
                new Node(new Vector2(0.8f,0.9f), 1, NodeType.Trading, null)
            };
            nodes[0].NextLevelNodes.Add(nodes[1]);

            using var bmp = BitmapMapRenderer.Render(nodes, asciiWidth: 10, asciiHeight: 5);
            bmp.Save("./bitmap_test.png");
            Console.WriteLine("Bitmap saved to C:\\Temp\\bitmap_test.png");
        }
    }
}
