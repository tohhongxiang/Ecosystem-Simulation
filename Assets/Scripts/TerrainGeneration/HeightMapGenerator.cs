using UnityEngine;

public static class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(Vector2 center, HeightMapSettings heightMapSettings, MeshSettings meshSettings)
    {
        return GenerateHeightMapForSpecificChunk(center, Vector2Int.zero, 1, heightMapSettings, meshSettings);
    }

    // we use the falloff map on a global basis, rather than a per-chunk basis
    public static HeightMap GenerateHeightMapForSpecificChunk(Vector2 center, Vector2Int chunkIndex, int numberOfChunks, HeightMapSettings heightMapSettings, MeshSettings meshSettings)
    {
        // expand noisemap to include border so that we can calculate UVs between chunks
        const int borderSize = 1;
        int overallSize = meshSettings.chunkSize + 2 * borderSize;

        float[,] noiseMap = Noise.GenerateNoiseMap(overallSize, overallSize, heightMapSettings.noiseParameters, center);

        if (heightMapSettings.useFalloff)
        {
            float[,] fallOffMap = FalloffMapGenerator.GenerateFallOffMapForSpecificChunk(overallSize, heightMapSettings.falloffParameters, numberOfChunks, chunkIndex);

            for (int y = 0; y < overallSize; y++)
            {
                for (int x = 0; x < overallSize; x++)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallOffMap[x, y]);
                }
            }
        }

        return new HeightMap(noiseMap);
    }
}

public struct HeightMap
{
    public readonly float[,] values;

    public HeightMap(float[,] values)
    {
        this.values = values;
    }
}
