using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[]triangles;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[]
        {
            new Vector3(0.0f, 0.097f, 0.0f),
            new Vector3(0.4f, 0.005f, 0.0f),
            new Vector3(0.0f, 0.005f, 0.4f),
            new Vector3(0.4f, 0.075f, 0.4f),
            new Vector3(0.8f, 0.007f, 0.4f),
            new Vector3(0.8f, 0.039f, 0.0f)
        };

        triangles = new int[]
        {
            2, 1, 0,
            2, 3, 1,
            4, 1, 3,
            1, 4, 5
        };
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
