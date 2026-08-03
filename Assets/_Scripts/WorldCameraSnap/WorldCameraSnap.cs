using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class WorldCameraSnap : MonoBehaviour {

    [SerializeField] private int tilemapSize;
    [SerializeField] private int fullWidth = 8192;  // Total width of the rendered image
    private int fullHeight; // Total height of the rendered image
    [SerializeField] private int chunkSize = 512;  // Size of each chunk (smaller renders)
    private CameraCuller cameraCuller;

    [ContextMenu("Snap")]
    public void SnapCamera() {
        if (cameraCuller == null) {
            cameraCuller = GetComponent<CameraCuller>();
        }
        fullHeight = fullWidth/2;
        int chunksX = fullWidth / chunkSize;  // Number of chunks horizontally
        int chunksY = fullHeight / chunkSize; // Number of chunks vertically

        // Create a final Texture2D to hold the stitched result
        Texture2D finalTexture = new Texture2D(fullWidth, fullHeight, TextureFormat.RGBA32, false);

        RenderTexture rt = new RenderTexture(chunkSize, chunkSize, 24);  // Chunked render texture
        Camera renderCam = GetComponent<Camera>();
        renderCam.orthographic = true;
        renderCam.clearFlags = CameraClearFlags.SolidColor;
        renderCam.orthographicSize = (tilemapSize / 4) / chunksX;
        renderCam.backgroundColor = Color.clear;

        int chunkCount = 0;

        float unitsPerPixel = tilemapSize / (float)fullWidth;
        float chunkWorldSize = chunkSize * unitsPerPixel;

        float worldLeft = -tilemapSize / 2f;
        float worldBottom = -tilemapSize / 4f;

        renderCam.orthographicSize = chunkWorldSize / 2f;

        int totalShots = chunksX * chunksY;
        for (int x = 0; x < chunksX; x++) {
            for (int y = 0; y < chunksY; y++) {
                float worldX = worldLeft + chunkWorldSize * (x + 0.5f);
                float worldY = worldBottom + chunkWorldSize * (y + 0.5f);
                renderCam.transform.position = new Vector3(worldX, worldY, -10);

                // load chunks in range
                cameraCuller.CullAtCurrentChunk();

                // Set the render target to the RenderTexture
                renderCam.targetTexture = rt;

                // Render the scene into the RenderTexture
                renderCam.Render();

                // Copy the RenderTexture into a Texture2D
                RenderTexture.active = rt;
                Texture2D chunkTexture = new Texture2D(chunkSize, chunkSize, TextureFormat.RGBA32, false);
                chunkTexture.ReadPixels(new Rect(0, 0, chunkSize, chunkSize), 0, 0);
                chunkTexture.Apply();

                // Copy the chunk into the final texture at the right position
                finalTexture.SetPixels(x * chunkSize, y * chunkSize, chunkSize, chunkSize, chunkTexture.GetPixels());

                // Cleanup
                RenderTexture.active = null;
                chunkCount++;
                
                if (!Application.isPlaying) {
                    int i = (x * chunksY) + y;
                    if (i % 10 == 0) {
                        float progress = i / (float)totalShots;
                        EditorUtility.DisplayProgressBar("Rendering Map", $"Rendering part {i}/{totalShots}", progress);
                    }
                }

            }
        }
        if (!Application.isPlaying) {
            EditorUtility.ClearProgressBar();
        }

        // Apply final texture changes
        finalTexture.Apply();

        // Encode final texture to PNG and save it
        byte[] finalBytes = finalTexture.EncodeToPNG();
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = Path.Combine(Application.dataPath, $"Screenshot/World_{timestamp}.png");
        File.WriteAllBytes(filename, finalBytes);

        Debug.Log($"Saved stitched screenshot to: {filename}");
    }
}
