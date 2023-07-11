using UnityEngine;

public static class FalloffMapGenerator
{
    public static float[,] GenerateFalloffMap(int size, FalloffParameters falloffParameters)
    {
        return GenerateFallOffMapForSpecificChunk(size, falloffParameters, 1, Vector2Int.zero);
    }

    public static float[,] GenerateFallOffMapForSpecificChunk(int chunkSize, FalloffParameters falloffParameters, int chunksPerSide, Vector2Int chunkPosition)
    {
        float[,] falloffMap = new float[chunkSize, chunkSize];

        Vector2Int startingIndex = new Vector2Int(chunkPosition.x * chunkSize, chunkPosition.y * chunkSize);
        Vector2Int endingIndex = new Vector2Int((chunkPosition.x + 1) * chunkSize, (chunkPosition.y + 1) * chunkSize);
        int totalSize = chunkSize * chunksPerSide;

        for (int i = startingIndex.x, ii = 0; i < endingIndex.x; i++, ii++)
        {
            for (int j = startingIndex.y, jj = 0; j < endingIndex.y; j++, jj++)
            {
                float x = i / (float)totalSize * 2 - 1;
                float y = j / (float)totalSize * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                if (value <= falloffParameters.falloffStart)
                {
                    falloffMap[ii, jj] = 0;
                }
                else if (value >= falloffParameters.falloffEnd)
                {
                    falloffMap[ii, jj] = 1;
                }
                else
                {
                    falloffMap[ii, jj] = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(falloffParameters.falloffStart, falloffParameters.falloffEnd, value));
                }
            }
        }

        return falloffMap;
    }
}

[System.Serializable]
public class FalloffParameters {
    [Range(0, 1)] public float falloffStart = 0.8f;
    [Range(0, 1)] public float falloffEnd = 0.99f;
}