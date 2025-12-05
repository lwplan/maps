using UnityEngine;
using maps;
using maps.GameMapPipeline;

public class GameMapGeneratorBehaviour : MonoBehaviour
{
    [Header("Map Generation Parameters")]
    [SerializeField] private int numLevels = 5;
    [SerializeField] private int minNodesPerLevel = 1;
    [SerializeField] private int maxNodesPerLevel = 3;
    [SerializeField, Range(0f, 1f)] private float bifurcationFactor = 0.5f;

    public GameMapPipeline Pipeline { get; private set; }
    public GameMap GeneratedMap { get; private set; }

    public void Generate()
    {
        Pipeline = BuildPipeline();

        var parameters = new MapGenParams(
            NumLevels: numLevels,
            MinNodesPerLevel: minNodesPerLevel,
            MaxNodesPerLevel: maxNodesPerLevel,
            BifurcationFactor: bifurcationFactor
        );

        GeneratedMap = Pipeline.Execute(parameters);
    }

    [ContextMenu("Generate Map")]
    private void GenerateFromContextMenu()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Generate Map can only be triggered in Play Mode.");
            return;
        }

        Generate();
    }

    private GameMapPipeline BuildPipeline()
    {
        return new GameMapPipeline()
            .AddStep(new GenerateRawNodesStep())
            .AddStep(new TriangulationStep())
            .AddStep(new AssignStartEndStep())
            .AddStep(new BiomeGenerationStep());
    }
}
