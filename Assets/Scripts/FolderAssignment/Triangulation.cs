using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

public class Triangulation : MonoBehaviour
{
    private Mesh _mesh;

    [FormerlySerializedAs("filePath")] public string path = "mergedCompressedHIGH.txt";

    [SerializeField]private float minX = float.MaxValue;
    [SerializeField]private float minZ = float.MaxValue;
    [SerializeField]private float maxX = float.MinValue;
    [SerializeField]private float maxZ = float.MinValue;

    public int triangleSize = 10;
    
    private Vector3[] _vertexData;
    
    public Vector3[] vertices;
    public int[] triangles;
    
    struct Triangle
    {
        private int[] _indices;
        private int[] _neighbours;
    }

    private void Start()
    {
        ReadVertices(path);
        FindExtremeValues();
        ConstructMeshData();
        UpdateMesh();
    }
    
    void UpdateMesh()
    {
        _mesh.Clear();
        
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateTangents();
        _mesh.RecalculateNormals();
    }

    void FindExtremeValues()
    {
        // Return if the vertices array are empty or is null
        if (_vertexData == null || _vertexData.Length <= 0) return;

        // Find extreme (max & min) values
        foreach (var vertex in _vertexData)
        {
            if (vertex.x < minX)
            {
                minX = vertex.x;
            }
            if (vertex.z < minZ)
            {
                minZ = vertex.z;
            }
            if (vertex.x > maxX)
            {
                maxX = vertex.x;
            }
            if (vertex.z > maxZ)
            {
                maxZ = vertex.z;
            }
        }
    }

    void ConstructMeshData()
    {
        List<Vector3> vectorList = new List<Vector3>();
        
        for (int index = 0, z = (int)minZ; z <= (int)maxZ; z+= triangleSize)
        {
            for (int x = (int)minX; x <= (int)maxX; x+= triangleSize)
            {
                vectorList.Add(new Vector3(x, 0, z));
                index++;
            }
        }
        
        vertices = vectorList.ToArray();

        List<int> triangleList = new List<int>();
        
        int vert = 0;
        //int tris = 0;
        // for (int index = 0, z = (int)minZ; z <= (int)maxZ; z += triangleSize)
        // {
            for (int x = (int)minX; x <= (int)maxX; x += triangleSize)
            {
                triangleList.Add(vert + 0);
                triangleList.Add(vert + x + 1);
                triangleList.Add(vert + 1);
                triangleList.Add(vert + 1);
                triangleList.Add(vert + x + 1);
                triangleList.Add(vert + x + 2);

                vert++;
                //tris += 6;
            }
        //     vert++;
        // }
        triangles = triangleList.ToArray();
    }

    private void OnDrawGizmos()
    {
        if (vertices == null || vertices.Length <= 0) return;
        foreach (var vertex in vertices)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(vertex, Vector3.one * 5);
        }
    }


    void ReadVertices(string filename)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, filename);
        List<Vector3> vectorList = new List<Vector3>();

        if (File.Exists(filePath))
        {
            string[] text = File.ReadAllLines(filePath);

            if (text.Length > 0)
            {
                // Size of array is the first element
                int arraySize = int.Parse(text[0]);
                
                // Use: '.' as decimal separator instead of ','
                CultureInfo cultureInfo = new CultureInfo("en-US");
                
                for (int i = 1; i <= arraySize; i ++)
                {
                    if (i < text.Length)
                    {
                        string[] strValues = text[i].Split(' ');

                        if (strValues.Length == 3)
                        {
                            float x = float.Parse(strValues[0], cultureInfo);
                            float y = float.Parse(strValues[1], cultureInfo);
                            float z = float.Parse(strValues[2], cultureInfo);
                            
                            Vector3 vertex = new Vector3(x, y, z);
                            vectorList.Add(vertex);
                        }
                    }
                }
                _vertexData = vectorList.ToArray();
            }
        }
        else
        {
            Debug.Log("Filepath not found: " + filePath);
        }
    }
}
