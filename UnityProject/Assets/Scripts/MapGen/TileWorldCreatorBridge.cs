using System.Collections.Generic;
using maps;
using GiantGrey.TileWorldCreator;
using UnityEngine;
using NumericsVector2 = System.Numerics.Vector2;

public class TileWorldCreatorBridge : MonoBehaviour
{
    [Header("Map Source")]
    [SerializeField] private GameMapGeneratorBehaviour mapGenerator;

    [Header("TileWorld Creator")]
    [SerializeField] private TileWorldCreatorManager tileWorldCreator;
    [SerializeField] private string blueprintLayerName = "Ground";

    private readonly Dictionary<Vector2Int, Node> cellToNode = new();

    [ContextMenu("Generate TileWorld Map")]
    public void GenerateTileWorldMap()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("TileWorld generation can only be triggered in Play Mode.");
            return;
        }

        if (mapGenerator == null || tileWorldCreator == null)
        {
            Debug.LogWarning("Map generator or TileWorld Creator reference is missing.");
            return;
        }

        mapGenerator.Generate();
        BuildFromGeneratedMap();
    }

    public void BuildFromGeneratedMap()
    {
        if (tileWorldCreator?.configuration == null)
        {
            Debug.LogWarning("TileWorldCreatorManager is missing a configuration asset.");
            return;
        }

        var generatedMap = mapGenerator?.GeneratedMap;
        if (generatedMap == null)
        {
            Debug.LogWarning("No generated map available. Run the GameMapGeneratorBehaviour first.");
            return;
        }

        var blueprintLayer = FindBlueprintLayer(blueprintLayerName);
        if (blueprintLayer == null)
        {
            Debug.LogWarning($"Blueprint layer '{blueprintLayerName}' not found.");
            return;
        }

        tileWorldCreator.ResetConfiguration();

        ConfigureGrid(generatedMap.RegionSize);
        PaintRegion(blueprintLayer, generatedMap);

        tileWorldCreator.ExecuteBlueprintLayers();
        tileWorldCreator.ExecuteBuildLayers(ExecutionMode.FromScratch);
    }

    private void ConfigureGrid(NumericsVector2 regionSize)
    {
        var width = Mathf.Max(1, Mathf.CeilToInt(regionSize.X));
        var height = Mathf.Max(1, Mathf.CeilToInt(regionSize.Y));

        tileWorldCreator.configuration.width = 1;
        tileWorldCreator.configuration.height = 1;
        tileWorldCreator.configuration.cellSize = Mathf.Max(width, height);
        tileWorldCreator.configuration.lastCellSize = tileWorldCreator.configuration.cellSize;
    }

    private void PaintRegion(BlueprintLayer blueprintLayer, GameMap generatedMap)
    {
        blueprintLayer.ClearLayer(_executeLayer: false);

        cellToNode.Clear();
        var cellPosition = Vector2Int.zero;

        var cells = new HashSet<Vector2> { new Vector2(cellPosition.x, cellPosition.y) };
        blueprintLayer.AddCells(cells);

        if (generatedMap.StartNode != null)
        {
            cellToNode[cellPosition] = generatedMap.StartNode;
        }
    }

    private BlueprintLayer FindBlueprintLayer(string layerName)
    {
        if (tileWorldCreator?.configuration == null)
        {
            return null;
        }

        foreach (var folder in tileWorldCreator.configuration.blueprintLayerFolders)
        {
            foreach (var layer in folder.blueprintLayers)
            {
                if (layer != null && layer.layerName == layerName)
                {
                    return layer;
                }
            }
        }

        return null;
    }
}
