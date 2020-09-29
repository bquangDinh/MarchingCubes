using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadGenerator : MonoBehaviour
{
    public Vector3[] verticles = { Vector3.zero, Vector3.zero, Vector3.zero };

    private Mesh mesh;

    private int[] indicates = { 0, 2, 1 };
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Test Mesh";    
    }

    // Update is called once per frame
    void Update()
    {
        mesh.Clear();
        mesh.vertices = verticles;
        mesh.triangles = indicates;
    }
}
