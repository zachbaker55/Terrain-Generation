using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Noise", menuName = "ScriptableObjects/WorldGen/NoiseSetting")]
public class NoiseSetting : ScriptableObject {
    public event Action OnSettingsChanged;
    [Min(0.0001f)] public float scale;

    [Min(1)] public int octaves;
    [Range(0f, 1f)] public float persistance;
    [Min(1)] public float lacunarity;
    public Vector2 offset;


    private void OnValidate() {
        OnSettingsChanged?.Invoke();
    }
}
