using UnityEngine;

public class MultipleTerrainObjectGenerator : TerrainObjectGenerator
{
    public GameObject[] prefabs;

    [Header("Constraints")]
    [Range(0, 1)] public float minimumSpawnHeight = 0;
    [Range(0, 1)] public float maximumSpawnHeight = 1;
    [Tooltip("Maximum angle between the final prefab rotation from vertical")] public float maxAngleFromVertical = 0;

    [Header("Randomisation Parameters")]
    [Tooltip("Total prefabs to spawn per meter square")] public float numberOfPrefabsPer100MetersSquared = 1;
    public float minScale = 1;
    public float maxScale = 1;
    [Tooltip("Max randomised offset rotation from the normal")] 
    public Vector3 maxRotationOffset = new Vector3(0, 360, 0);
    [Tooltip("Offset from intersection with ground to spawn prefab in")] 
    public Vector3 offsetFromGround = new Vector3(0, 0, 0);

    const int maxTries = 10;

    public override void SpawnObjects(Bounds spawnAreaBounds)
    {
        ClearObjects();

        float totalSurfaceArea = spawnAreaBounds.extents.x * 2 * spawnAreaBounds.extents.z * 2;
        int totalPrefabCount = (int)(totalSurfaceArea * numberOfPrefabsPer100MetersSquared / 100);

        float minimumHeightToSpawnPrefab = Mathf.Lerp(spawnAreaBounds.min.y, spawnAreaBounds.max.y, minimumSpawnHeight);
        float maximumHeightToSpawnPrefab = Mathf.Lerp(spawnAreaBounds.min.y, spawnAreaBounds.max.y, maximumSpawnHeight);

        for (int i = 0; i < totalPrefabCount; i++)
        {
            for (int j = 0; j < maxTries; j++)
            {
                float randomX = Random.Range(spawnAreaBounds.min.x, spawnAreaBounds.max.x);
                float randomZ = Random.Range(spawnAreaBounds.min.z, spawnAreaBounds.max.z);
                float highYCoordinate = 100;

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

                Vector3 scale = new Vector3(Random.Range(minScale, maxScale), Random.Range(minScale, maxScale), Random.Range(minScale, maxScale));
                instantiatedPrefab.transform.localScale = scale;
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

