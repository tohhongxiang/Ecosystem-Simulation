using UnityEngine;

public class TerrainObjectSpawner : MonoBehaviour
{
    public GameObject[] prefabs;

    public float minSpawnHeight;
    public float maxSpawnHeight;

    public float maxAngleFromVertical;

    public float minScale = 1;
    public float maxScale = 1;

    public int amount;

    const int maxTries = 100;

    public void SpawnTerrainObjects(Bounds spawnAreaBounds)
    {
        ClearTerrainObjects();

        for (int i = 0; i < amount; i++)
        {
            for (int j = 0;  j < maxTries; j++)
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
                if (info.transform.tag != "Terrain") {
                    continue;
                }

                Vector3 hitPosition = info.point;

                // if spawn position does not satisfy height constraints, retry
                if (hitPosition.y < minSpawnHeight || hitPosition.y > maxSpawnHeight)
                {
                    continue;
                }

                // make sure angle is within constraints
                float angleBetweenNormalAndVertical = Vector3.Angle(Vector3.up, info.normal);
                if (Mathf.Abs(angleBetweenNormalAndVertical) > maxAngleFromVertical)
                {
                    continue;
                }

                GameObject prefab = prefabs[Random.Range(0, prefabs.Length - 1)];
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, info.normal);
                GameObject instantiatedPrefab = Instantiate(prefab, hitPosition, rotation, gameObject.transform);
                Vector3 scale = new Vector3(Random.Range(minScale, maxScale), Random.Range(minScale, maxScale), Random.Range(minScale, maxScale));
                instantiatedPrefab.transform.localScale = scale;
                break;
            }

        }
    }

    public void ClearTerrainObjects() {
        while (transform.childCount != 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    void OnValidate() {
        if (maxScale < 0) {
            maxScale = 1;
        }

        if (minScale < 0) {
            minScale = 1;
        }
    }
}
