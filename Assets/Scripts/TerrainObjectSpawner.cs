using UnityEngine;

public class TerrainObjectSpawner : MonoBehaviour
{
    public GameObject[] prefabs;
    public float minSpawnHeight;
    public float maxSpawnHeight;
    public int amount;

    public void SpawnTerrainObjects(Bounds spawnBounds)
    {
        for (int i = 0; i < amount; i++)
        {
            while (true)
            {
                float randomX = Random.Range(spawnBounds.min.x, spawnBounds.max.x);
                float randomZ = Random.Range(spawnBounds.min.z, spawnBounds.max.z);
                float highYCoordinate = 100;

                Vector3 raycastStart = new Vector3(randomX, highYCoordinate, randomZ);
                Ray ray = new Ray(raycastStart, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit info, highYCoordinate))
                {
                    // todo fix trees spawning on top of trees
                    Vector3 hitPosition = info.point;
                    if (hitPosition.y > minSpawnHeight && hitPosition.y < maxSpawnHeight)
                    {
                        GameObject prefab = prefabs[Random.Range(0, prefabs.Length - 1)];
                        Instantiate(prefab, hitPosition, Quaternion.identity, gameObject.transform);
                        break;
                    }
                }
            }

        }
    }
}
