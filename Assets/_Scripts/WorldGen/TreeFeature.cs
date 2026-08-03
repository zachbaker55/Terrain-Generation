using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Feature", menuName = "ScriptableObjects/WorldGen/Features/Tree")]
public class TreeFeature : BaseFeature {
    [SerializeField] private List<TileWeight> trees;
    [SerializeField] private List<string> canGenerateOn;
    [SerializeField] [Min(1)] private int _minSpawnHeight = 3;
    [SerializeField] private int _maxSpawnHeight = 20;
    
    private int[,] treeLocationMap = null;
    private int[] treeTileID;
    private float[] treeWeights;
    
    
    

    protected override void Init() {
        treeLocationMap = WorldGeneration.TreeLocationMap;

        treeTileID = new int[trees.Count];
        for (int i = 0; i < trees.Count; i++) {
            treeTileID[i] = TileIndex.GetIDByName(trees[i].TileName);
        }
        treeWeights = TileWeight.NormalizeWeights(trees);
        
    }

    protected override void Place(int[,,] worldData, Vector3Int point) {
        if (point.z > _maxSpawnHeight || point.z < _minSpawnHeight) return; // Do not generate if not in spawn height range

        if (treeLocationMap[point.x, point.y] != 1) return; // Do not generate if not in tree location map
        if (point.z <= 0) return; // Do not generate if point.z <= 0; Extra check so no out of index errors occur when checking below
        if (worldData[point.x, point.y, point.z] != -1) return; // Only generate if tile is empty

        int tileBelowId = worldData[point.x, point.y, point.z - 1];
        if (tileBelowId == -1) return; // Do not generate if tile below is air

        // Randomly pick which tree to generate
        float randomVal = (float) rng.NextDouble();
        int randomTreeID = TileWeight.RandomWeightPick(treeTileID, treeWeights, randomVal);
        if (randomTreeID == -1) return; // Do not generate if weight target wasn't met

        string tileBelow = TileIndex.GetNameByID(tileBelowId);
        if (canGenerateOn.Contains(tileBelow)) { // Only generate if tile below is in canGenerateOn
            worldData[point.x, point.y, point.z] = treeTileID[randomTreeID];
        }
    }
}
