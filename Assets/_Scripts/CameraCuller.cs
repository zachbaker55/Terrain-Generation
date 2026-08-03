using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CameraCuller : MonoBehaviour {
    [SerializeField] [Range(0,3)] private int chunkLoadRange; 
    [SerializeField] private bool automaticCull = false;
    private ChunkManager cmInstance;
    private Vector2Int centerChunk;
    [SerializeField] private List<Vector2Int> loadedChunks;
    private int previousLoadRange;


    private void OnEnable() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += Init;
        #else
            Init();
        #endif
    }

    public void Init() {
        cmInstance = ChunkManager.Instance;
        ChunkManager.ChunkUnloadEvent += OnChunkUnload;
        if (cmInstance == null) { 
            Debug.Log("Camera Culler initializing before Chunk Manager. Please fix.");
        }
        previousLoadRange = -1;
    }

    private void OnDisable() {
        ChunkManager.ChunkUnloadEvent -= OnChunkUnload;
    }

    private void Update() {
        if (!automaticCull) return;
        if (!WorldGenerator.isWorldGenerated) return;
        if (transform.hasChanged) {
            CullAtCurrentChunk();
            transform.hasChanged = false;
        }
    }

    [ContextMenu("Cull")]
    public void CullAtCurrentChunk() {
        if (cmInstance == null) {
            Debug.Log("OnEnable wasn't called in CameraCuller");
            return;
        }
        Vector2Int currentChunk = cmInstance.WorldToChunkPosition(transform.position.x, transform.position.y);
        if (currentChunk == centerChunk && previousLoadRange == chunkLoadRange) return;
        centerChunk = currentChunk;
        List<Vector2Int> chunksRange = GetChunksInRange(chunkLoadRange);
        List<Vector2Int> newLoadedChunks = new List<Vector2Int>();
        foreach (Vector2Int chunk in chunksRange) {
            Vector2Int chunkToLoad = chunk + centerChunk;
            if (!cmInstance.IsChunkLoaded(chunkToLoad)) {
                cmInstance.LoadChunk(chunkToLoad);
            }
            newLoadedChunks.Add(chunkToLoad);
        }

        List<Vector2Int> chunksToUnload = new List<Vector2Int>(loadedChunks);
        foreach (Vector2Int chunk in chunksToUnload) {
            if (!newLoadedChunks.Contains(chunk)) {
                cmInstance.UnloadChunk(chunk);
            }
        }
        loadedChunks = newLoadedChunks;
        previousLoadRange = chunkLoadRange;
    }

    private void OnChunkUnload(Vector2Int chunk) {
        loadedChunks.Remove(chunk);
    }

    // Returns chunks in a spiral range
    private List<Vector2Int> GetChunksInRange(int radius) {
        List<Vector2Int> positions = new List<Vector2Int>();
        positions.Add(new Vector2Int(0, 0)); // Center

        for (int r = 1; r <= radius; r++) {
            int x = -r;
            int y = -r;

            // Move right along bottom edge
            for (; x < r; x++) {
                positions.Add(new Vector2Int(x, y));
            }

            // Move up along right edge
            for (; y < r; y++) {
                positions.Add(new Vector2Int(x, y));
            }
            // Move left along top edge
            for (; x > -r; x--) {
                positions.Add(new Vector2Int(x, y));
            }
            // Move down along left edge
            for (; y > -r; y--) {
                positions.Add(new Vector2Int(x, y));
            }
        }
        return positions;
    }

}
