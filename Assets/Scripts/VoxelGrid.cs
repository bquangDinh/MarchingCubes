using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class VoxelGrid : MonoBehaviour
{
    public Dimensions resolution; // number of voxel each axis

    public float scaleFactor = 0.1f;

    public float isoLevel = 0.0f;

    public bool showVoxel = true;

    public GameObject voxelPrefab;

    [HideInInspector]
    public Voxel[] voxels;

    private MeshRenderer[] voxelMaterials;

    private float voxelSize, gridSize;

    private Mesh mesh;

    private List<Vector3> verticles;

    private List<int> triangles;

    [HideInInspector]
    public VoxelGrid xNeighbor, yNeighbor, zNeighbor, xyzNeighbor;

    [HideInInspector]
    public Noise noise_generator;

    [HideInInspector]
    public bool isChanged = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize(Dimensions resolution, float size)
    {
        this.resolution = resolution;
        voxelSize = size;

        voxels = new Voxel[resolution.x * resolution.y * resolution.z];
        voxelMaterials = new MeshRenderer[resolution.x * resolution.y * resolution.z];

        verticles = new List<Vector3>();
        triangles = new List<int>();

        noise_generator = new Noise();

        GetComponent<MeshFilter>().mesh = mesh = new Mesh();

        CreateVoxelGrid();

        Refresh();
    }

    void CreateVoxelGrid()
    {
        for(int z = 0; z < resolution.z; ++z)
        {
            for(int y = 0; y < resolution.y; ++y)
            {
                for(int x = 0; x < resolution.x; ++x)
                {
                    CreateVoxel(x, y, z);
                }
            }
        }
    }

    void CreateVoxel(int x, int y, int z)
    {
        GameObject o = Instantiate(voxelPrefab);
        o.transform.parent = transform;
        o.transform.localPosition = new Vector3((x + 0.5f) * voxelSize, (y + 0.5f) * voxelSize, (z + 0.5f) * voxelSize);
        o.transform.localScale = Vector3.one * voxelSize * scaleFactor;
        voxels[MathUtils.getIndexFromXYZ(x, y, z, resolution)] = new Voxel(x, y, z, voxelSize);
        voxels[MathUtils.getIndexFromXYZ(x, y, z, resolution)].density = 0.0f;
        voxelMaterials[MathUtils.getIndexFromXYZ(x, y, z, resolution)] = o.GetComponent<MeshRenderer>();

        /*
        if (x == 0 && y == 0 && z == 0) voxelMaterials[MathUtils.getIndexFromXYZ(x, y, z, resolution)].color = Color.blue; // a
        if (x == 1 && y == 0 && z == 0) voxelMaterials[MathUtils.getIndexFromXYZ(x, y, z, resolution)].color = Color.white; // b
        if (x == 1 && y == 1 && z == 0) voxelMaterials[MathUtils.getIndexFromXYZ(x, y, z, resolution)].color = Color.grey; // c
        if (x == 0 && y == 1 && z == 0) voxelMaterials[MathUtils.getIndexFromXYZ(x, y, z, resolution)].color = Color.black; // d

        if (x == 0 && y == 0 && z == 1) voxelMaterials[MathUtils.getIndexFromXYZ(x, y, z, resolution)].color = Color.red; // e
        if (x == 1 && y == 0 && z == 1) voxelMaterials[MathUtils.getIndexFromXYZ(x, y, z, resolution)].color = Color.green; // f 
        if (x == 1 && y == 1 && z == 1) voxelMaterials[MathUtils.getIndexFromXYZ(x, y, z, resolution)].color = Color.yellow; // g
        if (x == 0 && y == 1 && z == 1) voxelMaterials[MathUtils.getIndexFromXYZ(x, y, z, resolution)].color = Color.cyan; // h
        */
    }

    float calcDensity(Vector3 p)
    {
        Vector3 wP = transform.TransformPoint(p);

        float density = noise_generator.calcNoise2D(wP);

        return density;
    }

    public void SetVoxel(int x, int y, int z)
    {
        //voxels[MathUtils.getIndexFromXYZ(x, y, z, resolution)].density = 1.0f;
        voxelMaterials[MathUtils.getIndexFromXYZ(x, y, z, resolution)].material.color = Color.white;
        //Refresh();
    }

    void Refresh()
    {
        Triangulate();
    }

    void Triangulate()
    {
        verticles.Clear();
        triangles.Clear();
        mesh.Clear();

        TriangulateCells();

        if(zNeighbor != null)
        {
            TriangulateGapRow();
        }

        mesh.vertices = verticles.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void TriangulateCells()
    {
        int numberOfCellsX = resolution.x - 1;
        int numberOfCellsY = resolution.y - 1;
        int numberOfCellsZ = resolution.z - 1;
        int a, b, c, d, e, f, g, h;

        for(int z = 0; z < numberOfCellsZ; ++z)
        {
            for(int y = 0; y < numberOfCellsY; ++y)
            {
                int x;
                for(x = 0; x < numberOfCellsX; ++x)
                {
                    a = MathUtils.getIndexFromXYZ(x, y, z, resolution);
                    b = MathUtils.getIndexFromXYZ(x + 1, y, z, resolution);
                    c = MathUtils.getIndexFromXYZ(x + 1, y + 1, z, resolution);
                    d = MathUtils.getIndexFromXYZ(x, y + 1, z, resolution);

                    e = MathUtils.getIndexFromXYZ(x, y, z + 1, resolution);
                    f = MathUtils.getIndexFromXYZ(x + 1, y, z + 1, resolution);
                    g = MathUtils.getIndexFromXYZ(x + 1, y + 1, z + 1, resolution);
                    h = MathUtils.getIndexFromXYZ(x, y + 1, z + 1, resolution);

                    TriangulateCell(voxels[a], voxels[b], voxels[c], voxels[d], voxels[e], voxels[f], voxels[g], voxels[h]);
                }
                if(xNeighbor != null)
                {
                    TriangulateGapCell(x, y, z);
                }
            }
        }
    }

    void TriangulateGapCell(int x, int y, int z)
    {
        /*
        Voxel dummySwap1 = dummyT1;
        Voxel dummySwap2 = dummyT2;
        dummySwap1.BecomeDummyXOf(xNeighbor.voxels[MathUtils.getIndexFromXYZ(0, y, z + 1, resolution)], gridSize); // f
        dummySwap2.BecomeDummyXOf(xNeighbor.voxels[MathUtils.getIndexFromXYZ(0, y + 1, z + 1, resolution)], gridSize); // g

        dummyT1 = dummy1; // b
        dummy1 = dummySwap1; // f

        dummyT2 = dummy2; // c
        dummy2 = dummySwap2; // g
        */
        Voxel c, b, f, g;

        c = new Voxel();
        b = new Voxel();
        f = new Voxel();
        g = new Voxel();
        
        c.BecomeDummyXOf(xNeighbor.voxels[MathUtils.getIndexFromXYZ(0, y + 1, z, resolution)], gridSize);
        b.BecomeDummyXOf(xNeighbor.voxels[MathUtils.getIndexFromXYZ(0, y, z, resolution)], gridSize);
        f.BecomeDummyXOf(xNeighbor.voxels[MathUtils.getIndexFromXYZ(0, y, z + 1, resolution)], gridSize);
        g.BecomeDummyXOf(xNeighbor.voxels[MathUtils.getIndexFromXYZ(0, y + 1, z + 1, resolution)], gridSize);

        TriangulateCell(
            voxels[MathUtils.getIndexFromXYZ(x, y, z, resolution)],
            b,
            c,
            voxels[MathUtils.getIndexFromXYZ(x, y + 1, z, resolution)],
            voxels[MathUtils.getIndexFromXYZ(x, y, z + 1, resolution)],
            f,
            g,
            voxels[MathUtils.getIndexFromXYZ(x, y + 1, z + 1, resolution)]
            );
    }

    void TriangulateGapRow()
    {
        /*
        int numCells = resolution - 1;

        Voxel e, f, g, h;

        e = new Voxel();
        f = new Voxel();
        g = new Voxel();
        h = new Voxel();

        for(int y = 0; y < numCells; ++y)
        {
            for(int x = 0; x < numCells; ++x)
            {
                e.BecomeDummyZOf(zNeighbor.voxels[MathUtils.getIndexFromXYZ(x, y, 0, resolution)], gridSize);
                f.BecomeDummyZOf(zNeighbor.voxels[MathUtils.getIndexFromXYZ(x + 1, y, 0, resolution)], gridSize);
                g.BecomeDummyZOf(zNeighbor.voxels[MathUtils.getIndexFromXYZ(x + 1, y + 1, 0, resolution)], gridSize);
                h.BecomeDummyZOf(zNeighbor.voxels[MathUtils.getIndexFromXYZ(x, y + 1, 0, resolution)], gridSize);

               

                TriangulateCell(
                    voxels[MathUtils.getIndexFromXYZ(x, y, resolution - 1, resolution)],
                    voxels[MathUtils.getIndexFromXYZ(x + 1, y, resolution - 1, resolution)],
                    voxels[MathUtils.getIndexFromXYZ(x + 1, y + 1, resolution - 1, resolution)],
                    voxels[MathUtils.getIndexFromXYZ(x, y + 1, resolution - 1, resolution)],
                    e,
                    f,
                    g,
                    h
                    );
            }
        }
        */
    }

    void TriangulateCell(Voxel a, Voxel b, Voxel c, Voxel d, Voxel e, Voxel f, Voxel g, Voxel h)
    {
        int cubeIndex = 0;
        Vector3[] edges = new Vector3[12]; // 12 edges of a cell

        //use bitmask to figure out edge configuration
        if (a.density > isoLevel) cubeIndex |= 1;
        if (b.density > isoLevel) cubeIndex |= 2;
        if (c.density > isoLevel) cubeIndex |= 4;
        if (d.density > isoLevel) cubeIndex |= 8;
        if (e.density > isoLevel) cubeIndex |= 16;
        if (f.density > isoLevel) cubeIndex |= 32;
        if (g.density > isoLevel) cubeIndex |= 64;
        if (h.density > isoLevel) cubeIndex |= 128;

        //Debug.Log("Cube Index: " + cubeIndex);

        if(Table.edgeTable[cubeIndex] == 0)
        {
            //entire cell inside or outside the surface
            return;
        }

        if ((Table.edgeTable[cubeIndex] & 1) != 0)
        {
            edges[0] = VertexInterp(isoLevel, a.position, b.position, a.density, b.density);
        }
        if ((Table.edgeTable[cubeIndex] & 2) != 0)
        {
            edges[1] = VertexInterp(isoLevel, b.position, c.position, b.density, c.density);
        }
        if ((Table.edgeTable[cubeIndex] & 4) != 0)
        {
            edges[2] = VertexInterp(isoLevel, c.position, d.position, c.density, d.density);
        }
        if ((Table.edgeTable[cubeIndex] & 8) != 0)
        {
            edges[3] = VertexInterp(isoLevel, d.position, a.position, d.density, a.density);
        }
        if ((Table.edgeTable[cubeIndex] & 16) != 0)
        {
            edges[4] = VertexInterp(isoLevel, e.position, f.position, e.density, f.density);
        }
        if ((Table.edgeTable[cubeIndex] & 32) != 0)
        {
            edges[5] = VertexInterp(isoLevel, f.position, g.position, f.density, g.density);
        }
        if ((Table.edgeTable[cubeIndex] & 64) != 0)
        {
            edges[6] = VertexInterp(isoLevel, g.position, h.position, g.density, h.density);
        }
        if ((Table.edgeTable[cubeIndex] & 128) != 0)
        {
            edges[7] = VertexInterp(isoLevel, h.position, e.position, h.density, e.density);
        }
        if ((Table.edgeTable[cubeIndex] & 256) != 0)
        {
            edges[8] = VertexInterp(isoLevel, a.position, e.position, a.density, e.density);
        }
        if ((Table.edgeTable[cubeIndex] & 512) != 0)
        {
            edges[9] = VertexInterp(isoLevel, b.position, f.position, b.density, f.density);
        }
        if ((Table.edgeTable[cubeIndex] & 1024) != 0)
        {
            edges[10] = VertexInterp(isoLevel, c.position, g.position, c.density, g.density);
        }
        if ((Table.edgeTable[cubeIndex] & 2048) != 0)
        {
            edges[11] = VertexInterp(isoLevel, d.position, h.position, d.density, h.density);
        }

        for(int i = 0; Table.triTable[cubeIndex,i] != -1; i += 3)
        {
            AddTriangle(edges[Table.triTable[cubeIndex, i]], edges[Table.triTable[cubeIndex, i + 1]], edges[Table.triTable[cubeIndex, i + 2]]);
        }
    }

    Vector3 VertexInterp(float isolevel, Vector3 p1, Vector3 p2, float val1, float val2)
    {
        //return (p1 + p2) / 2;
 
        if (Mathf.Abs(isolevel - val1) < 0.00001f) return p1;
        if (Mathf.Abs(isolevel - val2) < 0.00001f) return p2;
        if (Mathf.Abs(val2 - val1) < 0.00001f) return p1;

        float mu = (isoLevel - val1) / (val2 - val1);

        Vector3 p = Vector3.zero;

        p.x = p1.x + mu * (p2.x - p1.x);
        p.y = p1.y + mu * (p2.y - p1.y);
        p.z = p1.z + mu * (p2.z - p1.z);

        return p;
    }

    void AddTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        int vertexIndex = verticles.Count;

        verticles.Add(p1);
        verticles.Add(p2);
        verticles.Add(p3);

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
    }

    // Update is called once per frame
    void Update()
    {
        float density;
        if(isChanged)
        {
            for (int i = 0; i < voxels.Length; ++i)
            {
                density = calcDensity(voxels[i].position);
                voxels[i].density = density;
                voxelMaterials[i].material.color = (density - isoLevel) > 0 ? Color.white : Color.black;
                voxelMaterials[i].enabled = showVoxel;
            }
            Refresh();
            isChanged = false;
        }      
    }
}
