using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

[CustomEditor(typeof(TileIndex))]
public class TileIndexEditor : Editor {
    private TileIndex script;

    private void OnEnable() {
        script = (TileIndex) target;
        script.QuickInitiate();
    }
    public override void OnInspectorGUI() {
        DrawDefaultInspector();


        if (GUILayout.Button("Quick Initiate")) {
            script.QuickInitiate();
        }
    }


}