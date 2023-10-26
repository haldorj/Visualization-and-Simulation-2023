using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class PointCloudVisualizer : MonoBehaviour
{
    public Vector3[] vertices;

    public Material material;
    public Mesh mesh;

    private void Start()
    {
        ReadVertices("mergedCompressedHIGH.txt");
    }

    private void Update()
    {
        if (vertices == null) return;
        RenderParams rp = new RenderParams(material);
        Matrix4x4[] instData = new Matrix4x4[vertices.Length];
        for(int i=0; i<vertices.Length; ++i)
            instData[i] = Matrix4x4.Translate(vertices[i]);
        Graphics.RenderMeshInstanced(rp, mesh, 0, instData);
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
                vertices = vectorList.ToArray();
            }
        }
        else
        {
            Debug.Log("Filepath not found: " + filePath);
        }
    }

    // void OnDrawGizmos()
    // {
    //     if (vertices == null || vertices.Length <= 0) return;
    //
    //     foreach (var vertex in vertices)
    //     {
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawCube(vertex, Vector3.one * 1f);
    //     }
    // }
}
