using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SplineSurface : MonoBehaviour
{
    public int numControlPointsX;
    public int numControlPointsY;
    private int _numRectanglesX = 3;   //ni
    private int _numRectanglesY = 4;   //nj
    private Vector3[,] _controlPoints;
    public int degreeX = 3;          //ti
    public int degreeY = 3;          //tj
    public int[] knotsX;
    public int[] knotsY;
    public int resolutionX = 30;
    public int resolutionY = 40;
    private Vector3[,] _output;

    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    [SerializeField]private Color[] colors;
    public Gradient gradient;

    private void Start()
    {
        _numRectanglesX = numControlPointsX - 1;
        _numRectanglesY = numControlPointsY - 1;
        
        _controlPoints = new Vector3[_numRectanglesX + 1, _numRectanglesY + 1];
        knotsX = new int[_numRectanglesX + degreeX + 1];
        knotsY = new int[_numRectanglesY + degreeY + 1];
        _output = new Vector3[resolutionX, resolutionY];
        
        // SplineKnots(knotsX,numRectanglesX,degreeX);
        // SplineKnots(knotsY,numRectanglesY,degreeY);
        
        knotsX = InitializeKnotVector(knotsX,_numRectanglesX,degreeX);
        knotsY = InitializeKnotVector(knotsY,_numRectanglesY,degreeY);

        GenerateControlPoints();
        CalculateSplineSurface();
        DisplaySurface();
    }

    private void GenerateControlPoints()
    {
        for (int i = 0; i <= _numRectanglesX; i++)
        {
            for (int j = 0; j <= _numRectanglesY; j++)
            {
                _controlPoints[i, j] = new Vector3(i, Random.Range(-1f, 1f), j);
            }
        }
    }

    void CalculateSplineSurface()
    {
        var incrementI = (_numRectanglesX - degreeX + 2) / ((double)resolutionX - 1);
        var incrementJ = (_numRectanglesY - degreeY + 2) / ((double)resolutionY - 1);

        double intervalI = 0;
        for (int i = 0; i < resolutionX - 1; i++)
        {
            double intervalJ = 0;
            for (int j = 0; j < resolutionY - 1; j++)
            {
                _output[i, j] = Vector3.zero;

                for (int ki = 0; ki <= _numRectanglesX; ki++)
                {
                    for (int kj = 0; kj <= _numRectanglesY; kj++)
                    {
                        var bi = SplineBlend(ki, degreeX, knotsX, intervalI);
                        var bj = SplineBlend(kj, degreeY, knotsY, intervalJ);
                        _output[i, j] += _controlPoints[ki, kj] * (float)(bi * bj);
                    }
                }
                intervalJ += incrementJ;
            }
            intervalI += incrementI;
        }
    }

    void DisplaySurface()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < resolutionX; i++)
        {
            for (int j = 0; j < resolutionY; j++)
            {
                vertices.Add(_output[i, j]);
            }
        }

        for (int i = 0; i < resolutionX - 2; i++)
        {
            for (int j = 0; j < resolutionY - 2; j++)
            {
                int index = i * resolutionY + j;

                triangles.Add(index);
                triangles.Add(index + 1);
                triangles.Add(index + resolutionY);

                triangles.Add(index + resolutionY);
                triangles.Add(index + 1);
                triangles.Add(index + resolutionY + 1);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();


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
        
        _meshFilter.mesh = mesh;

        colors = new Color[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            float height = Mathf.InverseLerp(-1, 1, vertices[i].y);
            colors[i] = gradient.Evaluate(height);
        }
        mesh.colors = colors;
    }
    
    void SplineKnots(int[] u,int n,int t)
    {
        int j;

        for (j=0;j<=n+t;j++) 
        {
            if (j < t)
                u[j] = 0;
            else if (j <= n)
                u[j] = j - t + 1;
            else if (j > n)
                u[j] = n - t + 2;	
        }
    }

    int[] InitializeKnotVector(int[] knots, int numRects, int degree)
    {
        int index = 0;
        numRects++; // increment once to get number of controlpoints
        degree--;

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

    double SplineBlend(int k,int t, int[] u,double v)
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
                value = (u[k+t] - v) / (u[k+t] - u[k+1]) * SplineBlend(k+1,t-1,u,v);
            else if (u[k+t] == u[k+1])
                value = (v - u[k]) / (u[k+t-1] - u[k]) * SplineBlend(k,t-1,u,v);
            else
                value = (v - u[k]) / (u[k+t-1] - u[k]) * SplineBlend(k,t-1,u,v) + 
                        (u[k+t] - v) / (u[k+t] - u[k+1]) * SplineBlend(k+1,t-1,u,v);
        }
        return(value);
    }

    private void OnDrawGizmos()
    {
        if (_controlPoints == null) return;
        foreach (var point in _controlPoints)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(point, .04f);
        }
        Gizmos.color = Color.green;
        for (int i = 0; i <= _numRectanglesX; i++)
        {
            for (int j = 0; j < _numRectanglesY; j++)
            {
                // Up
                Gizmos.DrawLine(_controlPoints[i, j], _controlPoints[i, j + 1]);
            }
        }

        for (int i = 0; i < _numRectanglesX; i++)
        {
            for (int j = 0; j <= _numRectanglesY; j++)
            {
                // Right
                Gizmos.DrawLine(_controlPoints[i, j], _controlPoints[i + 1, j]);
            }
        }
    }
}