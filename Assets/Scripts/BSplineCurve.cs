using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BSplineCurve : MonoBehaviour
{
    [SerializeField] public List<Vector3> controlPoints = new(); // The controlPoints (c)
    [SerializeField] private float[] knots; // array storing knots (t)
    private const int Degree = 2; // The degree of the polynomial (d)

    public GameObject obj;

    private bool _forward;
    private float _tValue;

    private const float H = 0.05f;
    private const float Tmin = 0.0f;
    private const float Tmax = 3.0f;

    private void Awake()
    {
        knots = new float[]
        {
            0, 0, 0, 1, 2, 3, 3, 3
        };
    }

    private void Start()
    {
        // initial position
        obj.transform.position = controlPoints[0];
    }

    private void FixedUpdate()
    {
        MoveNPC(Time.fixedDeltaTime);
    }

    private Vector3 EvaluateBSplineSimple(float x)
    {
        int my = FindKnotinterval(x);

        Vector3[] a = new Vector3[Degree + 1];

        for (int j = 0; j <= Degree; j++)
        {
            a[Degree - j] = controlPoints[my - j];
        }

        for (int k = Degree; k > 0; k--)
        {
            int j = my - k;
            for (int i = 0; i < k; i++)
            {
                j++;
                float w = (x - knots[j]) / (knots[j + k] - knots[j]);
                a[i] = a[i] * (1 - w) + a[i + 1] * w;
            }
        }
        return a[0];
    }

    private int FindKnotinterval(float x)
    {
        int my = controlPoints.Count - 1; // index of last control point

        // Separate the conditions
        while (my >= 0 && my < knots.Length && x < knots[my] && my > Degree)
            my--;

        return my;
    }

    void MoveNPC(float dt)
    {
        Vector3 current;        // Current pos
        Vector3 next;           // Next pos
        Vector3 trajectory;     // Trajectory we will move in (Current - Next)
        
        if (_forward) // move forwards
        {
            if (_tValue < Tmax)
            {
                current = EvaluateBSplineSimple(_tValue);
                _tValue += dt;
                next = EvaluateBSplineSimple(_tValue);

                trajectory = next - current;

                obj.transform.position += trajectory;
                
            }
            if (_tValue >= Tmax)
            {
                // Correct position
                _tValue = Tmax;
                obj.transform.position = EvaluateBSplineSimple(Tmax);
                _forward = !_forward;
            }
        }
        else // move backwards
        {
            if (_tValue > Tmin)
            {
                current = EvaluateBSplineSimple(_tValue);
                _tValue -= dt;
                next = EvaluateBSplineSimple(_tValue);

                trajectory = next - current;
                
                obj.transform.position += trajectory;
            }
            if (_tValue <= Tmin)
            {
                // Correct position
                _tValue = Tmin;
                obj.transform.position = EvaluateBSplineSimple(Tmin);
                _forward = !_forward;
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        foreach (var point in controlPoints)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(point, 0.02f);
        }

        for (int i = 0; i < controlPoints.Capacity - 1; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(controlPoints[i], controlPoints[i + 1]);
        }


        if (knots.Length > 0)
        {
            var prev = EvaluateBSplineSimple(Tmin);
            for (var t = Tmin + H; t <= Tmax; t += H)
            {
                var current = EvaluateBSplineSimple(t);
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(prev, current);
                prev = current;
            }
        }
    }
}