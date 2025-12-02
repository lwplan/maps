// #if UNITY_5_3_OR_NEWER
// using Components.Ux.InputHandler;
// using ConsoleApp3.Core;
// using ConsoleApp3.TerminalGui;
// using Source.UI.Core.Model;
// using Source.UI.Map;
// using Source.UI.UxElements.Unity;
// using Source.UnityProject.Assets.Source.UI.Map;
// using UnityEngine;
//
// namespace Source.UI.Components.Map
// {
//     public class MapController : MonoBehaviour
//     {
//         [SerializeField] private UnityMapView mapView;
//         [SerializeField] public InputController inputController;
//
//         // Delegates for decoupling from GameManager
//         public delegate GameMap GetMapDelegate();
//         public delegate void NodeSelectedDelegate(int nodeIndex);
//         public GetMapDelegate GetCurrentMap; // Fetches the current map
//         public NodeSelectedDelegate OnNodeSelectedHandler; // Handles node selection
//         
//         void Awake()
//         {
//             if (mapView == null)
//             {
//                 Log.Error("MapController requires a MapView reference.");
//             }
//
//             inputController = FindObjectOfType<InputController>();
//             
//             // InputController can be assigned in Inspector or set by a parent component
//
//         }
//
//         private void OnEnable()
//         {
//             if (inputController == null)
//             {
//                 Log.Warning("No InputController assigned to MapController.");
//             }
//             else
//             {
//                 inputController.OnInput += HandleInputAction;
//             }
//         }
//
//         private void OnDisable()
//         {
//             if (inputController == null)
//             {
//                 Log.Warning("No InputController assigned to MapController.");
//             }
//             else
//             {
//                 inputController.OnInput -= HandleInputAction;
//             } 
//         }
//
//         void OnDestroy()
//         {
//             if (inputController != null)
//             {
//                 inputController.OnInput -= HandleInputAction;
//             }
//         }
//
//         void Start()
//         {
//             RefreshMap();
//         }
//
//         public void RefreshMap()
//         {
//             if (mapView != null && GetCurrentMap != null)
//             {
//                 GameMap map = GetCurrentMap();
//                 if (map != null)
//                 {
//                     mapView.RenderMap(map);
//                 }
//                 else
//                 {
//                     Log.Warning("No current map retrieved via GetCurrentMap delegate.");
//                 }
//             }
//         }
//
//         private void HandleInputAction(GameInputAction action)
//         {
//             if (action == GameInputAction.Escape)
//             {
//                // GameManager.Instance.OnEscapeFromMapController();
//             }
//             
//             if (mapView != null)
//             {
//                 
//                 int newNodeIndex = mapView.HandleInput(action);
//                 if (newNodeIndex != -1)
//                 {
//                     OnNodeSelected(newNodeIndex);
//                 }
//             }
//         }
//
//         private void OnNodeSelected(int nodeIndex)
//         {
//             if (GetCurrentMap != null)
//             {
//                 GameMap map = GetCurrentMap();
//                 if (map != null && nodeIndex >= 0 && nodeIndex < map.Nodes.Count)
//                 {
//                     Node selectedNode = map.Nodes[nodeIndex];
//                     map.VisitNode(selectedNode);
//                     if (OnNodeSelectedHandler != null)
//                     {
//                         OnNodeSelectedHandler(nodeIndex);
//                         // Log.Info($"Combat node selected: Index {nodeIndex}");
//                     }
//                 }
//             }
//         }
//     }
// }
// #endif