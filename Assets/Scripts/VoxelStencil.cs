using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelStencil
{
    protected bool fillType;

    public int centerX, centerY, centerZ;

    protected int radius;

    public int XStart
    {
        get
        {
            return centerX - radius;
        }
    }

    public int XEnd
    {
        get
        {
            return centerX + radius;
        }
    }

    public int YStart
    {
        get
        {
            return centerY - radius;
        }
    }

    public int YEnd
    {
        get
        {
            return centerY + radius;
        }
    }

    public int ZStart
    {
        get
        {
            return centerZ - radius;
        }
    }

    public int ZEnd
    {
        get
        {
            return centerZ + radius;
        }
    }

    public virtual void Initialize(bool fillType, int radius)
    {
        this.fillType = fillType;
        this.radius = radius;
    }

    public void SetCenter(int x, int y, int z)
    {
        centerX = x;
        centerY = y;
        centerZ = z;
    }

    public virtual bool Apply(int x, int y, int z, bool voxel)
    {
        return fillType;
    }
}
