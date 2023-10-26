using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class VertexDataGenerator : MonoBehaviour
{
    public string outputFilePath = "D:/VSIM/mergedCompressed.txt";
    
    public Vector3[] vertices;

    [SerializeField]private float _minX = float.MaxValue;
    [SerializeField]private float _minY = float.MaxValue;
    [SerializeField]private float _minZ = float.MaxValue;

    private void Start()
    {
        ReadVertices("merged.txt");
        FindMinValues();
        WriteToFile();
    }

    void FindMinValues()
    {
        // Return if the vertices array are empty or is null
        if (vertices == null || vertices.Length <= 0) return;

        // Find minimum values
        foreach (var vertex in vertices)
        {
            if (vertex.x < _minX)
            {
                _minX = vertex.x;
            }
            if (vertex.y < _minY)
            {
                _minY = vertex.y;
            }
            if (vertex.z < _minZ)
            {
                _minZ = vertex.z;
            }
        }

        // Update vertices by subtracting the minimum values
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= new Vector3(_minX, _minY, _minZ);
        }
    }

    void WriteToFile()
    {
        if (vertices == null || vertices.Length == 0)
        {
            Debug.LogError("Vector3 array is empty or null.");
            return;
        }

        string[] lines = new string[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vector = vertices[i];
            string line = string.Format("{0} {1} {2}", vector.x, vector.y, vector.z);
            lines[i] = line;
        }

        // Write the lines to the text file
        File.WriteAllLines(outputFilePath, lines);

        Debug.Log("Vector3 array data has been written to " + outputFilePath);
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

                const int increment = 1000;
                
                for (int i = 1; i <= arraySize; i += increment)
                {
                    if (i < text.Length)
                    {
                        string[] strValues = text[i].Split(' ');

                        if (strValues.Length == 3)
                        {
                            float x = float.Parse(strValues[0], cultureInfo);
                            float y = float.Parse(strValues[1], cultureInfo);
                            float z = float.Parse(strValues[2], cultureInfo);

                            // swapping y and z because of Unity
                            Vector3 vertex = new Vector3(x, z, y);
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
}
