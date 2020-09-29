using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public struct Dimensions{
    public int x;
    public int y;
    public int z;
}

[SelectionBase]
public class VoxelMap : MonoBehaviour
{
    public float voxelSize;

    public VoxelGrid voxelGridPrefab;

    private VoxelGrid[] chunks;

    private float isolevel;

    public float isoScale = 2.0f;

    public bool showVoxel = true;

    private bool oldShowVoxel = false;

    //private float chunkSize, voxelSize, halfSize;

    private Vector3 mapSize, chunkSize, halfMapSize;

    /*GUI Components*/
    private int fillTypeIndex;
    private string[] fillNames = { "Fill", "Empty" };

    private int radiusIndex;
    private string[] radiusNames = { "0", "1", "2", "3", "4", "5" };

    private int paintTypeIndex;
    private string[] paintNames = { "Box", "Sphere" };

    private VoxelStencil[] stencils = { new VoxelStencil(), new VoxelCircularStencil() };
    /*--------------------------*/

    [Header("Resolution")]
    public Dimensions voxelResolution; // number of voxel of each chunk

    public Dimensions chunkResolution; // number of chunk in the map along axis

    [Header("Main Noise")]
    public float scale = 1.0f;
    public float frequency = 1.0f;
    public float amplitude = 1.0f;
    public int octaves = 1;
    public NoiseType noiseType;

    private Noise noise;
    private Noise oldNoise;

    private void Awake()
    {
        Initialize();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    void Initialize()
    {
        chunkSize = new Vector3(voxelSize * voxelResolution.x, voxelSize * voxelResolution.y, voxelSize * voxelResolution.z);
        mapSize = new Vector3(chunkSize.x * chunkResolution.x, chunkSize.y * chunkResolution.y, chunkSize.z * chunkResolution.z);
        halfMapSize = new Vector3(mapSize.x / 2, mapSize.y / 2, mapSize.z / 2);

        chunks = new VoxelGrid[chunkResolution.x * chunkResolution.y * chunkResolution.z];

        noise = new Noise();
        noise.SetNoise(scale, frequency, amplitude, octaves, noiseType);

        oldNoise = new Noise();

        CreateChunkMap();
    }

    void CreateChunkMap()
    {
        for(int z = 0; z < chunkResolution.z; ++z)
        {
            for(int y = 0; y < chunkResolution.y; ++y)
            {
                for(int x = 0; x < chunkResolution.x; ++x)
                {
                    CreateChunk(x ,y ,z);
                }
            }
        }
    }

    void CreateChunk(int x, int y, int z)
    {
        VoxelGrid grid = Instantiate(voxelGridPrefab);
        grid.transform.parent = transform;
        grid.transform.localPosition = new Vector3((x * chunkSize.x - halfMapSize.x), (y * chunkSize.y - halfMapSize.y), (z * chunkSize.z - halfMapSize.z));
        grid.Initialize(voxelResolution, voxelSize);
        chunks[MathUtils.getIndexFromXYZ(x, y, z, chunkResolution)] = grid;

        if(x > 0)
        {
            chunks[MathUtils.getIndexFromXYZ(x - 1, y, z, chunkResolution)].xNeighbor = grid;
        }

        if(y > 0)
        {
            chunks[MathUtils.getIndexFromXYZ(x, y - 1, z, chunkResolution)].yNeighbor = grid;
        }

        if(z > 0)
        {
            chunks[MathUtils.getIndexFromXYZ(x, y, z - 1, chunkResolution)].zNeighbor = grid;

            if(x > 0 && y > 0)
            {
                chunks[MathUtils.getIndexFromXYZ(x - 1, y - 1, z - 1, chunkResolution)].xyzNeighbor = grid;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleEvent();
        HandleNoise();

        if(showVoxel != oldShowVoxel)
        {
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].showVoxel = showVoxel;
                chunks[i].isChanged = true;
            }
            oldShowVoxel = showVoxel;
        }
    }

    void HandleEvent()
    {
        if (Input.GetMouseButton(0)) // left click
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                if (hitInfo.collider.gameObject.tag == "Voxel")
                {
                    EditVoxels(hitInfo.point);
                }
            }
        }
    }

    void HandleNoise()
    {
        noise.SetNoise(scale, frequency, amplitude, octaves, noiseType);
        if(!noise.equals(oldNoise))
        {
            for (int i = 0; i < chunks.Length; ++i)
            {
                chunks[i].noise_generator = noise;
                chunks[i].isChanged = true;
            }
            oldNoise.Clone(noise);
        }
    }

    void EditVoxels(Vector3 point)
    {
        //get voxel coordinates from point
        //local coordinate in voxel map
        int voxelX = (int)((point.x + halfMapSize.x)/ voxelSize);
        int voxelY = (int)((point.y + halfMapSize.y) / voxelSize);
        int voxelZ = (int)((point.z + halfMapSize.z) / voxelSize);

        //get chunk coordinate which stores clicked voxel
        int chunkX = voxelX / voxelResolution.x;
        int chunkY = voxelY / voxelResolution.y;
        int chunkZ = voxelZ / voxelResolution.z;

        //get local position of voxel in the considered chunk
        voxelX -= chunkX * voxelResolution.x;
        voxelY -= chunkY * voxelResolution.y;
        voxelZ -= chunkZ * voxelResolution.z;

        chunks[MathUtils.getIndexFromXYZ(chunkX, chunkY, chunkZ, chunkResolution)].SetVoxel(voxelX, voxelY, voxelZ);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(4f, 4f, 150f, 500f));
        GUILayout.Label("Fill Type");
        fillTypeIndex = GUILayout.SelectionGrid(fillTypeIndex, paintNames, 2);
        GUILayout.Label("Radius");
        radiusIndex = GUILayout.SelectionGrid(radiusIndex, radiusNames, 6);
        GUILayout.Label("Paint Type");
        paintTypeIndex = GUILayout.SelectionGrid(paintTypeIndex, paintNames, 2);
        GUILayout.EndArea();
    }

    public void SliderCallback(float value)
    {
        isolevel = value * isoScale;

        for (int i = 0; i < chunks.Length; ++i)
        {
            chunks[i].isoLevel = isolevel;
            chunks[i].isChanged = true;
        }
    }
}
