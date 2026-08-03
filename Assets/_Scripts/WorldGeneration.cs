using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public static class WorldGeneration {
    public enum DrawMode {TemperatureMap, PrecipitationMap, MoralityMap, FlatnessMap, ElevationMap, NoiseMap, 
    VoronoiMap, GradientMap, FalloffMap, IslandMap, InlandnessMap, BiomeMap, TreeDensityMap, TreePoissonMap, TestSamplingMap};
    
    #region variables
    private static Stopwatch stopwatch;

    // World generation settings
    private static GenerationSettings genSettings;
    private static int width = 100;
    private static int height = 100;
    private static int depth = 10;
    private static System.Random rng = new System.Random(0);
    public static System.Random RandomNumberGenerator {
        get { return rng; }
    }

    // Island generation settings
    private static int numberOfIslands = 1;
    private static float islandRegionVariance = 0.9f;
    private static float minimumDistanceBetweenIslands = 20f;
    private static int maxIslandAttempts = 100;
    private static float islandCenterBiasStrength = 3f;
    private static float islandFalloff = 5f;
    private static float voronoiFalloff = 4f;
    private static float islandXMargin = 0.15f;
    private static float islandYMargin = 0.15f;
    private static float islandWaterCutoff = 0.5f;
    
    // Feature settings
    private static float treeBlendFalloffStrength = 6f;
    private static float minPoissonRadius = 2f;
    private static float maxPoissonRadius = 50f;
    private static int maxPoissonAttempts = 3;

    // Biome data
    private static BiomeData[] biomeData;
    private static int fillBiome = 0;

    // Generated map data
    
    // island maps
    private static float [,] islandNoiseMap;
    private static int [,] voronoiMap;
    private static float [,] gradientMap;
    private static float [,] falloffMap;
    private static int [,] islandMap;
    private static float [,] inlandnessMap;

    // biome maps
    private static float [,] temperatureMap;
    private static float [,] precipitationMap;
    private static float [,] moralityMap;
    private static int[,] biomeMap;


    // height maps
    private static float [,] heightFlatnessMap;
    private static float [,] heightElevationMap;
    private static float [,] heightNoiseMap;

    // feature maps
    private static float[,] treeDensityMap;
    private static int[,] treePoissonMap;
    public static int[,] TreeLocationMap {
        get { return treePoissonMap; }
    }

    #endregion
    #region main methods

    // Methods

    public static void SetGenerationSettings(int w, int h, int d, int seed, 
            int noIsle, float isleVar, float minDistIsle, int maxIsleAttempt, float isleCenterBias, float isleFall, float voroFall, 
            float isleXMargin, float isleYMargin, float isleWaterCut, 
            float treeBFStrength, float minPRadius, float maxPRadius, int maxPAttempts) {
        //Set world generation settings
        width = w;
        height = h;
        depth = d;
        rng = new System.Random(seed);
        
        //Set feature generation settings
        treeBlendFalloffStrength = treeBFStrength;
        minPoissonRadius = minPRadius;
        maxPoissonRadius = maxPRadius;
        maxPoissonAttempts = maxPAttempts;


        //Set island generation settings
        numberOfIslands = noIsle;
        islandRegionVariance = isleVar;
        minimumDistanceBetweenIslands = minDistIsle;
        maxIslandAttempts = maxIsleAttempt;
        islandCenterBiasStrength = isleCenterBias;
        islandFalloff = isleFall;
        voronoiFalloff = voroFall;
        islandXMargin = isleXMargin;
        islandYMargin = isleYMargin;
        islandWaterCutoff = isleWaterCut;

        stopwatch = new Stopwatch();
    }

    public static void SetBiomes(BiomeData[] bData, string fBiome) {
        biomeData = bData;
        fillBiome = 0;
        for (int b = 0; b < biomeData.Length; b++) {
            if (biomeData[b].GetName() == fBiome) {
                fillBiome = b;
            }
        }
    }

    // Call SetGenerationSettings and SetBiomes before this
    public static void GenerateMapData(NoiseSetting islandNoise, NoiseSetting biomeTemperature, NoiseSetting biomePrecipitation, NoiseSetting biomeMorality,
    NoiseSetting heightFlatness, NoiseSetting heightElevation, NoiseSetting heightNoise) {

        stopwatch.Start();
        
        // Generate height noise
        heightNoiseMap = Noise.GenerateNoiseMap(width, height, rng, heightNoise);

        // Generate Island Map
        islandNoiseMap = Noise.GenerateNoiseMap(width, height, rng, islandNoise);
        voronoiMap = GenerateVoronoiMap();
        gradientMap = CalculateVoronoiGradient(voronoiMap);
        falloffMap = GenerateFalloffMap();
        islandMap = GenerateIslandMap(islandNoiseMap, gradientMap, falloffMap);
        inlandnessMap = GenerateInlandnessMap(islandMap);
        
        // Generate flatness & elevation
        heightFlatnessMap = Noise.GenerateNoiseMap(width, height, rng, heightFlatness);
        heightElevationMap = Noise.GenerateNoiseMap(width, height, rng, heightElevation);


        // Generate biome maps
        temperatureMap = Noise.GenerateNoiseMap(width, height, rng, biomeTemperature);
        precipitationMap = Noise.GenerateNoiseMap(width, height, rng, biomePrecipitation);
        moralityMap = Noise.GenerateNoiseMap(width, height, rng, biomeMorality);
        biomeMap = GenerateBiomes(islandMap,temperatureMap, precipitationMap, moralityMap, inlandnessMap, heightFlatnessMap, heightElevationMap);
        //treeDensityMap also is created by GeneratedBiomes()

        // Generate feature maps
        treePoissonMap = GenerateTreeDensityPoissonDiscMap();

        stopwatch.Stop();
        //Debug.Log("Noise Maps generated: " + stopwatch.ElapsedMilliseconds + "ms");
        stopwatch.Reset();
    }

    // Call SetGenerationSettings, SetBiomes, and GenerateMapData before this
    public static Texture2D GenerateMapTexture(DrawMode drawMode) {

        // Set color array for minimap
        Color[] colorMap;
        switch (drawMode) {
            case DrawMode.TemperatureMap:
                colorMap = GenerateNoiseColorMap(temperatureMap);
                break;
            case DrawMode.PrecipitationMap:
                colorMap = GenerateNoiseColorMap(precipitationMap);
                break;
            case DrawMode.MoralityMap:
                colorMap = GenerateNoiseColorMap(moralityMap);
                break;
            case DrawMode.FlatnessMap:
                colorMap = GenerateNoiseColorMap(heightFlatnessMap);
                break;
            case DrawMode.ElevationMap:
                colorMap = GenerateNoiseColorMap(heightElevationMap);
                break;
            case DrawMode.NoiseMap:
                colorMap = GenerateNoiseColorMap(heightNoiseMap);
                break;
            case DrawMode.VoronoiMap:
                colorMap = GenerateRegionColorMap(voronoiMap, numberOfIslands);
                break;
            case DrawMode.GradientMap:
                colorMap = GenerateNoiseColorMap(gradientMap);
                break;
            case DrawMode.FalloffMap:
                colorMap = GenerateNoiseColorMap(falloffMap);
                break;
            case DrawMode.IslandMap:
                colorMap = GenerateNoiseColorMap(islandMap);
                break;
            case DrawMode.InlandnessMap:
                colorMap = GenerateNoiseColorMap(inlandnessMap);
                break;
            case DrawMode.BiomeMap:
                colorMap = GenerateBiomeColorMap(biomeMap);
                break;
            case DrawMode.TreeDensityMap:
                colorMap = GenerateDensityColorMap(treeDensityMap, maxPoissonRadius);
                break;
            case DrawMode.TreePoissonMap:
                colorMap = GenerateNoiseColorMap(treePoissonMap);
                break;
            case DrawMode.TestSamplingMap:
                colorMap = GenerateNoiseColorMap(GenerateGridSamplingMap(6, 8f));
                break;
            default:
                colorMap = new Color[width * height];
                break;
        }

        // Return minimap texture
        Texture2D texture = GenerateEmptyTexture();
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }



    // Call SetGenerationSettings, SetBiomes, and GenerateMapData before this
    public static int[,,] GenerateWorld(GenerationSettings gSettings) {
        genSettings = gSettings;


        // Initialize worldData to -1
        int[,,] worldData = InitWorldData();

        // Generating terrain
        stopwatch.Start();
        GenerateBaseTerrain(worldData);
        stopwatch.Stop();
        Debug.Log("Terrain generated: " + stopwatch.ElapsedMilliseconds + "ms");
        stopwatch.Reset();


        // Generating features
        stopwatch.Start();
        PlaceFeatures(worldData);
        stopwatch.Stop();
        Debug.Log("Features placed: " + stopwatch.ElapsedMilliseconds + "ms");
        stopwatch.Reset();

        // Generating ???

        return worldData;
    }

    #endregion
    #region color maps

    private static Color[] GenerateNoiseColorMap(float[,] map) {
        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colorMap[y*width + x] = Color.Lerp(Color.black, Color.white, map[x,y]);
            }
        }
        return colorMap;
    }
    private static Color[] GenerateNoiseColorMap(int[,] map) {
        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (map[x,y] == 0) {
                    colorMap[y*width + x] = Color.black;
                } else {
                    colorMap[y*width + x] = Color.white;
                }
            }
        }
        return colorMap;
    }

    private static Color[] GenerateRegionColorMap(int[,] map, int max) {
        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                float pos = MathUtils.MapIntToUnitFloat(map[x,y], 0, max);
                colorMap[y*width + x] = Color.Lerp(Color.black, Color.white, pos);
            }
        }
        return colorMap;
    }

    private static Color[] GenerateDensityColorMap(float[,] map, float max) {
        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                float pos = MathUtils.DensityMapFloatToUnitFloat(map[x,y], 0, max);
                colorMap[y*width + x] = Color.Lerp(Color.black, Color.white, pos);
            }
        }
        return colorMap;
    }

    private static Color[] GenerateDropoffColorMap(float[,] map) {
        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (map[x,y] >= islandWaterCutoff) {
                    colorMap[y*width + x] = Color.white;
                } else {
                    colorMap[y*width + x] = Color.black;
                }
            }
        }
        return colorMap;
    }

    private static Color[] GenerateBiomeColorMap(int[,] map) {
        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colorMap[y*width + x] = biomeData[map[x,y]].GetColor();
            }
        }
        return colorMap;
    }

    private static Color[] GenerateWorldMap(int[,] biomeMap, float[,] islandMap) {
        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                colorMap[y*width + x] = biomeData[biomeMap[x,y]].GetColor();
            }
        }
        return colorMap;
    }

    private static Texture2D GenerateEmptyTexture() {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }

    #endregion
    #region map gen
    
    private static int[,] GenerateVoronoiMap() {
        float[,] islandMap = new float[width,height];

        List<Vector2Int> points = GeneratePoints();
        foreach (var p in points) {
            islandMap[p.x,p.y] = 1f;
        }

        int[,] voronoiMap = GeneratePureVoronoiMap(points);
        return voronoiMap;
    }

    private static List<Vector2Int> GeneratePoints() {
        List<Vector2Int> points = new List<Vector2Int>();
        int attempts = 0;
        while (points.Count < numberOfIslands && attempts < maxIslandAttempts) {
            Vector2Int candidate = GenerateCenterBiasedPoint();
            attempts++;
            bool isFarEnough = true;
            foreach (var p in points) {
                if (Vector2.Distance(p, candidate) < minimumDistanceBetweenIslands) {
                    isFarEnough = false;
                    break;
                }
            }
            if (isFarEnough) {
                points.Add(candidate);
                attempts = 0;
            }
            if (attempts >= maxIslandAttempts) {
                Debug.LogWarning("Reached maximum attempts. Could not place all points with spacing.");
            }

        }
        return points;
    }

    private static Vector2Int GenerateCenterBiasedPoint() {
        float rx = 0f;
        float ry = 0f;
        
        for (int i = 0; i < islandCenterBiasStrength; i++) {
            rx += (float)rng.NextDouble();
            ry += (float)rng.NextDouble();
        }

        rx /= islandCenterBiasStrength;
        ry /= islandCenterBiasStrength;

        float xMargin = width * islandXMargin;
        float yMargin = height * islandYMargin;

        int x = (int)Mathf.Lerp(xMargin, width - xMargin, Mathf.SmoothStep(0, 1, rx));
        int y = (int)Mathf.Lerp(yMargin, height - yMargin, Mathf.SmoothStep(0, 1, ry));
        
        return new Vector2Int(x, y);
    }

    private static int[,] GeneratePureVoronoiMap(List<Vector2Int> points) {
        int[,] regionMap = new int[width,height];
        List<float> influences = new List<float>();
        
        foreach (Vector2Int p in points) {
            // Each center gets a random influence between 1-islandRegionVariance and 1+islandRegionVariance
            float random = (float) rng.NextDouble();
            double rangeStart = 1.0 - islandRegionVariance;
            double rangeEnd = 1.0 + islandRegionVariance;
            float influence = (float)(rangeStart + random * (rangeEnd - rangeStart));
            influences.Add(influence);
        }

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                rng.NextDouble();
                int closestPoint = 0;
                float closestDistance = float.MaxValue;

                for (int i = 0; i < points.Count; i++) {
                    
                    float distSqr = (points[i].x - x) * (points[i].x - x) + (points[i].y - y) * (points[i].y - y);
                    float influencedDist = distSqr * influences[i] * influences[i];  //square influences as well
                    if (influencedDist < closestDistance)
                    {
                        closestDistance = influencedDist;
                        closestPoint = i;
                    }
                }

                regionMap[x, y] = closestPoint;
            }
        }

        return regionMap;
    }
    public static float[,] CalculateVoronoiGradient(int[,] voronoiRegions) {
    float[,] edgeGradient = new float[width, height];

    Vector2Int[] directions = new Vector2Int[] {
        new Vector2Int(-1, 0), new Vector2Int(1, 0),
        new Vector2Int(0, -1), new Vector2Int(0, 1),
        new Vector2Int(-1, -1), new Vector2Int(1, -1),
        new Vector2Int(-1, 1), new Vector2Int(1, 1)
    };

    //Initialize with MaxValue
    for (int x = 0; x < width; x++) {
        for (int y = 0; y < height; y++) {
            
        }
    }

    Queue<Vector2Int> queue = new Queue<Vector2Int>();

    //1: Mark edge tiles
    for (int x = 0; x < width; x++) {
        for (int y = 0; y < height; y++) {
            edgeGradient[x, y] = float.MaxValue;
            int currentRegion = voronoiRegions[x, y];
            bool isEdgeTile = false;

            foreach (var dir in directions) {
                int nx = x + dir.x;
                int ny = y + dir.y;

                if (nx >= 0 && ny >= 0 && nx < width && ny < height) {
                    if (voronoiRegions[nx, ny] != currentRegion) {
                        isEdgeTile = true;
                        break;
                    }
                }
            }

            if (isEdgeTile) {
                edgeGradient[x, y] = 1f;
                queue.Enqueue(new Vector2Int(x, y));
            }
        }
    }

    // Step 2: BFS
    while (queue.Count > 0) {
        Vector2Int currentTile = queue.Dequeue();
        int x = currentTile.x;
        int y = currentTile.y;

        foreach (var dir in directions) {
            int nx = x + dir.x;
            int ny = y + dir.y;

            if (nx >= 0 && ny >= 0 && nx < width && ny >= 0 && ny < height) {
                if (edgeGradient[nx, ny] == float.MaxValue) {
                    // Instead of decreasing by 0.1f, divide by falloffStrength
                    edgeGradient[nx, ny] = edgeGradient[x, y] - (0.1f / voronoiFalloff);
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
    }

    // Step 3: Find maximum distance manually
    float maxDistance = float.MinValue;
    for (int x = 0; x < width; x++) {
        for (int y = 0; y < height; y++) {
            maxDistance = Mathf.Max(maxDistance, edgeGradient[x, y]);
        }
    }

    // Step 4: Normalize
    for (int x = 0; x < width; x++) {
        for (int y = 0; y < height; y++) {
            edgeGradient[x, y] = 1f- Mathf.InverseLerp(0f, maxDistance, edgeGradient[x, y]);
        }
    }

    return edgeGradient;
}


    public static float[,] GenerateFalloffMap() {
        float[,] falloffMap = new float[width, height];
        float centerX = width / 2f;
        float centerY = height / 2f;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                // Normalize coordinates to the center
                float normalizedX = (x - centerX) / centerX;
                float normalizedY = (y - centerY) / centerY;

                //Squircle equation
                float dist = Mathf.Pow(Mathf.Abs(normalizedX), islandFalloff) + Mathf.Pow(Mathf.Abs(normalizedY), islandFalloff);
                
                //Normalize the distance 
                falloffMap[x, y] = Mathf.Clamp01(1f-dist);
            }
        }

        return falloffMap;
    }


    public static int[,] GenerateIslandMap(float[,] noiseMap, float[,] voronoiMap, float[,] falloffMap) {
        int[,] islandMap = new int[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                // Combine voronoi, noise, and falloff, adjusting the falloff strength
                float noiseValue = noiseMap[x, y];
                float voronoiValue = voronoiMap[x, y];
                float falloffValue = falloffMap[x, y];
                float fullFalloffValue;

                if (numberOfIslands == 1) {
                    fullFalloffValue = falloffValue;
                } else {
                    //float fullFalloffValue = voronoiValue * falloffValue; Multiplication option
                    fullFalloffValue = Mathf.Min(voronoiValue, falloffValue); // Minimum option
                }

                float val = Mathf.Clamp01((noiseValue * (1 - islandFalloff)) + (fullFalloffValue * islandFalloff));
                if (val >= islandWaterCutoff) {
                    islandMap[x,y] = 1;
                } else {
                    islandMap[x,y] = 0;
                }
            }
        }

        return islandMap;
    }

    private static int[,] GenerateBiomes(int[,] islandMap, float[,] temperatureMap, float[,] precipitationMap, float[,] moralityMap, 
            float[,] inlandnessMap, float[,] flatnessMap, float[,] elevationMap) {
                
        int[,] biomeMap = new int[width,height];
        treeDensityMap = new float[width, height];

        // Find biome per tile
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                if (islandMap[x,y] == 0) {
                    biomeMap[x,y] = fillBiome;
                    treeDensityMap[x,y] = biomeData[fillBiome].TreeDensity;
                } else {

                    Vector6 sampleValues = new Vector6(temperatureMap[x,y], precipitationMap[x,y], moralityMap[x,y], inlandnessMap[x,y], flatnessMap[x,y], elevationMap[x,y]);

                    float totalWeight = 0f;
                    float weightedTreeDensity = 0f;

                    int closestBiome = fillBiome;
                    float closestDist = float.MaxValue;

                    for (int b = 0; b < biomeData.Length; b++) {
                        if (!biomeData[b].IsNormalGeneration()) continue;
                        float distance = Vector6.Distance(sampleValues, biomeData[b].GetValues());
                        distance = distance * 1/biomeData[b].SpawnStrength;
                        if (distance < closestDist) {
                            closestDist = distance;
                            closestBiome = b;
                        }

                        float gaussianWeight = Mathf.Exp(-Mathf.Pow(distance * treeBlendFalloffStrength, 2));
                        totalWeight += gaussianWeight;
                        weightedTreeDensity += biomeData[b].TreeDensity * gaussianWeight;
                    }

                    treeDensityMap[x,y] = weightedTreeDensity / totalWeight;
                    biomeMap[x,y] = closestBiome;

                }
            }
        }

        return biomeMap;
    }



    //Chamfer Distance Transform
    private static float[,] GenerateInlandnessMap(int[,] islandMap) {
        float[,] dist = new float[width, height];
        float maxDistance = width + height;

        // Initialize distances
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                dist[x, y] = islandMap[x, y] == 0 ? 0f : maxDistance;
            }
        }

        // Pass 1: top-left to bottom-right
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                // Check top-left neighbors
                if (x > 0) { //left
                    dist[x, y] = Mathf.Min(dist[x, y], dist[x - 1, y] + 3f);
                }
                if (y > 0) { //up
                    dist[x, y] = Mathf.Min(dist[x, y], dist[x, y - 1] + 3f);
                }
                if (x > 0 && y > 0) { //up-left
                    dist[x, y] = Mathf.Min(dist[x, y], dist[x - 1, y - 1] + 4f);
                }
                if (x < width - 1 && y > 0) {
                    dist[x, y] = Mathf.Min(dist[x, y], dist[x + 1, y - 1] + 4f);
                }
            }
        }

        // Pass 2: bottom-right to top-left
        for (int y = height - 1; y >= 0; y--) {
            for (int x = width - 1; x >= 0; x--) {
                // Check bottom-right neighbors
                if (x < width - 1) { //right
                    dist[x, y] = Mathf.Min(dist[x, y], dist[x + 1, y] + 3f);
                }
                if (y < height - 1) { //down
                    dist[x, y] = Mathf.Min(dist[x, y], dist[x, y + 1] + 3f);
                }
                if (x < width - 1 && y < height - 1) { //down-right
                    dist[x, y] = Mathf.Min(dist[x, y], dist[x + 1, y + 1] + 4f);
                }
                if (x > 0 && y < height - 1) { //down-left
                    dist[x, y] = Mathf.Min(dist[x, y], dist[x - 1, y + 1] + 4f);
                }
            }
        }

        return NormalizeDistanceMap(dist, islandMap);
    }


    private static float[,] NormalizeDistanceMap(float[,] distMap, int[,] islandMap) {
        float maxDist = 0f;

        // Only consider distances from 1 pixels
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (islandMap[x, y] == 1 && distMap[x, y] > maxDist)
                    maxDist = distMap[x, y];
            }
        }

        float[,] gradientMap = new float[width, height];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (islandMap[x, y] == 0) {
                    gradientMap[x, y] = 0f; // 0f oceans
                } else {
                    // Inland = brighter, Edge = darker
                    gradientMap[x, y] = distMap[x, y] / maxDist;
                }
            }
        }

        return gradientMap;
    }

    private static int[,] GenerateTreeDensityPoissonDiscMap(int currentDepth = 0) {
        float cellSize = minPoissonRadius / Mathf.Sqrt(2); // allows tighter memory layout

        int smallerDim = Mathf.Min(height, width);
        float maxPRadius = Mathf.Clamp(maxPoissonRadius, minPoissonRadius, Mathf.Floor(smallerDim / 3f));
        
        int gridWidth = Mathf.CeilToInt(width / cellSize);
        int gridHeight = Mathf.CeilToInt(height / cellSize);

        int?[,] grid = new int?[gridWidth, gridHeight]; // grid stores indices into sample list
        List<Vector2> samples = new List<Vector2>();
        List<Vector2> activeList = new List<Vector2>();

        float GetRadius(Vector2 pos) {
            int x = Mathf.Clamp(Mathf.FloorToInt(pos.x), 0, treeDensityMap.GetLength(0) - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(pos.y), 0, treeDensityMap.GetLength(1) - 1);
            return Mathf.Clamp(treeDensityMap[x, y], minPoissonRadius, maxPRadius);
        }

        Vector2 first = new Vector2((float)rng.NextDouble() * width, (float)rng.NextDouble() * height);
        samples.Add(first);
        activeList.Add(first);
        Vector2Int firstCell = new Vector2Int(Mathf.FloorToInt(first.x / cellSize), Mathf.FloorToInt(first.y / cellSize));
        grid[firstCell.x, firstCell.y] = 0;

        while (activeList.Count > 0) {
            int i = rng.Next(activeList.Count);
            Vector2 parent = activeList[i];
            float parentRadius = GetRadius(parent);

            bool found = false;
            for (int c = 0; c < maxPoissonAttempts; c++) {
                float angle = (float)rng.NextDouble() * 2 * Mathf.PI;
                float candidateRadius = (float)rng.NextDouble() * parentRadius + parentRadius;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 candidate = parent + dir * candidateRadius;

                if (candidate.x < 0 || candidate.y < 0 || candidate.x >= width || candidate.y >= height) continue;

                float candidateR = GetRadius(candidate);
                int cx = Mathf.FloorToInt(candidate.x / cellSize);
                int cy = Mathf.FloorToInt(candidate.y / cellSize);

                int sx0 = Mathf.Max(cx - 2, 0);
                int sy0 = Mathf.Max(cy - 2, 0);
                int sx1 = Mathf.Min(cx + 2, gridWidth - 1);
                int sy1 = Mathf.Min(cy + 2, gridHeight - 1);

                bool valid = true;
                for (int y = sy0; y <= sy1 && valid; y++) {
                    for (int x = sx0; x <= sx1; x++) {
                        int? idx = grid[x, y];
                        if (idx.HasValue) {
                            Vector2 neighbor = samples[idx.Value];
                            float neighborR = GetRadius(neighbor);
                            float minDist = Mathf.Max(candidateR, neighborR);
                            if (Vector2.SqrMagnitude(candidate - neighbor) < minDist * minDist) {
                                valid = false;
                                break;
                            }
                        }
                    }
                }

                if (valid) {
                    int newIndex = samples.Count;
                    samples.Add(candidate);
                    activeList.Add(candidate);
                    grid[cx, cy] = newIndex;
                    found = true;
                    break;
                }
            }

            if (!found) activeList.RemoveAt(i);
        }

        // Failsafe, try again. Should work eventually with new RNG. Given 50 tries
        // Bad way of doing this honestly
        if (samples.Count < 50) {
            if (currentDepth < 10) {
                return (GenerateTreeDensityPoissonDiscMap(currentDepth + 1));
            } else {
                Debug.LogError("Incredibly unlucky RNG? Tree map generation failed.");
            }
        }

        int[,] map = new int[width, height];
        foreach (Vector2 sample in samples) {
            int xVal = Mathf.RoundToInt(sample.x);
            int yVal = Mathf.RoundToInt(sample.y);
            if (xVal < 0 || yVal < 0 || xVal >= width || yVal >= height) continue;
            map[xVal, yVal] = 1;
        }

        return map;
    }


    private static int[,] GenerateGridSamplingMap(int cellSize, float randomJitterStrength) {
        int[,] map = new int[width, height];

        // use width and height to form number of cells
        // but if it doesn't divide evenly, lets pretend it does.
        int addedWidth = (cellSize - (width % cellSize)) % cellSize;
        int addedHeight = (cellSize - (height % cellSize)) % cellSize;
        int pretendWidth = width + addedWidth;
        int pretendHeight = height + addedHeight;
    
        int noOfCellsX = pretendWidth / cellSize;
        int noOfCellsY = pretendHeight / cellSize;
        float halfCellSize = cellSize / 2f;

        for (int x = 0; x < noOfCellsX; x++) {
            for (int y = 0; y < noOfCellsY; y++) {
                float angle = (float) rng.NextDouble() * 2f * Mathf.PI;
                float jitterStrength = (float)rng.NextDouble() * randomJitterStrength;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 relativeDotPosition = dir * jitterStrength;
                Vector2 worldDotPosition = relativeDotPosition + new Vector2(x * cellSize + halfCellSize, y * cellSize + halfCellSize);

                Vector2Int intPosition = new Vector2Int(Mathf.FloorToInt(worldDotPosition.x), Mathf.FloorToInt(worldDotPosition.y));
                if (intPosition.x >= 0 && intPosition.y >= 0 && intPosition.x < width && intPosition.y < height) {
                    map[intPosition.x, intPosition.y] = 1;
                }
            }
        }


        
        return map;
    }

    #endregion
    #region world gen

    private static int[,,] InitWorldData() {
        int[,,] worldData = new int[width, height, depth];
        for (int z = 0; z < depth; z++) {
            for (int y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    worldData[x,y,z] = -1;
                }
            }
        }

        return worldData;
    }

    private static void GenerateBaseTerrain(int[,,] worldData) {
        
        int bottomLayerID = TileIndex.GetIDByName(genSettings.BottomLayer);
        for (int y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {

                // Get values from maps
                BiomeData biome = biomeData[biomeMap[x,y]];
                float inlandnessValue = inlandnessMap[x,y];
                float heightFlatnessValue = heightFlatnessMap[x,y];
                float heightElevationValue = heightElevationMap[x,y];
                float heightNoiseValue = heightNoiseMap[x,y];

                float ridgeValue = Mathf.Abs(3*Mathf.Abs(2 * heightElevationValue - 1)-2);

                float baseHeight = genSettings.BaseHeightCurve.Evaluate(inlandnessValue);
                //baseHeight *= baseHeight; Multiply the world height for "amplified" terrain
                baseHeight -= heightFlatnessValue * genSettings.FlatnessMultiplier;
                baseHeight += ridgeValue * genSettings.RidgesMultiplier;
                float noiseHeight = baseHeight + (heightNoiseValue - 0.5f) * genSettings.NoiseAmplitude;

                int topLayerHeight = Mathf.RoundToInt(Mathf.Clamp(noiseHeight, biome.MinHeight, biome.MaxHeight));
                
                for (int z = 0; z < depth; z++) {

                    // Layer 0
                    if (z == 0) {
                        worldData[x,y,z] = bottomLayerID;
                    }

                    // Layer 1 & 2
                    if (z == 1 || z == 2) {
                        worldData[x,y,z] = TileIndex.GetIDByName(biome.MiddleLayer);
                    }

                    // Layer 3
                    if (z == 3) {
                        if (z < topLayerHeight) {
                            worldData[x,y,z] = TileIndex.GetIDByName(biome.MiddleLayer);
                        } else if (z >= topLayerHeight) {
                            worldData[x,y,z] = TileIndex.GetIDByName(biome.TopLayer);
                        }
                    }

                    // Layer 4+
                    if (z >= 4) {
                        if (z < topLayerHeight) {
                            worldData[x,y,z] = TileIndex.GetIDByName(biome.MiddleLayer);
                        } else if (z == topLayerHeight) {
                            worldData[x,y,z] = TileIndex.GetIDByName(biome.TopLayer);
                        }
                    }
                    

                }
            }
        }
    }

    private static void PlaceFeatures(int[,,] worldData) {
        /* Placing features
        The next step is to place features. I'm not ENTIRELY sure how this will work but I want to generate 4 types of features right now:
        #1 Trees - Generate tree placements using Poisson Disc Sampling
        #2 Flowers - Either just use a feature perlin map or larger scale Poisson Disc sampling with fields of flowers
        #3 Ores - Overwrites tiles. Uses cellular automaton?
        #4 Ponds - Just uses perlin?

        The way im gonna structure this is BiomeData contains a list of FeatureData. FeatureData contains a Feature.
        Features inherit from BaseFeature and have some kind of GenerateHere(x,y) method
        They are scriptable object so you can select the tile. 
        Density will work for Poisson probably but the other stuff? Maybe need to just include that alongside the Feature.
        */

        // Init biome features
        
        for (int y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                BiomeData biome = biomeData[biomeMap[x,y]];
                for (int z = 0; z < depth; z++) {
                    foreach (BaseFeature feature in biome.Features) {
                        feature.PlaceFeature(worldData, new Vector3Int(x,y,z));
                    }
                }
            }
        }

        // Reset biome features for initialization.
        for (int b = 0; b < biomeData.Length; b++) {
            foreach (BaseFeature feature in biomeData[b].Features) {
                feature.Reset();
            }
        }
    }


    #endregion
}
