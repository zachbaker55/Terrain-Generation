using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public static class Noise {
    public static float[,] GenerateNoiseMap(int width, int height, System.Random rng, NoiseSetting noiseSetting) {
        float scale = noiseSetting.scale;
        int octaves = noiseSetting.octaves;
        float persistance = noiseSetting.persistance;
        float lacunarity = noiseSetting.lacunarity;
        Vector2 offset = noiseSetting.offset;

        float[,] noiseMap = new float[width,height];

        Vector2[] octaveOffsets = new Vector2[octaves];
        
        for (int o = 0; o < octaves; o++) {
            float offsetX = rng.Next(-100000,100000) + offset.x;
            float offsetY = rng.Next(-100000,100000) + offset.y;
            octaveOffsets[o] = new Vector2(offsetX, offsetY);

        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                //octave loop being inside seems kind of odd to me...
                    for (int o = 0; o < octaves; o++) {
                    float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[o].x;
                    float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[o].y;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; //multiplied so in Range(-1,1)
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (noiseHeight > maxNoiseHeight) { maxNoiseHeight = noiseHeight; 
                } else if (noiseHeight < minNoiseHeight) { minNoiseHeight = noiseHeight; }
                noiseMap[x,y] = noiseHeight;;
            }
        }

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
            }
        }
        return noiseMap;
    }
}
