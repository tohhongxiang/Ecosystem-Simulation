using UnityEngine;

public class WaterGenerator : TerrainObjectGenerator
{
    public GameObject waterPrefab;
    [Range(0, 1)] public float waterHeight;

    public override void SpawnObjects(Bounds bounds)
    {
        ClearObjects();

        Vector3 chunkCenterWithoutVertical = new Vector3(bounds.center.x, 0, bounds.center.z);
        GameObject spawnedWater = Instantiate(waterPrefab, chunkCenterWithoutVertical, Quaternion.identity, gameObject.transform);

        // a plane with the same scale as a cube, is 10 times as large
        int planeSizeMultiplier = 10;
        Vector3 spawnedWaterScale = bounds.size / planeSizeMultiplier;
        spawnedWaterScale.y = 1;
        spawnedWater.transform.localScale = spawnedWaterScale;

        float spawnedWaterHeight = Mathf.Lerp(bounds.min.y, bounds.max.y, waterHeight);
        spawnedWater.transform.position = new Vector3(spawnedWater.transform.position.x, spawnedWaterHeight, spawnedWater.transform.position.z);
    }

    public override void ClearObjects()
    {
        while (transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}
