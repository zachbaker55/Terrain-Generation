using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class ChunkManager : Singleton<ChunkManager> {

    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private int chunkSize = 32;
    [SerializeField] private Vector2Int inputChunk = Vector2Int.zero; // Entirely for editor purposes
    private Dictionary<Vector2Int, GameObject> loadedChunks = new();

    public static event Action<Vector2Int> ChunkLoadEvent;
    public static event Action<Vector2Int> ChunkUnloadEvent;
    
    protected override void Awake() {
        Debug.Log("Initiating Chunk Manager!");
        base.Awake();
    }

    public void OnEnable() {
        if (!Application.isPlaying) {
            Awake();
        }
    }

    [ContextMenu("Quick Load Chunk")]
    public void QuickLoadChunk() {
        LoadChunk(inputChunk.x, inputChunk.y);
    }

    [ContextMenu("Quick Unload Chunk")]
    public void QuickUnloadChunk() {
        UnloadChunk(inputChunk.x, inputChunk.y);
    }
    [ContextMenu("Quick Unload All Chunks")]
    public void QuickUnloadAllChunks() {
        UnloadAllChunks();
    }

    // Chunks

    public void LoadChunk(Vector2Int chunk) {
        LoadChunk(chunk.x, chunk.y);
    }
    public void LoadChunk(int chunkX, int chunkY) {
        Vector2Int key = new(chunkX, chunkY);
        if (loadedChunks.ContainsKey(key)) return; // Chunk already loaded
        
        ChunkLoadEvent?.Invoke(new Vector2Int(chunkX, chunkY));

        GameObject newChunk = Instantiate(chunkPrefab);
        newChunk.transform.SetParent(transform);
        newChunk.transform.position = ChunkToWorldPosition(chunkX, chunkY);
        newChunk.name = $"Chunk [{chunkX},{chunkY}]";
        
        Chunk newChunkScript = newChunk.GetComponent<Chunk>();
        newChunkScript.InitializeAndLoad(chunkSize, chunkX, chunkY);
        loadedChunks[key] = newChunk;
    }

    public void UnloadChunk(Vector2Int chunk) {
        UnloadChunk(chunk.x, chunk.y);
    }

    public void UnloadChunk(int chunkX, int chunkY) {
        Vector2Int key = new(chunkX, chunkY);
        if (!loadedChunks.TryGetValue(key, out var chunk)) return;
        
        ChunkUnloadEvent?.Invoke(new Vector2Int(chunkX, chunkY));
        
        loadedChunks.Remove(key);
        if (Application.isPlaying) {
            Destroy(chunk);
        } else {
            DestroyImmediate(chunk);
        }
        
        
    }

    public void UnloadAllChunks() {
        List<GameObject> chunksToUnload = new List<GameObject>(loadedChunks.Values);

        loadedChunks.Clear();
        foreach (GameObject chunk in chunksToUnload) {
            if (Application.isPlaying) {
                Destroy(chunk);
            } else {
                DestroyImmediate(chunk);
            }
        }
        
    }
    
    public bool IsChunkLoaded(int chunkX, int chunkY) {
        return IsChunkLoaded(new Vector2Int(chunkX, chunkY));
    }

    public bool IsChunkLoaded(Vector2Int chunk) {
        if (loadedChunks.ContainsKey(chunk)) {
            return true;
        } else return false;
    }

    public Vector3 ChunkToWorldPosition(int chunkX, int chunkY) {
        float offsetX = (chunkX - chunkY) * (chunkSize / 2f);
        float offsetY = (chunkX + chunkY) * (chunkSize / 4f);
        return new Vector3(offsetX, offsetY, 0);
    }

    public Vector2Int WorldToChunkPosition(float worldX, float worldY) {
        float a = worldX / (chunkSize / 2f);
        float b = worldY / (chunkSize / 4f);

        int chunkX = Mathf.RoundToInt((a + b) / 2f);
        int chunkY = Mathf.RoundToInt((b - a) / 2f);

        return new Vector2Int(chunkX, chunkY);
    }


}

// Top right: +x
// Bottom left: -x
// Top left: +y
// Bottom right: -y
// Up 1: +z
// Down 1: -z
// SetTileBlock places size at position starting at the bottom. Given position is seen as (0) instead of centering