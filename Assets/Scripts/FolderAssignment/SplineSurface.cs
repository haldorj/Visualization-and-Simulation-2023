using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

// This implementation takes heavy inspiration from:
// Bourke, P. (1996, November). Spline Surface (in 3D). from Paul Bourke: 
// https://paulbourke.net/geometry/spline

// The function "BasisFunction" is the same as "SplineBlend" from:
// Bourke, P. (1996, November). Spline curves (in 3D). from Paul Bourke: 
// https://paulbourke.net/geometry/spline/

public class SplineSurface : MonoBehaviour
{
    [Range(5, 8)]
    [SerializeField]public int numControlPointsX = 6;
    [Range(5, 7)]
    [SerializeField]public int numControlPointsY = 5;
    [Range(2,4)]
    public int degreeX = 3;
    [Range(2,4)]
    public int degreeY = 3;
    
    private Vector3[,] _controlPoints;
    
    public int[] knotsX;
    public int[] knotsY;
    public int resolutionX = 30;
    public int resolutionY = 40;
    private Vector3[,] _output;

    private Mesh _mesh;
    private List<Vector3> _vertices;
    private List<int> _triangles;
    
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    [SerializeField]private Color[] colors;
    public Gradient gradient;

    private void Awake()
    {
        _controlPoints = new Vector3[numControlPointsX, numControlPointsY];

        _output = new Vector3[resolutionX, resolutionY];
        
        knotsX = new int[numControlPointsX + degreeX + 1];
        knotsY = new int[numControlPointsY + degreeY + 1];
        
        knotsX = InitializeKnotVector(knotsX,numControlPointsX,degreeX);
        knotsY = InitializeKnotVector(knotsY,numControlPointsY,degreeY);

        GenerateControlPoints();
        CalculateSplineSurface();
        GenerateMeshData();
        UpdateMesh();
    }

    private void GenerateControlPoints()
    {
        for (int i = 0; i < numControlPointsX; i++)
        {
            for (int j = 0; j < numControlPointsY; j++)
            {
                _controlPoints[i, j] = new Vector3(i, Random.Range(-1f, 1f), j);
            }
        }
    }

    private void CalculateSplineSurface()
    {
        var incrementX = (numControlPointsX - degreeX) / ((double)resolutionX - 1);
        var incrementY = (numControlPointsY - degreeY) / ((double)resolutionY - 1);

        double intervalI = 0;
        for (int i = 0; i < resolutionX - 1; i++)
        {
            double intervalJ = 0;
            for (int j = 0; j < resolutionY - 1; j++)
            {
                _output[i, j] = Vector3.zero;

                for (int ki = 0; ki < numControlPointsX; ki++)
                {
                    for (int kj = 0; kj < numControlPointsY; kj++)
                    {
                        var bi = BasisFunction(ki, degreeX + 1, knotsX, intervalI);
                        var bj = BasisFunction(kj, degreeY + 1, knotsY, intervalJ);
                        _output[i, j] += _controlPoints[ki, kj] * (float)(bi * bj);
                    }
                }
                intervalJ += incrementY;
            }
            intervalI += incrementX;
        }
    }

    private void GenerateMeshData()
    {
        _mesh = new Mesh();
        _vertices = new List<Vector3>();
        _triangles = new List<int>();

        for (int i = 0; i < resolutionX; i++)
        {
            for (int j = 0; j < resolutionY; j++)
            {
                _vertices.Add(_output[i, j]);
            }
        }

        for (int i = 0; i < resolutionX - 2; i++)
        {
            for (int j = 0; j < resolutionY - 2; j++)
            {
                int index = i * resolutionY + j;

                _triangles.Add(index);
                _triangles.Add(index + 1);
                _triangles.Add(index + resolutionY);

                _triangles.Add(index + resolutionY);
                _triangles.Add(index + 1);
                _triangles.Add(index + resolutionY + 1);
            }
        }

        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
    }

    private void UpdateMesh()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
        {
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null)
        {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        
        _meshFilter.mesh = _mesh;

        colors = new Color[_mesh.vertices.Length];
        for (int i = 0; i < _mesh.vertices.Length; i++)
        {
            float height = Mathf.InverseLerp(-1, 1, _vertices[i].y);
            colors[i] = gradient.Evaluate(height);
        }

        _mesh.colors = colors;
    }

    private static int[] InitializeKnotVector(int[] knots, int numRects, int degree)
    {
        int index = 0;

        List<int> k = new List<int>(); // knots

        for (int i = 0; i <= degree; i++)
        {
            k.Add(index);
        }

        index++;
        // c - d - 1
        if (numRects - degree - 1 > 0)
        {
            for (int i = 0; i < numRects - degree - 1; i++)
            {

                k.Add(index);
                index++;
            }
        }

        for (int i = 0; i <= degree; i++)
        {
            k.Add(index);
        }

        return k.ToArray();
    }

    private static double BasisFunction(int k,int t, int[] u,double v)
    {
        double value;

        if (t == 1) {
            if ((u[k] <= v) && (v < u[k+1]))
                value = 1;
            else
                value = 0;
        } else {
            if ((u[k+t-1] == u[k]) && (u[k+t] == u[k+1]))
                value = 0;
            else if (u[k+t-1] == u[k]) 
                value = (u[k+t] - v) / (u[k+t] - u[k+1]) * BasisFunction(k+1,t-1,u,v);
            else if (u[k+t] == u[k+1])
                value = (v - u[k]) / (u[k+t-1] - u[k]) * BasisFunction(k,t-1,u,v);
            else
                value = (v - u[k]) / (u[k+t-1] - u[k]) * BasisFunction(k,t-1,u,v) + 
                        (u[k+t] - v) / (u[k+t] - u[k+1]) * BasisFunction(k+1,t-1,u,v);
        }
        return(value);
    }

    private void OnDrawGizmos()
    {
        if (_controlPoints == null) return;
        Gizmos.color = Color.green;
        foreach (var point in _controlPoints)
        {
            Gizmos.DrawSphere(point, .04f);
        }
        for (int i = 0; i < numControlPointsX; i++)
        {
            for (int j = 0; j < numControlPointsY - 1; j++)
            {
                // Up
                Gizmos.DrawLine(_controlPoints[i, j], _controlPoints[i, j + 1]);
            }
        }
        for (int i = 0; i < numControlPointsX - 1; i++)
        {
            for (int j = 0; j < numControlPointsY; j++)
            {
                // Right
                Gizmos.DrawLine(_controlPoints[i, j], _controlPoints[i + 1, j]);
            }
        }
    }
}