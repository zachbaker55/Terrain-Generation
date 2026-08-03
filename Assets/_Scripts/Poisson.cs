using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This class is unused storage for a generic poisson disc map function
public static class Poisson {

    private static int width;
    private static int height;
    private static int maxPoissonAttempts;
    private static System.Random rng;
    
    private static int[,] GenerateGenericPoissonDiscMap(float radius) {
        // Prepare variables
        //float radius = 0.00000f
        float cellSize = radius / Mathf.Sqrt(2);
        int gridWidth = Mathf.CeilToInt(width / cellSize);
        int gridHeight = Mathf.CeilToInt(height / cellSize);

        Vector2[,] grid = new Vector2[gridWidth, gridHeight];
        List<Vector2> samples = new List<Vector2>();
        List<Vector2> activeList = new List<Vector2>();


        // Pick first sample randomly
        Vector2 first = new Vector2((float)rng.NextDouble() * width, (float)rng.NextDouble() * height);
        samples.Add(first);
        activeList.Add(first);

        while (activeList.Count > 0) {
            // Pick random sample from queue
            int i = rng.Next(activeList.Count);
            Vector2 parent = activeList[i];

            bool found = false;
            // Try maxAttempts # of times
            for (int c = 0; c < maxPoissonAttempts; c++) {
                // Make a new candidate point
                float angle = (float) rng.NextDouble() * 2 * Mathf.PI;
                float pointRadius = (float) rng.NextDouble() * (radius * 2 - radius) + radius;
                //float pointRadius = radius + epsilon; More evenly placed
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 candidate =  parent + direction * pointRadius;
                
                // Candidate is out of bounds!
                if (candidate.x < 0 || candidate.y < 0 || candidate.x >= width || candidate.y >= height) continue;
                
                //candidate's grid position
                int cx = Mathf.FloorToInt(candidate.x / cellSize);
                int cy = Mathf.FloorToInt(candidate.y / cellSize);

                //candidate search bounds. 5x5 limited by grid bounds
                int sx0 = Mathf.Max(cx - 2, 0);
                int sy0 = Mathf.Max(cy - 2, 0);
                int sx1 = Mathf.Min(cx + 2, gridWidth -1);
                int sy1 = Mathf.Min(cy + 2, gridHeight -1);

                bool valid = true;
                for (int y = sy0; y <= sy1; y++) {
                    for (int x = sx0; x <= sx1; x++) {
                        Vector2 neighbor = grid[x, y];
                        if (neighbor != Vector2.zero && Vector2.Distance(candidate, neighbor) < radius) {
                            valid = false;
                            break;
                        }
                    }
                    if (!valid) break;
                }

                if (valid) {
                    samples.Add(candidate);
                    activeList.Add(candidate);
                    grid[cx, cy] = candidate; //THIS LINE
                    found = true;
                    break;
                }
            }
            if (!found) activeList.RemoveAt(i);
        }

        int[,] map = new int[width, height];
        foreach (Vector2 sample in samples) {
            int xVal = Mathf.RoundToInt(sample.x);
            int yVal = Mathf.RoundToInt(sample.y);
            if (xVal < 0 || yVal < 0 || xVal >= width || yVal >= height) continue;
            map[xVal, yVal] = 1;
        }
        return map;
    }


}
