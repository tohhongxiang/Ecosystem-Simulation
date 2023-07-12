using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGenerator : MonoBehaviour
{
    public GameObject waterPrefab;
    [Range(0, 1)] public float waterHeight;

    public void SpawnWater(Bounds waterBounds)
    {
        ClearSpawnedWater();

        Vector3 chunkCenterWithoutVertical = new Vector3(waterBounds.center.x, 0, waterBounds.center.z);
        GameObject spawnedWater = Instantiate(waterPrefab, chunkCenterWithoutVertical, Quaternion.identity, gameObject.transform);

        Vector3 spawnedWaterScale = waterBounds.size;
        spawnedWaterScale.y *= waterHeight;
        spawnedWater.transform.localScale = spawnedWaterScale;
    }

    public void ClearSpawnedWater()
    {
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}
