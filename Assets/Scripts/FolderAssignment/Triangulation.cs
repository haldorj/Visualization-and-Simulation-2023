using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

public class Triangulation : MonoBehaviour
{
    private Mesh _mesh;

    public string path = "mergedCompressedHIGH.txt";

    [SerializeField]private float minX = float.MaxValue;
    [SerializeField]private float minZ = float.MaxValue;
    [SerializeField]private float maxX = float.MinValue;
    [SerializeField]private float maxZ = float.MinValue;

    [FormerlySerializedAs("triangleSize")] public int quadSize = 100;
    
    private Vector3[] _vertexData;
    
    public Vector3[] corners;
    public Vector3[] vertices;
    public int[] triangles;

    public struct Quad
    {
        public float XMax;
        public float XMin;
        public float ZMax;
        public float ZMin;
        
        public Vector3 Center;
    }

    private List<Quad> _quads = new();
    
    struct Triangle
    {
        private int[] _indices;
        private int[] _neighbours;
    }

    private void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        
        ReadVertexData(path);
        FindExtremeValues();
        CreateQuads();

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
    
    void ConstructMeshData()
    {
        List<Vector3> vertexList = new List<Vector3>();

        foreach (var quad in _quads)
        {
            vertexList.Add(quad.Center);
        }

        vertices = vertexList.ToArray();

        int vert = 0;
        List<int> triangleList = new List<int>();
        
        Debug.Log(maxX/quadSize);
        
        for (int z = 0; z < maxZ - quadSize * 2; z+= quadSize)
        {
            for (int x = 0; x < (int)maxX - quadSize*2; x+= quadSize)
            {
                    // triangleList.Add(vert + 0);
                    // triangleList.Add(vert + (int)maxX/quadSize);
                    // triangleList.Add(vert + 1);
                    // triangleList.Add(vert + 0);
                    // triangleList.Add(vert + (int)maxX/quadSize - 1);
                    // triangleList.Add(vert + (int)maxX/quadSize);
                    
                    triangleList.Add(vert + 0);
                    triangleList.Add(vert + (int)maxX/quadSize - 1);
                    triangleList.Add(vert + 1);
                    triangleList.Add(vert + 1);
                    triangleList.Add(vert + (int)maxX/quadSize - 1);
                    triangleList.Add(vert + (int)maxX/quadSize);

                    vert++;
            }
            vert++;
        }
        
        triangles = triangleList.ToArray();
    }

    void CreateQuads()
    {
        List<Vector3> vectorList = new List<Vector3>();
        
        int quadXCount = (int)(maxX / quadSize);
        int quadZCount = (int)(maxZ / quadSize);
    
        for (int z = 0; z < quadZCount; z++)
        {
            for (int x = 0; x < quadXCount; x++)
            {
                vectorList.Add(new Vector3(x * quadSize, 0, z * quadSize));

                if (z < quadZCount - 1 && x < quadXCount - 1)
                {
                    Quad quad = new Quad
                    {
                        XMin = x * quadSize,
                        XMax = (x + 1) * quadSize,
                        ZMin = z * quadSize,
                        ZMax = (z + 1) * quadSize,
                        Center = new Vector3(x * quadSize + quadSize / 2, 0, z * quadSize + quadSize / 2)
                    };
                    
                    quad.Center.y = CalculateAverageHeightInQuad(quad);
                    
                    _quads.Add(quad);
                }
            }
            corners = vectorList.ToArray();
        }
    }

    void FindExtremeValues()
    {
        if (_vertexData == null || _vertexData.Length == 0) return;

        foreach (var vertex in _vertexData)
        {
            minX = Mathf.Min(minX, vertex.x);
            minZ = Mathf.Min(minZ, vertex.z);
            maxX = Mathf.Max(maxX, vertex.x);
            maxZ = Mathf.Max(maxZ, vertex.z);
        }

        int quadXCount = (int)(maxX / quadSize);
        int quadZCount = (int)(maxZ / quadSize);
        _quads = new List<Quad>(quadXCount * quadZCount);
    }

    float CalculateAverageHeightInQuad(Quad quad)
    {
        int pointCount = 0;
        float totalHeight = 0;

        foreach (var vertex in _vertexData)
        {
            if (vertex.x >= quad.XMin && vertex.x <= quad.XMax && vertex.z >= quad.ZMin && vertex.z <= quad.ZMax)
            {
                totalHeight += vertex.y;
                pointCount++;
            }
        }

        // Calculate the average height (avoid division by zero)
        return pointCount > 0 ? totalHeight / pointCount : 0;
    }

    void ReadVertexData(string filename)
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
    
    // private void OnDrawGizmos()
    // {
    //     if (corners == null || corners.Length <= 0) return;
    //     foreach (var vertex in corners)
    //     {
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawCube(vertex, Vector3.one * 2);
    //     }
    //
    //     foreach (var quad in _quads)
    //     {
    //         Gizmos.color = Color.blue;
    //         Gizmos.DrawCube(quad.Center, Vector3.one * 5);
    //     }
    // }
}
