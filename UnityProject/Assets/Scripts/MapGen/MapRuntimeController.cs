using maps;
using UnityEngine;

public class MapRuntimeController : MonoBehaviour
{
    [SerializeField] private GameMapGeneratorBehaviour generator;
    [SerializeField] private MapPlacer placer;
    [SerializeField] private bool regenerateOnStart = true;

    private void Awake()
    {
        if (generator != null)
        {
            generator.OnMapGenerated += HandleMapGenerated;
        }
    }

    private void OnDestroy()
    {
        if (generator != null)
        {
            generator.OnMapGenerated -= HandleMapGenerated;
        }
    }

    private void Start()
    {
        if (regenerateOnStart && generator != null)
        {
            generator.Generate();
        }
        else if (generator != null && generator.GeneratedMap != null && placer != null)
        {
            placer.Build(generator.GeneratedMap);
        }
    }

    private void HandleMapGenerated(GameMap map)
    {
        if (map == null || placer == null)
        {
            return;
        }

        placer.Build(map);
    }
}
