using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class WaterGenerator : TerrainObjectGenerator
{
    public static WaterGenerator Instance { get; private set; }
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public GameObject waterPrefab;
    [Range(0, 1)] public float waterHeight;
    private List<Vector3> accessibleWaterPoints = new List<Vector3>();
    
    private int planeSizeMultiplier = 10;

    public List<Vector3> GetAccessibleWaterPoints()
    {
        return accessibleWaterPoints;
    }

    public override void SpawnObjects(Bounds bounds)
    {
        ClearObjects();

        Vector3 chunkCenterWithoutVertical = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z);
        GameObject spawnedWater = Instantiate(waterPrefab, chunkCenterWithoutVertical, Quaternion.identity, gameObject.transform);
        spawnedWater.layer = gameObject.layer;

        // a plane with the same scale as a cube, is 10 times as large
        Vector3 spawnedWaterScale = bounds.size / planeSizeMultiplier;
        spawnedWaterScale.y = 1;
        spawnedWater.transform.localScale = spawnedWaterScale;

        float spawnedWaterHeight = Mathf.Lerp(bounds.min.y, bounds.max.y, waterHeight);
        spawnedWater.transform.position = new Vector3(spawnedWater.transform.position.x, spawnedWaterHeight, spawnedWater.transform.position.z);

        StartCoroutine(populateAccessibleWaterPoints(spawnedWater));
    }

    private IEnumerator populateAccessibleWaterPoints(GameObject spawnedWater)
    {
        yield return new WaitForSeconds(0.5f); // wait for mesh to be generated https://forum.unity.com/threads/raycast-doesnt-hit-mesh-collider.626878/

        while (spawnedWater == null) {
            yield return new WaitForSeconds(0.5f);
        }

        int gridSize = 50;
        Bounds bounds = spawnedWater.GetComponent<Renderer>().bounds;
        float minX = bounds.min.x;
        float minZ = bounds.min.z;

        float gridXSpace = bounds.size.x / gridSize;
        float gridZSpace = bounds.size.z / gridSize;

        float highYCoordinate = spawnedWater.transform.position.y + 10;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Vector3 raycastStart = new Vector3(minX + i * gridXSpace, highYCoordinate, minZ + j * gridZSpace);
                Ray ray = new Ray(raycastStart, Vector3.down);

                // if no ground, retry
                if (!Physics.Raycast(ray, out RaycastHit info, highYCoordinate + 2))
                {
                    continue;
                }

                if (info.collider.gameObject.CompareTag(gameObject.tag))
                {
                    accessibleWaterPoints.Add(info.point);
                }
            }
        }

        yield return null;
    }

    public override void ClearObjects()
    {
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}
