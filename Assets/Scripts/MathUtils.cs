using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtils
{
    public static int getIndexFromXYZ(int x, int y, int z, Dimensions resolution)
    {
        return z + resolution.z * (y + resolution.y * x);
    }

    public static float OctavePerlin2D(float x, float y, int octaves, float persistence, float frequency, float amplitude)
    {
        float total = 0;
        float maxValue = 1;
        for(int i = 0; i < octaves; ++i)
        {
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }

    public static float OctavePerlin3D(Vector3 p, int octaves, float persistence, float frequency, float amplitude, FastNoise fastNoise)
    {
        float total = 0.0f;
        float maxValue = 1.0f;

        for(int i = 0; i < octaves; ++i)
        {
            total += fastNoise.GetPerlin(p.x * frequency, p.y * frequency, p.z * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= persistence;

            frequency *= 2;
        }

        return total / maxValue;
    }
}
