using UnityEngine;

public class TerrainAgentGenerator : TerrainObjectWithCountGenerator
{
    public GameObject prefab;
    public AgentStats initialAgentStats;
    public bool loadSettingsFromUserInput = false;

    [TagSelector] public string newTag;

    [Header("Randomisation Parameters")]
    const int maxTries = 100;

    void Awake()
    {
        if (loadSettingsFromUserInput)
        {
            count = SimulationSettingsController.settings[generatorName].population;
            initialAgentStats.speed = SimulationSettingsController.settings[generatorName].speed;
            initialAgentStats.size = SimulationSettingsController.settings[generatorName].size;
        }
    }

    public override void SpawnObjects(Bounds spawnAreaBounds)
    {
        ClearObjects();

        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < maxTries; j++)
            {
                float randomX = Random.Range(spawnAreaBounds.min.x, spawnAreaBounds.max.x);
                float randomZ = Random.Range(spawnAreaBounds.min.z, spawnAreaBounds.max.z);
                float highYCoordinate = spawnAreaBounds.max.y + 10;

                Vector3 raycastStart = new Vector3(randomX, highYCoordinate, randomZ);
                Ray ray = new Ray(raycastStart, Vector3.down);

                // if no ground, retry
                if (!Physics.Raycast(ray, out RaycastHit info, highYCoordinate))
                {
                    continue;
                }

                // if not terrain, retry
                if (info.transform.tag != "Terrain")
                {
                    continue;
                }

                Vector3 hitPosition = info.point;

                // rotate object on y axis
                Quaternion finalRotation = Quaternion.Euler(
                    0,
                    Random.Range(-180, 180),
                    0
                );

                GameObject instantiatedPrefab = Instantiate(prefab, hitPosition, finalRotation, gameObject.transform);
                instantiatedPrefab.layer = gameObject.layer;

                if (newTag.Length > 0)
                {
                    instantiatedPrefab.tag = newTag;
                }
                else
                {
                    instantiatedPrefab.tag = "Untagged";
                }

                AgentStats agentStats = new AgentStats(initialAgentStats, initialAgentStats);
                instantiatedPrefab.GetComponent<AgentBehavior>().stats = agentStats;

                break;
            }

        }
    }

    public override void ClearObjects()
    {
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}
