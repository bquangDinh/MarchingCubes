using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public enum NoiseType
{
    FUNKY,
    PERLIN,
    PLANE,
    MIXED
}

public class Noise
{
    public float scale = 1.0f;
    public float frequency = 1.0f;
    public float amplitude = 1.0f;
    public int octaves = 1;
    public NoiseType noiseType;
    public FastNoise fastNoise;

    public Noise()
    {
        fastNoise = new FastNoise();
    }

    public bool equals(Noise n)
    {
        return scale == n.scale && frequency == n.frequency && amplitude == n.amplitude && octaves == n.octaves && noiseType == n.noiseType;
    }

    public void Clone(Noise n)
    {
        scale = n.scale;
        frequency = n.frequency;
        amplitude = n.amplitude;
        octaves = n.octaves;
        noiseType = n.noiseType;
    }

    public void SetNoise(float scale, float frequency, float amplitude, int octaves, NoiseType noiseType)
    {
        this.scale = scale;
        this.frequency = frequency;
        this.amplitude = amplitude;
        this.octaves = octaves;
        this.noiseType = noiseType;
    }

    private float plane(Vector3 p)
    {
        return -p.y;
    }

    private float perlin2D(Vector3 p)
    {
        return MathUtils.OctavePerlin2D(p.x, p.z, octaves, 1.0f, frequency, amplitude) * 2;
    }

    public float calcNoise2D(Vector3 p)
    {
        float totalNoise = 0.0f;

        if(noiseType == NoiseType.PLANE)
        {
            totalNoise = plane(p);
        }

        if(noiseType == NoiseType.PERLIN)
        {
            totalNoise = perlin2D(p);
        }

        if(noiseType == NoiseType.MIXED)
        {
            totalNoise += plane(p) + perlin2D(p) + Mathf.Sin(p.y);
        }
        

        return totalNoise;
    }

    public float calcNoise3D(Vector3 p)
    {
        float totalNoise = 0.0f;

        totalNoise = plane(p) + MathUtils.OctavePerlin3D(p, octaves, 1.0f, frequency, amplitude, fastNoise);

        return totalNoise;
    }
}
