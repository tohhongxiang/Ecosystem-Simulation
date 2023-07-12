using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public GameObject previewTexture;
    public GameObject previewMesh;

    public enum DrawMode { NoiseMap, FalloffMap, Mesh, Full };
    public DrawMode drawMode;
    public HeightMapSettings heightMapSettings;
    public MeshSettings meshSettings;
    public TerrainObjectGenerator[] terrainObjectSpawners;

    public void DrawMapInEditor()
    {
        if (terrainObjectSpawners != null)
        {
            foreach (TerrainObjectGenerator terrainObjectSpawner in terrainObjectSpawners)
            {
                terrainObjectSpawner.ClearObjects();
            }
        }

        // drawing fall off map does not require generation of heightmap, so we place it above
        if (drawMode == DrawMode.FalloffMap)
        {
            float[,] falloffMap = FalloffMapGenerator.GenerateFalloffMap(meshSettings.chunkSize, heightMapSettings.falloffParameters);
            Texture2D texture = TextureGenerator.TextureFromHeightMap(falloffMap);
            DrawTexture(texture);
            return;
        }

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(Vector2.zero, heightMapSettings, meshSettings);

        if (drawMode == DrawMode.NoiseMap)
        {
            Texture2D texture = TextureGenerator.TextureFromHeightMap(heightMap.values);
            DrawTexture(texture);
        }
        else if (drawMode == DrawMode.Mesh)
        {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap.values, heightMapSettings.heightMultiplier, heightMapSettings.heightCurve);
            DrawMesh(meshData);
        }
        else if (drawMode == DrawMode.Full)
        {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap.values, heightMapSettings.heightMultiplier, heightMapSettings.heightCurve);
            DrawMesh(meshData);

            if (terrainObjectSpawners != null)
            {
                foreach (TerrainObjectGenerator terrainObjectSpawner in terrainObjectSpawners)
                {
                    terrainObjectSpawner.SpawnObjects(previewMesh.GetComponent<Renderer>().bounds);
                }
            }
        }
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnValidate()
    {
        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (terrainObjectSpawners != null)
        {
            foreach (TerrainObjectGenerator terrainObjectSpawner in terrainObjectSpawners)
            {
                terrainObjectSpawner.OnValuesUpdated -= OnValuesUpdated;
                terrainObjectSpawner.OnValuesUpdated += OnValuesUpdated;
            }
        }
    }

    public void DrawTexture(Texture2D texture)
    {
        Renderer textureRenderer = previewTexture.GetComponent<Renderer>();

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        previewTexture.gameObject.SetActive(true);
        previewMesh.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        MeshFilter meshFilter = previewMesh.GetComponent<MeshFilter>();
        MeshCollider meshCollider = previewMesh.GetComponent<MeshCollider>();
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshCollider.sharedMesh = meshFilter.sharedMesh;

        previewTexture.gameObject.SetActive(false);
        previewMesh.gameObject.SetActive(true);
    }

}
