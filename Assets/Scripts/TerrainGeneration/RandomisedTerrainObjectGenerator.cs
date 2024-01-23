using UnityEngine;
using System.Collections;
using System;

using Random = UnityEngine.Random;
using Pathfinding;

public class RandomisedTerrainObjectGenerator : TerrainObjectWithCountGenerator
{
    [Header("Prefab list")]
    public GameObject[] prefabs;

    [Header("Constraints")]
    public bool unwalkable = false;
    [Range(0, 1)] public float minimumSpawnHeight = 0;
    [Range(0, 1)] public float maximumSpawnHeight = 1;
    [Tooltip("Maximum angle between the final prefab rotation from vertical")] public float maxAngleFromVertical = 0;

    [Header("Randomisation Parameters")]
    [Range(0, 2)] public float minScale = 1;
    [Range(0, 2)] public float maxScale = 1;

    [Tooltip("Max randomised offset rotation from the normal")]
    public Vector3 maxRotationOffset = new Vector3(0, 360, 0);
    [Tooltip("Offset from intersection with ground to spawn prefab in")]
    public Vector3 offsetFromGround = new Vector3(0, 0, 0);
    [Tooltip("Whether prefabs should respawn if removed")] public bool shouldRespawn = false;
    public float minimumTimeBetweenRespawnSeconds = 1;
    public bool loadSettingsFromUserInput = false;
    const int maxTries = 1000;

    void Awake()
    {
        if (loadSettingsFromUserInput)
        {
            count = SimulationSettingsController.settings[generatorName].population;
        }
    }

    public override void SpawnObjects(Bounds areaBounds)
    {
        ClearObjects();

        for (int i = 0; i < count; i++)
        {
            SpawnRandomObjectInRandomPlaceWithRandomRotationAndScale(areaBounds);
        }

        if (Application.isPlaying && shouldRespawn)
        {
            StartCoroutine(CleanUpInvalidObjects());
        }
    }

    private bool SpawnRandomObjectInRandomPlaceWithRandomRotationAndScale(Bounds spawnAreaBounds)
    {
        float minimumHeightToSpawnPrefab = Mathf.Lerp(spawnAreaBounds.min.y, spawnAreaBounds.max.y, minimumSpawnHeight);
        float maximumHeightToSpawnPrefab = Mathf.Lerp(spawnAreaBounds.min.y, spawnAreaBounds.max.y, maximumSpawnHeight);

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
            if (!info.transform.CompareTag("Terrain"))
            {
                continue;
            }

            Vector3 hitPosition = info.point;

            // if spawn position does not satisfy height constraints, retry
            if (hitPosition.y < minimumHeightToSpawnPrefab || hitPosition.y > maximumHeightToSpawnPrefab)
            {
                continue;
            }

            // tilt object from normal
            Vector3 finalRotation = Quaternion.Euler(
                Random.Range(-maxRotationOffset.x, maxRotationOffset.x),
                Random.Range(-maxRotationOffset.y, maxRotationOffset.y),
                Random.Range(-maxRotationOffset.z, maxRotationOffset.z)
            ) * info.normal;

            // check whether final rotation is within bounds
            float angleBetweenNormalAndVertical = Vector3.Angle(Vector3.up, finalRotation);
            if (Mathf.Abs(angleBetweenNormalAndVertical) > maxAngleFromVertical)
            {
                continue;
            }

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length - 1)];
            GameObject instantiatedPrefab = Instantiate(prefab, hitPosition + offsetFromGround, Quaternion.identity, gameObject.transform);
            instantiatedPrefab.transform.rotation = Quaternion.FromToRotation(Vector3.up, finalRotation);
            instantiatedPrefab.layer = gameObject.layer;

            Vector3 scale = new Vector3(Random.Range(minScale, maxScale), Random.Range(minScale, maxScale), Random.Range(minScale, maxScale));
            instantiatedPrefab.transform.localScale = scale;

            if (shouldRespawn)
            {
                TerrainObject terrainObject = instantiatedPrefab.AddComponent<TerrainObject>();
                terrainObject.OnDestroyed += HandleTerrainObjectRemovedWithBounds(spawnAreaBounds);
            }

            if (unwalkable)
            {
                RecastMeshObj recastMeshObj = instantiatedPrefab.AddComponent<RecastMeshObj>();
                recastMeshObj.area = -1;
            }

            return true;
        }

        return false;
    }

    private TerrainObject.OnDestroyedHandler HandleTerrainObjectRemovedWithBounds(Bounds areaBounds)
    {
        void HandleTerrainObjectRemoved()
        {
            if (Application.isPlaying && gameObject.activeInHierarchy && shouldRespawn)
            {
                StartCoroutine(HandleRespawnCurrentObject(areaBounds));
            }

        }

        return HandleTerrainObjectRemoved;
    }

    IEnumerator HandleRespawnCurrentObject(Bounds areaBounds)
    {
        yield return new WaitForSeconds(minimumTimeBetweenRespawnSeconds + 0.1f);
        SpawnRandomObjectInRandomPlaceWithRandomRotationAndScale(areaBounds);
    }

    private const int cleanUpFoodIntervalSeconds = 10;
    IEnumerator CleanUpInvalidObjects()
    {
        // when food is unreachable, we move its layer to "Default"
        // we should clear these unreachable foods periodically
        // the `TerrainObject.OnDestroyHandler` in the child will handle its respawn
        while (true)
        {
            yield return new WaitForSeconds(cleanUpFoodIntervalSeconds);

            foreach (Transform child in transform)
            {
                if (child.gameObject.layer != gameObject.layer)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    public override void ClearObjects()
    {
        if (gameObject == null)
        {
            return;
        }

        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}