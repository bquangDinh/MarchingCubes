using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel
{
    public Vector3 position;

    public bool state;

    public float density;

    public Voxel()
    {

    }

    public Voxel(int x, int y, int z, float size)
    {
        position.x = (x + 0.5f) * size;
        position.y = (y + 0.5f) * size;
        position.z = (z + 0.5f) * size;
        density = 0;
    }

    //clone v's properties and move this voxel along X axis by offset
    public void BecomeDummyXOf(Voxel v, float offset)
    {
        state = v.state;
        position = v.position;

        position.x += offset;
    }

    //clone v's properties and move this voxel along Y axis by offset
    public void BecomeDummyYOf(Voxel v, float offset)
    {
        state = v.state;
        position = v.position;

        position.y += offset;
    }

    public void BecomeDummyZOf(Voxel v, float offset)
    {
        state = v.state;
        position = v.position;

        position.z += offset;
    }

    public void BecomeDummyXYZOf(Voxel v, float offset)
    {
        state = v.state;
        position = v.position;

        position.x += offset;
        position.y += offset;
        position.z += offset;
    }
}
