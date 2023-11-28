using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class BSplineSurface : MonoBehaviour
{

    [SerializeField] private float[] knotsX; // u
    [SerializeField] private float[] knotsY; // v
    
    private const int NumControlPointsX = 3; // Num of control points for knots X
    private const int NumControlPointsY = 3; // Num of control points for knots Y
    
    [SerializeField] private int degree = 2; // Degree (same in both directions)

    private Vector3[,] _controlPoints;

    private void Awake()
    {
        if (NumControlPointsX > degree && NumControlPointsY > degree)
        {
            knotsX = InitializeKnotVector(NumControlPointsX);
            knotsY = InitializeKnotVector(NumControlPointsY);
            
            _controlPoints = new Vector3[NumControlPointsX, NumControlPointsY];
        
            GenerateControlPoints();
        }
        else
        {
            print("Not enough control points!");
        }
    }

    float[] InitializeKnotVector(int numControlPoints)
    {
        int index = 0;

        List<float> k = new List<float>(); // knots

        for (int i = 0; i <= degree; i++)
        {
            k.Add(index);
        }
        index++;
        // c - d - 1
        if (numControlPoints - degree - 1 > 0)
        {
            for (int i = 0; i <= numControlPoints - degree - 1; i++)
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


    
    void GenerateControlPoints()
    {
        float spacingX = 1.0f;
        float spacingY = 1.0f;

        Debug.Log("x " + NumControlPointsX);
        Debug.Log("y " + NumControlPointsY);
        Debug.Log(_controlPoints.Length);

        for (int row = 0; row < NumControlPointsX; row++)
        {
            for (int col = 0; col < NumControlPointsY; col++)
            {
                float x = row * spacingX;
                float y = Random.Range(-0.5f, 0.5f);
                float z = col * spacingY;

                _controlPoints[row, col] = new Vector3(x, y, z);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (_controlPoints.Length > 0 || _controlPoints != null)
        {
            for (int row = 0; row < NumControlPointsX; row++)
            {
                for (int col = 0; col < NumControlPointsY; col++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(_controlPoints[row, col], 0.05f);
                }
            }
        }
    }
}
