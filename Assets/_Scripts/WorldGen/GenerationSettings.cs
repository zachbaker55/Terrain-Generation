using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Generation Settings", menuName = "ScriptableObjects/WorldGen/Generation Settings")]
public class GenerationSettings : ScriptableObject {

    [SerializeField] private AnimationCurve _baseHeightCurve;

    public AnimationCurve BaseHeightCurve {
        get { return _baseHeightCurve; }
    }

    [SerializeField] private float _flatnessMultiplier;

    public float FlatnessMultiplier {
        get { return _flatnessMultiplier; }
    }


    [SerializeField] private float _ridgesMultiplier;

    public float RidgesMultiplier {
        get { return _ridgesMultiplier; }
    }

    [SerializeField] private float _noiseAmplitude;

    public float NoiseAmplitude {
        get { return _noiseAmplitude; }
    }

    [SerializeField] private string _bottomLayer;

    public string BottomLayer {
        get { return _bottomLayer; }
    }

}
