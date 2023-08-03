using UnityEngine;

public static class Noise
{
    public enum NormalizeMode { Local, Global };

    public static float[,] GenerateNoiseMap(int width, int height, NoiseParameters noiseParameters, Vector2 center)
    {
        float[,] noiseMap = new float[width, height];

        float maxPossibleHeight = 0;

        float amplitude = 1;
        float frequency = 1;

        // for each octave, we want to sample from a different point
        System.Random prng = new System.Random(noiseParameters.seed);
        Vector2[] octaveOffsets = new Vector2[noiseParameters.octaves];
        for (int i = 0; i < noiseParameters.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + noiseParameters.offset.x + center.x;
            float offsetY = prng.Next(-100000, 100000) - noiseParameters.offset.y - center.y;

            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= noiseParameters.persistence;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                amplitude = 1;
                frequency = 1;
                float currentNoiseHeight = 0;

                for (int i = 0; i < noiseParameters.octaves; i++)
                {
                    float sampleX = (x - width / 2f + octaveOffsets[i].x) / noiseParameters.scale * frequency;
                    float sampleY = (y - height / 2f + octaveOffsets[i].y) / noiseParameters.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    currentNoiseHeight += perlinValue * amplitude;

                    amplitude *= noiseParameters.persistence;
                    frequency *= noiseParameters.lacunarity;
                }

                maxLocalNoiseHeight = Mathf.Max(maxLocalNoiseHeight, currentNoiseHeight);
                minLocalNoiseHeight = Mathf.Min(minLocalNoiseHeight, currentNoiseHeight);
                noiseMap[x, y] = currentNoiseHeight;
            }
        }

        // normalize values back to 0-1
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (noiseParameters.normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    noiseMap[x, y] = Mathf.Clamp((noiseMap[x, y] + 1) / maxPossibleHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }
}

[System.Serializable]
public class NoiseParameters
{
    public Noise.NormalizeMode normalizeMode;

    public float scale = 50;

    public int octaves = 6;
    [Range(0, 1)]
    public float persistence = .6f;
    public float lacunarity = 2;

    public int seed;
    public Vector2 offset;

    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistence = Mathf.Clamp01(persistence);
    }
}
