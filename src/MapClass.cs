#if UNITY_5_3_OR_NEWER
using System.Collections.Generic;
using GameBase.Model.Map;
using GameBase.UI.Core.Implementations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityVector2 = UnityEngine.Vector2;
using NumericVector2 = System.Numerics.Vector2;


namespace GameBase.UI.Components.Map
{
    public class MapClass
    {
        public List<Node> Nodes { get; private set; }
        public Node StartNode { get; private set; }
        public Node EndNode { get; private set; }
        public NumericVector2 RegionSize { get; private set; }
        public int NumLevels { get; private set; }
        public int MinNodesPerLevel { get; private set; }
        public int MaxNodesPerLevel { get; private set; }
        public float BifurcationFactor { get; private set; }
        private GameContext _context;
        
        #if UNITY_5_3_OR_NEWER
        public RectTransform MapContainer { get; private set; }
        #endif
        public MapClass(NumericVector2 regionSize, int numLevels, int minNodesPerLevel, int maxNodesPerLevel, float bifurcationFactor, RectTransform MapContainer, GameContext context)
        {
            RegionSize = regionSize;
            NumLevels = numLevels;
            MinNodesPerLevel = minNodesPerLevel;
            MaxNodesPerLevel = maxNodesPerLevel;
            BifurcationFactor = bifurcationFactor;
            MapContainer = MapContainer;
            _context = context;
            GenerateMap();
        }

        public void GenerateMap()
        {
            if (RegionSize.X <= 0 || RegionSize.Y <= 0 || NumLevels < 3)
            {
                Log.Warning("Invalid map generation parameters.");
                return;
            }

            Nodes = NodeGenerator.GenerateNodes(RegionSize, NumLevels, MinNodesPerLevel, MaxNodesPerLevel, _context);
            if (Nodes.Count < 3)
            {
                Log.Error($"Map generation failed: Only {Nodes.Count} nodes generated (minimum 3 required).");
                Nodes = null;
                return;
            }
            StartNode = Nodes.Find(n => n.Level == 0);
            EndNode = Nodes.Find(n => n.Level == NumLevels - 1);
            Triangulator.GenerateTriangulatedEdges(Nodes);
            EliminateDisconnectedNodes();
            EnforceConnectivityAndBifurcation();
        }

        public void RenderToCanvas(Canvas canvas, float nodeSize = 30f, float edgeWidth = 0.3f, Material edgeMaterial = null, bool showDebugLabels = true)
        {
            if (Nodes == null || Nodes.Count == 0)
            {
                Log.Warning("No nodes to render.");
                return;
            }

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            UnityVector2 canvasSize = canvasRect.sizeDelta;

            // Clear previous render
            for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);
            }
            

            // Render nodes as UI Images
            foreach (var node in Nodes)
            {
                UnityVector2 canvasPos = MapToCanvasCoordinates(node.Coordinates, canvasSize);
                Color nodeColor = node.Type switch
                {
                    NodeType.Start => Color.yellow,
                    NodeType.End => Color.magenta,
                    NodeType.Combat => Color.red,
                    NodeType.Event => Color.blue,
                    NodeType.Powerup => Color.green,
                    _ => Color.white
                };
                GameObject nodeObj = CreateNodeObject(MapContainer.transform, canvasPos, nodeColor, nodeSize);
                if (node.Type == NodeType.Start || node.Type == NodeType.End)
                    nodeObj.transform.localScale *= 1.5f;

                if (showDebugLabels)
                    CreateDebugLabel(nodeObj.transform, node.Level.ToString());
            }

            // Render edges as LineRenderers in UI space
            foreach (var node in Nodes)
            {
                foreach (var nextNode in node.NextLevelNodes)
                {
                    UnityVector2 p1 = MapToCanvasCoordinates(node.Coordinates, canvasSize);
                    UnityVector2 p2 = MapToCanvasCoordinates(nextNode.Coordinates, canvasSize);
                    CreateEdgeRenderer(MapContainer.transform, p1, p2, edgeWidth, edgeMaterial);
                }
            }

            Log.Info("Map rendered to canvas.");
        }

        private UnityVector2 MapToCanvasCoordinates(NumericVector2 normalizedPos, UnityVector2 canvasSize)
        {
            float x = (normalizedPos.X * canvasSize.x) - (canvasSize.x / 2f);
            float y = (normalizedPos.Y * canvasSize.y) - (canvasSize.y / 2f);
            return new UnityVector2(x, y);
        }

        private GameObject CreateNodeObject(Transform parent, UnityVector2 position, Color color, float size)
        {
            GameObject nodeObj = new GameObject("Node");
            nodeObj.transform.SetParent(parent, false);
            Image img = nodeObj.AddComponent<Image>();
            img.sprite = Resources.Load<Sprite>("Sprites/Circle"); // Ensure a circle sprite exists
            img.color = color;
            RectTransform rt = nodeObj.GetComponent<RectTransform>();
            rt.sizeDelta = new UnityVector2(size, size);
            rt.anchoredPosition = position;
            return nodeObj;
        }

        private void CreateEdgeRenderer(Transform parent, UnityVector2 p1, UnityVector2 p2, float width, Material edgeMaterial)
        {
            GameObject edgeObj = new GameObject("Edge");
            edgeObj.transform.SetParent(parent, false);
            edgeObj.AddComponent<CanvasRenderer>();
            UILineRenderer line = edgeObj.AddComponent<UILineRenderer>();
            line.startPoint = p1;
            line.endPoint = p2;
            line.thickness = width;
            line.material = edgeMaterial;
            line.color = Color.green; // Set color directly since UILineRenderer inherits from Graphic
        }

        private void CreateDebugLabel(Transform parent, string text)
        {
            GameObject labelObj = new GameObject("DebugLabel");
            labelObj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 14;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform rt = tmp.GetComponent<RectTransform>();
            rt.sizeDelta = new UnityVector2(50, 20);
            rt.anchoredPosition = UnityVector2.zero;
        }

        // Assume EliminateDisconnectedNodes and EnforceConnectivityAndBifurcation are unchanged
        private void EliminateDisconnectedNodes() { /* Existing implementation */ }
        private void EnforceConnectivityAndBifurcation() { /* Existing implementation */ }
    }
}
#endif