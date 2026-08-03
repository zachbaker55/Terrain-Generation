using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseFeature : ScriptableObject {
    [SerializeField] private AnimationCurve featureHeightCurve = AnimationCurve.Linear(0f,1f,100f,1f);
    protected System.Random rng;
    private bool isInitialized = false;
    protected abstract void Init();
    protected abstract void Place(int[,,] worldData, Vector3Int point);

    public void PlaceFeature(int [,,] worldData, Vector3Int point) {
        if (!isInitialized) {
            rng = WorldGeneration.RandomNumberGenerator;
            Init();
            isInitialized = true;
        }

        float randomVal = (float) rng.NextDouble();
        float heightChance = featureHeightCurve.Evaluate(point.z);
        if (heightChance >= randomVal) {
            Place(worldData, point);   
        }
    } 

    public void Reset() {
        isInitialized = false;
    }
}