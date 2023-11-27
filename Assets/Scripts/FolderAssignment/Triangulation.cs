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
    [SerializeField]private float minY = float.MaxValue;
    [SerializeField]private float minZ = float.MaxValue;
    [SerializeField]private float maxX = float.MinValue;
    [SerializeField]private float maxY = float.MinValue;
    [SerializeField]private float maxZ = float.MinValue;
    
    public int quadSize = 10;
    
    private Vector3[] _vertexData;
    
    public Vector3[] corners;
    public Vector3[] vertices;
    public int[] triangles;
    
    [SerializeField]private Color[] colors;
    public Gradient gradient;

    // Only used with camera class
    public MeshCollider meshCollider;
    
    public struct Quad
    {
        public float XMax;
        public float XMin;
        public float ZMax;
        public float ZMin;
        
        public Vector3 Center;
    }

    private List<Quad> _quads = new();

    public List<int>[] Grid;
    public int cellSize = 10;
    public int numCellsX;
    public int numCellsZ;
    

    private void Awake()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        meshCollider = GetComponent<MeshCollider>();
        
        ReadVertexData(path);
        FindExtremeValues();
        CreateQuads();

        ConstructMeshData();
        UpdateMesh();

        InitializeGrid();
    }
    
    void UpdateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;

        colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            float height = Mathf.InverseLerp(minY, maxY, vertices[i].y);
            colors[i] = gradient.Evaluate(height);
        }
        
        _mesh.colors = colors;
        _mesh.RecalculateTangents();
        _mesh.RecalculateNormals();
        meshCollider.sharedMesh = _mesh;
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

        for (int z = 0; z < (int)maxZ - quadSize * 2; z+= quadSize)
        {
            for (int x = 0; x < (int)maxX - quadSize * 2; x+= quadSize)
            {
                
                    //  2 ---- 3
                    //  |  \   |
                    //  |   \  |
                    //  0 ---- 1
                    
                    triangleList.Add(vert + (int)maxX/quadSize - 1);    // 2
                    triangleList.Add(vert + 1);                         // 1
                    triangleList.Add(vert + 0);                         // 0
                    triangleList.Add(vert + (int)maxX/quadSize - 1);    // 2
                    triangleList.Add(vert + (int)maxX/quadSize);        // 3
                    triangleList.Add(vert + 1);                         // 1

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
            minY = Mathf.Min(minY, vertex.y);
            minZ = Mathf.Min(minZ, vertex.z);
            maxX = Mathf.Max(maxX, vertex.x);
            maxY = Mathf.Max(maxY, vertex.y);
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

        // If num of points are more than zero we divide the total height on the number of points.
        // Otherwise, return 0.
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
                            
                            Vector3 vertex = new Vector3(x, y, z) * 0.1f;
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

    void InitializeGrid()
    {
        // Calculate the number of cells based on the terrain size and cell size
        numCellsX = Mathf.CeilToInt((maxX - minX) / cellSize);
        numCellsZ = Mathf.CeilToInt((maxZ - minZ) / cellSize);

        // Create a grid with empty lists for each cell
        Grid = new List<int>[numCellsX * numCellsZ];
        for (int i = 0; i < Grid.Length; i++)
        {
            Grid[i] = new List<int>();
        }

        // Populate the grid with triangle indices based on vertices
        for (int i = 0; i < triangles.Length; i += 3)
        {
        }
    }
    
    // private void OnDrawGizmos()
    // {
    //     if (corners == null || corners.Length <= 0) return;
    //     foreach (var vertex in corners)
    //     {
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawCube(vertex, Vector3.one * .2f);
    //     }
    //
    //     foreach (var quad in _quads)
    //     {
    //         Gizmos.color = Color.blue;
    //         Gizmos.DrawCube(quad.Center, Vector3.one * .5f);
    //     }
    // }
}
