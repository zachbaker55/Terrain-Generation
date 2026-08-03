using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour {
    
    private int chunkSize;
    private int chunkX;
    private int chunkY;
    private Tilemap tilemap;
    public void InitializeAndLoad(int c, int cX, int cY) {
        chunkSize = c;
        chunkX = cX;
        chunkY = cY;
        tilemap = GetComponent<Tilemap>();
        LoadChunk();
    }

    private void LoadChunk() {
        tilemap.ClearAllTiles();

        int[,,] chunkData = WorldGenerator.Instance.GetChunk(chunkSize, chunkX, chunkY);

        int halfSize = Mathf.FloorToInt(chunkSize * 0.5f);
        Vector3Int size = new Vector3Int(chunkSize, chunkSize, WorldGenerator.Instance.WorldSize.z); // Width x Height x Depth
        Vector3Int position = new Vector3Int(-halfSize, -halfSize, -1); // Center at (0,0,0)
        BoundsInt bounds = new BoundsInt(position, size);
        GameTile[] tiles = new GameTile[size.x * size.y * size.z];

        for (int z = 0; z < size.z; z++) {
            for (int y = 0; y < size.y; y++) {
                for (int x = 0; x < size.x; x++) {
                    // Flatten 3D to 1D
                    int index = z + size.z * (x + size.x * y); // Z > X > Y
                    if (chunkData[x,y,z] == -1) {
                        tiles[index] = null;
                    } else {
                        tiles[index] = TileIndex.GetTileByID(chunkData[x,y,z]);   
                    }  
                }
            } 
        }

        tilemap.SetTilesBlock(bounds, tiles);
        tilemap.RefreshAllTiles();
    }

    private void OnDestroy() {
        //Do something?
    }
}
