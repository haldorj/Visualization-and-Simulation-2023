using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class BSplineSurface : MonoBehaviour
{

    [SerializeField] private float[] knotsX; // u
    [SerializeField] private float[] knotsY; // v
    
    private static int numCols = 4; // Num of control points for knots X
    private static int numRows = 3; // Num of control points for knots Y
    private int _degree = 2; // Degree (same in both directions)

    private readonly Vector3[,] _controlPoints = new Vector3[numRows, numCols];
    

    private void Awake()
    {
        knotsX = new[]
        {
            0.0f , 0.0f , 0.0f , 1.0f , 2.0f , 2.0f , 2.0f
        };
        
        knotsY = new[]
        {
            0.0f , 0.0f , 0.0f , 1.0f , 1.0f , 1.0f
        };

        GenerateControlPoints();
    }
    
    Tuple <int , int> find_my(float tu , float tv )
    {
        throw new NotImplementedException();
    }
    
    // Tuple <Vector3 , Vector3> B2 (float tu , float tv , int my_u, int my_v)
    // {
    //     Vector3 Bv = default;
    //     Vector3 Bu = default;
    //     var w12 = ( tu - knotsX [my_u - 1]) / ( knotsX [my_u+1]=knotsX [my_u - 1] );
    //     var w22 = ( tu - knotsX [my_u]) / ( knotsX [my_u+2]=knotsX [my_u ] ) ;
    //     var w11 = ( tu - knotsX [my_u]) / ( knotsX [my_u+1]=knotsX [my_u ] ) ;
    //     
    //     return (Bu, Bv) ;
    // }
    //
    // Vector3 EvaluateSurface(float tu, float tv, int my_u, int my_v, Vector3 bu, Vector3 bv)
    // {
    //     var r = Vector3.zero;
    //     Vector3[,] w;
    //     for ( int j =0; j <3; j++) 
    //     {
    //         for ( int i =0; i <3; i++)
    //         {
    //             w[i][j] = _controlPoints[my_u - d_u + i, my_v - d_v + j] * bu[i] * bv[j] ;
    //             r = r + w[ i ] [ j ] ;
    //         }
    //     }
    //     return r ;
    // }

    
    void GenerateControlPoints()
    {
        float spacingX = 1.0f;
        float spacingY = 1.0f;

        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                float x = col * spacingX;
                float y = Random.Range(-0.5f, 0.5f);
                float z = row * spacingY;
                
                _controlPoints[row, col] = new Vector3(x, y, z);
            }
        }

        // Now, you can use the controlPoints array for your purposes.
        // For example, you can print the values to the console.
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                Debug.Log("Control Point [" + row + ", " + col + "]: " + _controlPoints[row, col]);
            }
        }
    }
    
    Vector3 Evaluate(float tu , float tv , int myU, int myV, Vector3 bu , Vector3 bv)
    {
        throw new NotImplementedException();
    }

    private void OnDrawGizmos()
    {
        if (_controlPoints.Length > 0)
        {
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(_controlPoints[row, col], 0.05f);
                }
            }
        }
    }
}
