using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class Minimap : MonoBehaviour {    

    [SerializeField] private WorldGenerator worldGenerator;
    [SerializeField] float scale = 2;
    RawImage rawImage;
    Texture2D minimapTexture;

    
    private void OnEnable() {
        rawImage = GetComponent<RawImage>();
        WorldGenerator.WorldTextureUpdate += UpdateMinimapTexture;
        if (minimapTexture == null) {
            GenerateMinimapTexture();
        }
    }

    private void OnDisable() {
        WorldGenerator.WorldTextureUpdate -= UpdateMinimapTexture;
    }

    private void OnValidate() {
        SetMinimapImage();    
    }

    private void GenerateMinimapTexture() {
        worldGenerator.GenerateMapTexture();
    }

    private void UpdateMinimapTexture(Texture2D texture) {
        minimapTexture = texture;


        SetMinimapImage();
    }

    private void SetMinimapImage() {
    
        if (rawImage != null) {
            rawImage.texture = minimapTexture;
            rawImage.SetNativeSize();
            rawImage.rectTransform.sizeDelta = new Vector2(minimapTexture.width, minimapTexture.height) * scale;
        }
    }
    

}
