using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

public class TriangleSurface : MonoBehaviour
{
    private Mesh _mesh;

    public Vector3[] vertices;
    public int[] triangles;

    void Awake()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        //CreateShape();
        
        ReadVertices("Vertices.txt");
        ReadTriangles("Triangles.txt");
        
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
                
                for (int i = 1; i <= arraySize; i++)
                {
                    if (i < text.Length)
                    {
                        string[] strValues = text[i].Split(' ');

                        if (strValues.Length == 3)
                        {
                            float x = float.Parse(strValues[0]);
                            float y = float.Parse(strValues[1]);
                            float z = float.Parse(strValues[2]);

                            Vector3 vertex = new Vector3(x, y, z);
                            vectorList.Add(vertex);
                        }
                    }
                }
                vertices = vectorList.ToArray();
            }
        }
        else
        {
            Debug.Log("Filepath not found: " + filePath);
        }
    }

    void ReadTriangles(string filename)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, filename);
        List<int> indices = new List<int>();

        if (File.Exists(filePath))
        {
            string[] text = File.ReadAllLines(filePath);

            if (text.Length > 0)
            {
                // Size of array is the first element, returns number of triangles
                // * 3 (returns number of vertices)
                int arraySize = int.Parse(text[0]) * 3;
                
                for (int i = 1; i <= arraySize; i++)
                {
                    if (i < text.Length)
                    {
                        string[] strValues = text[i].Split(' ');
                        
                        if (strValues.Length >= 6)
                        {
                            // Only add the first three indices to our array
                            indices.Add(int.Parse(strValues[0]));
                            indices.Add(int.Parse(strValues[1]));
                            indices.Add(int.Parse(strValues[2]));
                    
                            // Skip the next ones (holds neighbor information)
                        }
                    }
                }
                triangles = indices.ToArray();
            }
        }
        else
        {
            Debug.Log("Filepath not found: " + filePath);
        }
    }

    public static Vector3 BarycentricCoordinates(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 pt)
    {
        Vector2 p12 = p2 - p1;
        Vector2 p13 = p3 - p1;
        Vector3 n = Vector3.Cross(new Vector3(p12.x, 0.0f, p12.y), new Vector3(p13.x, 0.0f, p13.y));
        float areal123 = n.magnitude;
        Vector3 baryc = default;
        // u
        Vector2 p = p2 - pt;
        Vector2 q = p3 - pt;
        n = Vector3.Cross(new Vector3(p.x, 0.0f, p.y), new Vector3(q.x, 0.0f, q.y));
        baryc.x = n.y / areal123;
        // v
        p = p3 - pt;
        q = p1 - pt;
        n = Vector3.Cross(new Vector3(p.x, 0.0f, p.y), new Vector3(q.x, 0.0f, q.y));
        baryc.y = n.y / areal123;
        // w
        p = p1 - pt;
        q = p2 - pt;
        n = Vector3.Cross(new Vector3(p.x, 0.0f, p.y), new Vector3(q.x, 0.0f, q.y));
        baryc.z = n.y / areal123;

        return baryc;
    }
    
    public float GetSurfaceHeight(Vector2 p)
    {
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p0 = vertices[triangles[i]];
            Vector3 p1 = vertices[triangles[i + 1]];
            Vector3 p2 = vertices[triangles[i + 2]];
            
            Vector3 baryCoords = BarycentricCoordinates(
                new Vector2(p0.x, p0.z), 
                new Vector2(p1.x, p1.z),  
                new Vector2(p2.x, p2.z),  
                p
            );
            
            // Check if the player's position is inside the triangle.
            if (baryCoords is { x: >= 0.0f, y: >= 0.0f, z: >= 0.0f })
            {
                // The player's position is inside the triangle.
                // Calculate the height of the surface at the player's position.
                float height = baryCoords.x * p0.y + baryCoords.y * p1.y + baryCoords.z * p2.y;
                
                // Return the height as the height of the surface at the player's position.
                return height;
            }
        }
        return 0.0f;
    }
    
    void CreateShape()
    {
        vertices = new Vector3[]
        {
            new(0.01f, 0.097f, 0.0f),
            new(0.4f, 0.005f, 0.0f),
            new(0.0f, 0.005f, 0.4f),
            new(0.4f, 0.075f, 0.4f),
            new(0.8f, 0.007f, 0.4f),
            new(0.8f, 0.039f, 0.0f)
        };

        triangles = new[]
        {
            2, 1, 0,
            // 2, 3, 1,
            // 4, 1, 3,
            // 1, 4, 5
        };
    }
}
