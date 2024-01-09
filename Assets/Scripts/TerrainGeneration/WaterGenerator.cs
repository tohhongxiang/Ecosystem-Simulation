using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Pathfinding;

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
    [Range(1, 512)] public int gridSize = 256;
    private HashSet<Vector3> accessibleWaterPoints = new HashSet<Vector3>();

    private int planeSizeMultiplier = 10;

    public HashSet<Vector3> GetAccessibleWaterPoints()
    {
        return accessibleWaterPoints;
    }

    public override void SpawnObjects(Bounds bounds)
    {
        ClearObjects();
        accessibleWaterPoints.Clear();

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

        while (spawnedWater == null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        Bounds bounds = spawnedWater.GetComponent<Renderer>().bounds;
        float minX = bounds.min.x;
        float minZ = bounds.min.z;

        float gridXSpace = bounds.size.x / gridSize;
        float gridZSpace = bounds.size.z / gridSize;

        float highYCoordinate = spawnedWater.transform.position.y + 10;

        for (int i = 1; i < gridSize - 1; i++)
        {
            for (int j = 1; j < gridSize - 1; j++)
            {
                Vector3 raycastStart = new Vector3(minX + i * gridXSpace, highYCoordinate, minZ + j * gridZSpace);
                Ray ray = new Ray(raycastStart, Vector3.down);

                if (!Physics.Raycast(ray, out RaycastHit info, highYCoordinate + 2))
                {
                    continue;
                }

                if (info.collider.gameObject.layer != gameObject.layer)
                {
                    continue;
                }

                // if all neighboring points are water, the agent doesn't need to care about this point. We skip this point
                if (AreAllNeighborsWater(i, j, minX, highYCoordinate, minZ, gridXSpace, gridZSpace))
                {
                    continue;
                }

                // from the water point, get the nearest walkable point to that water point
                accessibleWaterPoints.Add(AstarPath.active.GetNearest(info.point).position);
            }
        }

        yield return null;
    }

    private bool AreAllNeighborsWater(int i, int j, float minX, float highYCoordinate, float minZ, float gridXSpace, float gridZSpace) {
        RaycastHit upNeighborInfo;
        RaycastHit downNeighborInfo;
        RaycastHit leftNeighborInfo;
        RaycastHit rightNeighborInfo;

        Vector3 upRaycastStart = new Vector3(minX + i * gridXSpace, highYCoordinate, minZ + (j - 1) * gridZSpace);
        Ray upRay = new Ray(upRaycastStart, Vector3.down);
        Physics.Raycast(upRay, out upNeighborInfo, highYCoordinate + 2);

        Vector3 downRaycastStart = new Vector3(minX + i * gridXSpace, highYCoordinate, minZ + (j + 1) * gridZSpace);
        Ray downRay = new Ray(downRaycastStart, Vector3.down);
        Physics.Raycast(downRay, out downNeighborInfo, highYCoordinate + 2);

        Vector3 leftRaycastStart = new Vector3(minX + (i - 1) * gridXSpace, highYCoordinate, minZ + j * gridZSpace);
        Ray leftRay = new Ray(leftRaycastStart, Vector3.down);
        Physics.Raycast(leftRay, out leftNeighborInfo, highYCoordinate + 2);

        Vector3 rightRaycastStart = new Vector3(minX + (i + 1) * gridXSpace, highYCoordinate, minZ + j * gridZSpace);
        Ray rightRay = new Ray(rightRaycastStart, Vector3.down);
        Physics.Raycast(rightRay, out rightNeighborInfo, highYCoordinate + 2);

        // if all neighboring points are water, the agent doesn't need to care about this point. We continue
        return upNeighborInfo.collider.gameObject.layer == gameObject.layer &&
            downNeighborInfo.collider.gameObject.layer == gameObject.layer &&
            leftNeighborInfo.collider.gameObject.layer == gameObject.layer &&
            rightNeighborInfo.collider.gameObject.layer == gameObject.layer;
    }

    public void ClearWaterPoint(Vector3 waterPoint) {
        accessibleWaterPoints.Remove(waterPoint);
    }

    public override void ClearObjects()
    {
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        foreach (var waterPoint in accessibleWaterPoints)
        {
            Gizmos.DrawCube(waterPoint, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}
