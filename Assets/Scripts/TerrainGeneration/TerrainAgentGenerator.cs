using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainAgentGenerator : TerrainObjectGenerator
{
    public GameObject prefab;
    public AgentStats initialAgentStats;
    [TagSelector] public string newTag;

    [Header("Constraints")]
    [Range(0, 1)] public float minimumSpawnHeight = 0;
    [Range(0, 1)] public float maximumSpawnHeight = 1;

    [Header("Randomisation Parameters")]
    [Range(0, 1000)][Tooltip("Total prefabs to spawn")] public int count = 1;
    const int maxTries = 10;

    public override void SpawnObjects(Bounds spawnAreaBounds)
    {
        ClearObjects();

        float minimumHeightToSpawnPrefab = Mathf.Lerp(spawnAreaBounds.min.y, spawnAreaBounds.max.y, minimumSpawnHeight);
        float maximumHeightToSpawnPrefab = Mathf.Lerp(spawnAreaBounds.min.y, spawnAreaBounds.max.y, maximumSpawnHeight);


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

                // if spawn position does not satisfy height constraints, retry
                if (hitPosition.y < minimumHeightToSpawnPrefab || hitPosition.y > maximumHeightToSpawnPrefab)
                {
                    continue;
                }

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
