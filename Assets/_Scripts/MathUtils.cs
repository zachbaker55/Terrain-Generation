using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils {

    public static int MapFloatToRange(float value, int min, int max) {
        value = Mathf.Clamp01(value);
        return Mathf.RoundToInt(value * ((max - 1)- min) + min);
    }

    public static float MapIntToUnitFloat(int value, int a, int b) {
        if (a == b) {
            return 0.0f;
        }

        float clamped = Mathf.Clamp(value, Mathf.Min(a, b), Mathf.Max(a, b));
        return (clamped - a) / (b - a);
    }

    //This is specifically for tree density
    public static float DensityMapFloatToUnitFloat(float value, float a, float b) {
        if (a == b) {
            return 0.0f;
        }

        float clamped = Mathf.Clamp(value, Mathf.Min(a, b), Mathf.Max(a, b));
        return 1.0f - (clamped - a) / (b - a);
    }
}

[System.Serializable]
public struct Vector5 {
    public float x, y, z, w, v;

    public Vector5(float x, float y, float z, float w, float v) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
        this.v = v;
    }

    public static float Distance(Vector5 a, Vector5 b) {
        float dx = b.x - a.x;
        float dy = b.y - a.y;
        float dz = b.z - a.z;
        float dw = b.w - a.w;
        float dv = b.v - a.v;

        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz + dw * dw + dv * dv);
    }
}

[System.Serializable]
public struct Vector6 {
    public float x, y, z, w, v, u;

    public Vector6(float x, float y, float z, float w, float v, float u) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
        this.v = v;
        this.u = u;
    }

    public static float Distance(Vector6 a, Vector6 b) {
        float dx = b.x - a.x;
        float dy = b.y - a.y;
        float dz = b.z - a.z;
        float dw = b.w - a.w;
        float dv = b.v - a.v;
        float du = b.u - a.u;

        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz + dw * dw + dv * dv + du * du);
    }
}

[System.Serializable]
public struct IndexWeight {
    public int index;
    public float weight;
    public IndexWeight(int index, float weight) {
        this.index = index;
        this.weight = weight;
    }

    public override readonly string ToString() {
        return string.Format("[{0}, {1}]", index, weight);
    }
}