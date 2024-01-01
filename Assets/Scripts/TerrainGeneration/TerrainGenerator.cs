using UnityEngine;
using UnityEngine.AI;

public class TerrainGenerator : MonoBehaviour
{
    public NavMeshSurface surface;
    public HeightMapSettings heightMapSettings;
    public MeshSettings meshSettings;
    public Material terrainMaterial;
    public TerrainObjectGenerator[] terrainObjectSpawners;

    void Start()
    {
        Bounds chunkBounds = SpawnChunks();
        
        foreach (TerrainObjectGenerator terrainObjectSpawner in terrainObjectSpawners) {
            terrainObjectSpawner.SpawnObjects(chunkBounds);
        }

        surface.BuildNavMesh();
    }

    Bounds SpawnChunks()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.one);

        for (int x = 0; x < meshSettings.numberOfChunks; x++)
        {
            for (int z = 0; z < meshSettings.numberOfChunks; z++)
            {
                // going down is negative for the z direction
                Vector2 chunkCoordinates = new Vector2(x * (meshSettings.chunkSize - 1), -z * (meshSettings.chunkSize - 1));

                // create the terrain chunk mesh
                GameObject meshObject = new GameObject("Terrain Chunk: (" + chunkCoordinates.x + "," + chunkCoordinates.y + ")");
                meshObject.transform.position = new Vector3(chunkCoordinates.x, 0, chunkCoordinates.y) * meshSettings.scale;
                meshObject.transform.parent = gameObject.transform;
                meshObject.transform.localScale = Vector3.one * meshSettings.scale;
                meshObject.tag = "Terrain";
                meshObject.layer = gameObject.layer;

                // create meshrenderer to apply material
                MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshRenderer.material = terrainMaterial;
                meshRenderer.material.mainTextureOffset += chunkCoordinates * meshSettings.scale;

                // create meshFilter and meshCollider to apply mesh
                MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
                MeshCollider meshCollider = meshObject.AddComponent<MeshCollider>();

                HeightMap heightMap = HeightMapGenerator.GenerateHeightMapForSpecificChunk(chunkCoordinates, new Vector2Int(x, z), meshSettings.numberOfChunks, heightMapSettings, meshSettings);
                MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap.values, heightMapSettings.heightMultiplier, heightMapSettings.heightCurve);

                meshFilter.sharedMesh = meshData.CreateMesh();
                meshCollider.sharedMesh = meshFilter.sharedMesh;

                Bounds meshBounds = meshObject.GetComponent<MeshCollider>().bounds;
                bounds.Encapsulate(meshBounds);
            }
        }

        return bounds;
    }
}

