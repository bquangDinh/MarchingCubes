using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelCircularStencil : VoxelStencil
{
    private int squareRadius;
    public override void Initialize(bool fillType, int radius)
    {
        base.Initialize(fillType, radius);
        squareRadius = radius * radius;
    }

    public override bool Apply(int x, int y, int z, bool voxel)
    {
        //convert x, y to local coordinate in the circle
        //ex: if x,y are the center coordinates, then x,y = 0 in the circle, means they are center coordiante of the circle
        x -= centerX;
        y -= centerY;

        //use pythagorean to determine whether the point is outside or inside the circle
        if (x * x + y * y + z * z <= squareRadius)
        {
            return fillType;
        }

        return voxel;
    }
}
