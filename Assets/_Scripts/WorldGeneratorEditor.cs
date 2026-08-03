using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

[CustomEditor(typeof(WorldGenerator))]
public class WorldGeneratorEditor : Editor {
    private WorldGenerator script;

    private NoiseSetting lastIslandNoise;
    private NoiseSetting lastBiomeTemperature;
    private NoiseSetting lastBiomePrecipitation;
    private NoiseSetting lastBiomeMorality;
    private NoiseSetting lastHeightFlatness;
    private NoiseSetting lastHeightElevation;
    private NoiseSetting lastHeightNoise;

    private void OnEnable() {
        script = (WorldGenerator) target;
    }
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (script == null) {Debug.Log("OnInspectorGUI is called before OnEnable() for some reason.");}
        

        //NoiseSetting inspect event
        //islandNoise
        if (script.islandNoise != lastIslandNoise) {
            if (lastIslandNoise != null)
                lastIslandNoise.OnSettingsChanged -= UpdateWorldButton;

            if (script.islandNoise != null)
                script.islandNoise.OnSettingsChanged += UpdateWorldButton;

            lastIslandNoise = script.islandNoise;
        }
        //biomeTemperature
        if (script.biomeTemperature != lastBiomeTemperature) {
            if (lastBiomeTemperature != null)
                lastBiomeTemperature.OnSettingsChanged -= UpdateWorldButton;

            if (script.biomeTemperature != null)
                script.biomeTemperature.OnSettingsChanged += UpdateWorldButton;

            lastBiomeTemperature = script.biomeTemperature;
        }
        //biomePrecipitation
        if (script.biomePrecipitation != lastBiomePrecipitation) {
            if (lastBiomePrecipitation != null)
                lastBiomePrecipitation.OnSettingsChanged -= UpdateWorldButton;

            if (script.biomePrecipitation != null)
                script.biomePrecipitation.OnSettingsChanged += UpdateWorldButton;

            lastBiomePrecipitation = script.biomePrecipitation;
        }
        //biomeMorality
        if (script.biomeMorality != lastBiomeMorality) {
            if (lastBiomeMorality != null)
                lastBiomeMorality.OnSettingsChanged -= UpdateWorldButton;

            if (script.biomeMorality != null)
                script.biomeMorality.OnSettingsChanged += UpdateWorldButton;

            lastBiomeMorality = script.biomeMorality;
        }
        //heightFlatness
        if (script.heightFlatness != lastHeightFlatness) {
            if (lastHeightFlatness != null)
                lastHeightFlatness.OnSettingsChanged -= UpdateWorldButton;

            if (script.heightFlatness != null)
                script.heightFlatness.OnSettingsChanged += UpdateWorldButton;

            lastHeightFlatness = script.heightFlatness;
        }
        //heightBumps
        if (script.heightElevation != lastHeightElevation) {
            if (lastHeightElevation != null)
                lastHeightElevation.OnSettingsChanged -= UpdateWorldButton;

            if (script.heightElevation != null)
                script.heightElevation.OnSettingsChanged += UpdateWorldButton;

            lastHeightElevation = script.heightElevation;
        }
        //heightNoise
        if (script.heightNoise != lastHeightNoise) {
            if (lastHeightNoise != null)
                lastHeightNoise.OnSettingsChanged -= UpdateWorldButton;

            if (script.heightNoise != null)
                script.heightNoise.OnSettingsChanged += UpdateWorldButton;

            lastHeightNoise = script.heightNoise;
        }

        //Updating properly
        UpdateWorldButton();

        if (GUILayout.Button("Update Map")) {
            script.UpdateMapButton();
        }

        if (GUILayout.Button("Generate World")) {
            script.GenerateWorldButton();
        }
        if (GUILayout.Button("Snap World Photo")) {
            script.SnapPhotoButton();
        }
    }

    private void UpdateWorldButton() {
        if (script.autoUpdate) {
            script.UpdateMapButton();
        }
    }
}