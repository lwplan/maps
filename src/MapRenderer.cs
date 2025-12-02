#if UNITY_5_3_OR_NEWER || UNITY_EDITOR
using GameBase.Model.Map;
using GameBase.UI.Core.Implementations;
using UnityEngine;
using UnityEngine.UI;

namespace GameBase.UI.Components.Map
{
    public class MapRenderer
    {
        private Canvas canvas;
        private Camera renderCamera;
        private int width;
        private int height;
        private GameObject mapContainer;

        public MapRenderer(Canvas canvas, Camera camera, int width, int height)
        {
            this.canvas = canvas;
            this.renderCamera = camera;
            this.width = width;
            this.height = height;
        }

        public Texture2D RenderMap(MapClass map)
        {
            // Setup render texture
            RenderTexture renderTexture = new RenderTexture(width, height, 24);
            renderCamera.targetTexture = renderTexture;
            renderCamera.orthographic = true;
            renderCamera.orthographicSize = map.RegionSize.Y / 2f; // Half height for ortho view
            renderCamera.transform.position = new Vector3(map.RegionSize.X / 2f, map.RegionSize.Y / 2f, -10f);
            renderCamera.backgroundColor = Color.black;

            // Create temporary container on canvas
            mapContainer = new GameObject("MapContainer");
            mapContainer.transform.SetParent(canvas.transform, false);
            mapContainer.AddComponent<RectTransform>();

            // Render nodes
            foreach (var node in map.Nodes)
            {
                Vector2 canvasPos = new Vector2(node.Coordinates.X * map.RegionSize.X, node.Coordinates.Y * map.RegionSize.Y);
                Color nodeColor = node.Type switch
                {
                    NodeType.Start => Color.yellow,
                    NodeType.End => Color.magenta,
                    NodeType.Combat => Color.red,
                    NodeType.Event => Color.blue,
                    NodeType.Powerup => Color.green,
                    _ => Color.white
                };
                GameObject nodeObj = CreateNodeObject(canvasPos, nodeColor);
                if (node.Type == NodeType.Start || node.Type == NodeType.End)
                    nodeObj.transform.localScale *= 1.5f;
            }

            // Render edges
            foreach (var node in map.Nodes)
            {
                foreach (var nextNode in node.NextLevelNodes)
                {
                    Vector2 p1 = new Vector2(node.Coordinates.X * map.RegionSize.X, node.Coordinates.Y * map.RegionSize.Y);
                    Vector2 p2 = new Vector2(nextNode.Coordinates.X * map.RegionSize.X, nextNode.Coordinates.Y * map.RegionSize.Y);
                    CreateEdgeRenderer(p1, p2);
                }
            }

            // Capture render
            RenderTexture.active = renderTexture;
            renderCamera.Render();
            Texture2D mapImage = new Texture2D(width, height, TextureFormat.RGB24, false);
            mapImage.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            mapImage.Apply();

            // Cleanup
            RenderTexture.active = null;
            renderCamera.targetTexture = null;
            Object.DestroyImmediate(mapContainer);
            Object.DestroyImmediate(renderTexture);

            Log.Info($"Generated map image: {width}x{height}");
            return mapImage;
        }

        private GameObject CreateNodeObject(Vector2 position, Color color)
        {
            GameObject nodeObj = new GameObject("Node");
            nodeObj.transform.SetParent(mapContainer.transform, false);
            Image img = nodeObj.AddComponent<Image>();
            img.sprite = Resources.Load<Sprite>("Sprites/Circle"); // Assumes a circle sprite in Resources
            img.color = color;
            RectTransform rt = nodeObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(30f, 30f); // Fixed size, adjustable
            rt.anchoredPosition = position;
            return nodeObj;
        }

        private void CreateEdgeRenderer(Vector2 p1, Vector2 p2)
        {
            GameObject edgeObj = new GameObject("Edge");
            edgeObj.transform.SetParent(mapContainer.transform, false);
            LineRenderer line = edgeObj.AddComponent<LineRenderer>();
            line.material = new Material(Shader.Find("UI/Default")); // Default UI shader
            line.startColor = Color.green;
            line.endColor = Color.green;
            line.startWidth = 0.3f;
            line.endWidth = 0.3f;
            line.positionCount = 2;
            line.useWorldSpace = false;
            line.SetPosition(0, new Vector3(p1.x, p1.y, -1));
            line.SetPosition(1, new Vector3(p2.x, p2.y, -1));
            line.sortingOrder = -1;
        }
    }
}
#endif