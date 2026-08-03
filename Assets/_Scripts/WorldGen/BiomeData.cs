using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Biome", menuName = "ScriptableObjects/WorldGen/BiomeData")]
public class BiomeData : ScriptableObject {
    [Header("Biome")]
    [SerializeField] private string biomeName;
    [SerializeField] private bool generatesNormally;
    [SerializeField] private Color color;
    [Header("Biome Distance Point")]
    [SerializeField] private float _spawnStrength = 1.0f;
    public float SpawnStrength {
        get {
            if (_spawnStrength == 0) {
                return 0.0001f;
            } else {
                return _spawnStrength; 
            }
        }
    }

    [SerializeField] private float temperature;
    [SerializeField] private float precipitation;
    [SerializeField] private float morality;
    [SerializeField] private float inlandness;
    [SerializeField] private float flatness;
    [SerializeField] private float elevation; // Biomes can more easily be Range -1 > 1 

    [Header("Biome Height Settings")]

    [SerializeField] private int minHeight = 3;
    public int MinHeight {
        get {return minHeight; }
    }
    [SerializeField] private int maxHeight = 10;
    public int MaxHeight {
        get {return maxHeight; }
    }


    [Header("Tile Composition")]
    [SerializeField] private string _topLayer;

    public string TopLayer {
        get {return _topLayer; }
    }

    [SerializeField] private string _middleLayer;

    public string MiddleLayer {
        get {return _middleLayer; }
    }

    [Header("Features")]
    [SerializeField] float _treeDensity;
    public float TreeDensity {
        get {return _treeDensity; }
    }
    [SerializeField] List<BaseFeature> _features;
    public List<BaseFeature> Features {
        get {return _features; }
    }

    public string GetName() {
        return biomeName;
    }

    public bool IsNormalGeneration() {
        return generatesNormally;
    }

    public Color GetColor() {
        return color;
    }
    
    public Vector6 GetValues() {
        return new Vector6(temperature, precipitation, morality, inlandness, flatness, elevation);
    }

}
