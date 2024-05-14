using UnityEngine;

namespace Simplicity.Noise
{
    using System;

    public static class PerlinNoise
    {
        public static float[,] GeneratePerlinNoise(int width, int height, int seed, float scale, int octaves,
            float persistence, float lacunarity, Vector2 offset)
        {
            Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];

            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-10000, 10000) + offset.x;
                float offsetY = prng.Next(-10000, 10000) + offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            float[,] noise = new float[width, height];

            if (scale <= 0)
                scale = 0.0001f;

            float minNoise = float.MaxValue;
            float maxNoise = float.MinValue;

            float halfWidth = width / 2.0f;
            float halfHeight = height / 2.0f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1.0f;
                    float frequency = 1.0f;
                    float noiseHeight = 0.0f;

                    for (int o = 0; o < octaves; o++)
                    {
                        float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[o].x;
                        float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[o].y;

                        float perlin = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                        noiseHeight += perlin * amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxNoise)
                        maxNoise = noiseHeight;
                    else if (noiseHeight < minNoise)
                        minNoise = noiseHeight;

                    noise[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    noise[x, y] = Mathf.InverseLerp(minNoise, maxNoise, noise[x, y]);
                }
            }

            return noise;
        }
    }
}