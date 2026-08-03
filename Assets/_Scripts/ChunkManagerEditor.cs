using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

[CustomEditor(typeof(ChunkManager))]
public class ChunkManagerEditor : Editor {
    private ChunkManager script;

    private void OnEnable() {
        script = (ChunkManager) target;
    }
    public override void OnInspectorGUI() {
        DrawDefaultInspector();


        if (GUILayout.Button("Quick Load Chunk")) {
            script.QuickLoadChunk();
        }
        if (GUILayout.Button("Quick Unload Chunk")) {
            script.QuickUnloadChunk();
        }
        if (GUILayout.Button("Quick Unload All Chunks")) {
            script.QuickUnloadAllChunks();
        }
    }


}