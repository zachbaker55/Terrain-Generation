using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : Singleton<WorldGenerator> {
    [Header("World Settings")]
    [Min(1)] [SerializeField] private int worldWidth;
    [Min(1)] [SerializeField] private int worldHeight;
    [Min(1)] [SerializeField] private int worldDepth;
    public Vector3Int WorldSize {
        get {return new Vector3Int(worldWidth, worldHeight, worldDepth);}
    }
    [SerializeField] private int seed;

    [Header("Island Settings")]
    [Min(1)][SerializeField] private int numberOfIslands = 1;
    [Range(0f,0.9f)][SerializeField] private float islandRegionVariance = 0.9f; //range 0f-0.9f
    [Min(1)][SerializeField] private float minimumDistanceBetweenIslands = 20f;
    [Min(10)][SerializeField] private int maxIslandAttempts = 100;
    [Min(1f)][SerializeField] private float islandCenterBiasStrength = 3f;
    [Min(1f)][SerializeField] private float islandFalloff = 5f;
    [SerializeField] private float voronoiFalloff = 4f;
    [Range(0f,0.3f)] [SerializeField] private float islandXMargin = 0.15f;
    [Range(0f,0.3f)] [SerializeField] private float islandYMargin = 0.15f;
    [Range(0f,1f)] [SerializeField] private float islandWaterCutoff = 0.5f;

    [Header("Feature Settings")]
    [SerializeField] private float treeBlendFalloffStrength = 6f;
    [SerializeField] private float minPoissonRadius = 2f;
    [SerializeField] private float maxPoissonRadius = 50f;
    [SerializeField] private int maxPoissonAttempts = 3;

    [Header("Biomes")]
    [SerializeField] private string fillBiomeName;
    [SerializeField] private BiomeData[] biomeData;

    [Header("Noise Maps")]
    public NoiseSetting islandNoise;
    public NoiseSetting biomeTemperature;
    public NoiseSetting biomePrecipitation;
    public NoiseSetting biomeMorality;
    public NoiseSetting heightFlatness;
    public NoiseSetting heightElevation;
    public NoiseSetting heightNoise;

    [Header("Generation Settings")]
    [SerializeField] private GenerationSettings worldGenSettings;
    public bool autoUpdate;
    [SerializeField] private WorldGeneration.DrawMode drawMode;
    
    public static event Action<Texture2D> WorldTextureUpdate;
    [HideInInspector] public static bool isWorldGenerated = false;

    // other

    private WorldCameraSnap worldCameraSnap;


    //World storage
    int[,,] worldData;

    protected override void Awake() {
        Debug.Log("Initiating World Generator");
        base.Awake();
    }

    
    [ContextMenu("Update Map")]
    public void UpdateMapButton() {
        GenerateMapTexture();
    }
    [ContextMenu("Generate World")]
    public void GenerateWorldButton() {
        Awake();
        GenerateWorld();
    }

    [ContextMenu("Snap World Photo")]
    public void SnapPhotoButton() {
        worldCameraSnap = GetComponentInChildren<WorldCameraSnap>();
        SnapPhoto();
    }
    

    public void GenerateMapTexture() {
        WorldGeneration.SetGenerationSettings(worldWidth, worldHeight, worldDepth, seed, 
            numberOfIslands, islandRegionVariance, minimumDistanceBetweenIslands,maxIslandAttempts,islandCenterBiasStrength,
            islandFalloff, voronoiFalloff, islandXMargin, islandYMargin, islandWaterCutoff, 
            treeBlendFalloffStrength, minPoissonRadius, maxPoissonRadius, maxPoissonAttempts);
        WorldGeneration.SetBiomes(biomeData, fillBiomeName);
        WorldGeneration.GenerateMapData(islandNoise, biomeTemperature, biomePrecipitation, biomeMorality,
        heightFlatness, heightElevation, heightNoise);
        Texture2D texture = WorldGeneration.GenerateMapTexture(drawMode);
        WorldTextureUpdate?.Invoke(texture);
    }

    public void GenerateWorld() {
        WorldGeneration.SetGenerationSettings(worldWidth, worldHeight, worldDepth, seed, 
            numberOfIslands, islandRegionVariance, minimumDistanceBetweenIslands,maxIslandAttempts,islandCenterBiasStrength,
            islandFalloff, voronoiFalloff, islandXMargin, islandYMargin, islandWaterCutoff, 
            treeBlendFalloffStrength, minPoissonRadius, maxPoissonRadius, maxPoissonAttempts);
        WorldGeneration.SetBiomes(biomeData, fillBiomeName);
        WorldGeneration.GenerateMapData(islandNoise, biomeTemperature, biomePrecipitation, biomeMorality,
        heightFlatness, heightElevation, heightNoise);
        worldData = WorldGeneration.GenerateWorld(worldGenSettings);
        isWorldGenerated = true;
    }

    private void SnapPhoto() {
        worldCameraSnap.SnapCamera();
    }

    public int[,,] GetChunk(int c, int cX, int cY)  {
        if (worldData == null) {
            Debug.Log("World not yet generated.");
            return null;
        }

        int[,,] chunkData = new int[c,c,worldDepth];

        int halfWidth = Mathf.FloorToInt(worldWidth * 0.5f);
        int halfHeight = Mathf.FloorToInt(worldHeight * 0.5f);

        int chunkOriginX = (cX * c) - c / 2;
        int chunkOriginY = (cY * c) - c / 2;

        // Transfer worldData to chunkData
        for (int localY = 0; localY < c; localY++) {

            int worldY = chunkOriginY + localY;
            int dataY = worldY + halfHeight;

            for (int localX = 0; localX < c; localX++) {
                int worldX = chunkOriginX + localX;

                int dataX = worldX + halfWidth;

                //Bounds check
                if (dataX < 0 || dataX >= worldWidth || dataY < 0 || dataY >= worldHeight) {
                    for (int z = 0; z < worldDepth; z++) {
                        chunkData[localX, localY, z] = -1;
                    }
                    continue;
                }

                for (int z = 0; z < worldDepth; z++) {
                    chunkData[localX, localY, z] = worldData[dataX, dataY, z];
                }
            }
        }


        //
        return chunkData; 
    }


}
