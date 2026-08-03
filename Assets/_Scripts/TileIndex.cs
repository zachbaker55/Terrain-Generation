using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileIndex : PersistentSingleton<TileIndex> {

    // Tile data. Contains info of all tiles in the game
    private Dictionary<string, GameTile> nameToTile = new();
    private Dictionary<int, GameTile> idToTile = new();
    private Dictionary<GameTile, int> tileToID = new();
    private Dictionary<GameTile, string> tileToName = new();

    protected override void Awake() {
        Debug.Log("Initiating Tile Index");
        base.Awake();
        InitiateAllTiles();
    }

    [ContextMenu("Quick Initiate")]
    public void QuickInitiate() {
        Awake();
    }

    public void InitiateAllTiles() {
        nameToTile = new();
        idToTile = new();
        tileToID = new();
        tileToName = new();


        GameTile[] allTiles = Resources.LoadAll<GameTile>("TileData");

        for (int i = 0; i < allTiles.Length; i++) {
            GameTile tile = allTiles[i];
            nameToTile[tile.name] = tile;
            idToTile[i] = tile;
            tileToID[tile] = i;
            tileToName[tile] = tile.name;
            //Debug.Log("Loaded tile: #" + i + ": " + tile.name);
        }
    }

    // Name > Tile
    private GameTile PGetTileByName(string name) {
        return nameToTile.TryGetValue(name, out var tile) ? tile : null;
    }
    public static GameTile GetTileByName(string name) {
        return Instance.PGetTileByName(name);
    }

    // ID > Tile
    private GameTile PGetTileByID(int id) {
        return idToTile.TryGetValue(id, out var tile) ? tile : null;
    }
    public static GameTile GetTileByID(int id) {
        return Instance.PGetTileByID(id);
    }

    // Tile > ID
    private int PGetIDByTile(GameTile tile) {
        return tileToID.TryGetValue(tile, out var id) ? id : -1;
    }
    public static int GetIDByTile(GameTile tile) {
        return Instance.PGetIDByTile(tile);
    }

    // Tile > Name
    
    private string PGetNameByTile(GameTile tile) {
        return tileToName.TryGetValue(tile, out var name) ? name : null;
    }
    public static string GetNameByTile(GameTile tile) {
        return Instance.PGetNameByTile(tile);
    }

    // Name > ID
    public static int GetIDByName(string name) {
        GameTile tile = Instance.PGetTileByName(name);
        return Instance.PGetIDByTile(tile);
    }

    // ID > Name
    public static string GetNameByID(int id) {
        GameTile tile = Instance.PGetTileByID(id);
        return Instance.PGetNameByTile(tile);
    }

}


[System.Serializable]
public struct TileWeight {
    [SerializeField] private string _tileName;
    public string TileName {
        get { return _tileName; }
    }
    [SerializeField] private float _weight;
    public float Weight {
        get { return _weight; }
    }
    public TileWeight(string tileName, float weight) {
        this._tileName = tileName;
        this._weight = weight;
    }

    public override readonly string ToString() {
        return string.Format("[{0}, {1}]", _tileName, _weight);
    }

    public static float[] NormalizeWeights(List<TileWeight> tileWeights) {
        float[] normalizedWeights = new float[tileWeights.Count];
        float weightSum = 0f;
        foreach (TileWeight tWeight in tileWeights) {
            weightSum += tWeight.Weight;
        }

        if (weightSum == 0f) return normalizedWeights; // Case where there are no weights inserted. Return array of 0s
        if (weightSum < 1) { // Case where floats are "properly" inputted. Any gaps are interpretted as emptiness. 
            // Return original weights as array.
            for (int i = 0; i < tileWeights.Count; i++) {
                normalizedWeights[i] = tileWeights[i].Weight;
            }
            return normalizedWeights;
        }

        // C
        for (int i = 0; i < tileWeights.Count; i++) {
            normalizedWeights[i] = tileWeights[i].Weight / weightSum;
        }

        return normalizedWeights;
    }

    // Returns an index or -1 based of the weights
    public static int RandomWeightPick(int[] indexes, float[] weights, float randomVal) {
        if (indexes.Length != weights.Length) return -1; // Error with inputs
        
        float currentWeightSum = 0f;
        for (int i = 0; i < weights.Length; i++) {
            currentWeightSum += weights[i];
            if (currentWeightSum >= randomVal) return i;
        }

        return -1;
    }
}
