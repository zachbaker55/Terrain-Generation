using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Feature", menuName = "ScriptableObjects/WorldGen/Features/Blob")]
public class BlobFeature : BaseFeature {
    protected override void Init() {
        throw new System.NotImplementedException();
    }

    protected override void Place(int[,,] worldData, Vector3Int point) {
        // use GenerateGridSamplingMap() for points
        // Triangle distribution chart for height bias
        throw new System.NotImplementedException();
    }
}
