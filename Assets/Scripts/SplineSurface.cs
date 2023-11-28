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
    public int numControlPointsX = 3;   //ni
    public int numControlPointsY = 4;   //nj
    private Vector3[,] _controlPoints;
    public int degreeX = 3;             //ti
    public int degreeY = 3;             //tj
    public int[] knotsX;
    public int[] knotsY;
    public int resolutionX = 30;
    public int resolutionY = 40;
    private Vector3[,] _output;

    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;

    private void Start()
    {
        _controlPoints = new Vector3[numControlPointsX + 1, numControlPointsY + 1];
        knotsX = new int[numControlPointsX + degreeX + 1];
        knotsY = new int[numControlPointsY + degreeY + 1];
        _output = new Vector3[resolutionX, resolutionY];
        
        SplineKnots(knotsX,numControlPointsX,degreeX);
        SplineKnots(knotsY,numControlPointsY,degreeY);

        GenerateControlPoints();
        CalculateSplineSurface();
        DisplaySurface();
    }

    private void GenerateControlPoints()
    {
        for (int i = 0; i <= numControlPointsX; i++)
        {
            for (int j = 0; j <= numControlPointsY; j++)
            {
                _controlPoints[i, j] = new Vector3(i, Random.Range(-1f, 1f), j);
            }
        }
    }

    void CalculateSplineSurface()
    {
        double intervalI, incrementI;
        double intervalJ, incrementJ;
        double bi, bj;

        incrementI = (numControlPointsX - degreeX + 2) / ((double)resolutionX - 1);
        incrementJ = (numControlPointsY - degreeY + 2) / ((double)resolutionY - 1);

        // Your spline surface calculation logic goes here

        intervalI = 0;
        for (int i = 0; i < resolutionX - 1; i++)
        {
            intervalJ = 0;
            for (int j = 0; j < resolutionY - 1; j++)
            {
                _output[i, j] = Vector3.zero;

                for (int ki = 0; ki <= numControlPointsX; ki++)
                {
                    for (int kj = 0; kj <= numControlPointsY; kj++)
                    {
                        bi = SplineBlend(ki, degreeX, knotsX, intervalI);
                        bj = SplineBlend(kj, degreeY, knotsY, intervalJ);
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

        // You can add more settings to the mesh, such as normals and UV coordinates, if needed.

        // Create or get a MeshRenderer component attached to the GameObject
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null)
        {
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Create or get a MeshFilter component attached to the GameObject
        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null)
        {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        // Assign the mesh to the MeshFilter
        _meshFilter.mesh = mesh;

        // Optionally, you can set materials, shaders, or other properties for rendering
        // meshRenderer.material = yourMaterial;
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
        foreach (var point in _controlPoints)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(point, .04f);
        }
        Gizmos.color = Color.green;
        for (int i = 0; i <= numControlPointsX; i++)
        {
            for (int j = 0; j < numControlPointsY; j++)
            {
                // Up
                Gizmos.DrawLine(_controlPoints[i, j], _controlPoints[i, j + 1]);
            }
        }

        for (int i = 0; i < numControlPointsX; i++)
        {
            for (int j = 0; j <= numControlPointsY; j++)
            {
                // Right
                Gizmos.DrawLine(_controlPoints[i, j], _controlPoints[i + 1, j]);
            }
        }
    }
}