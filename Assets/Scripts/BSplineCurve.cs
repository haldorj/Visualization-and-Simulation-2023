using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BSplineCurve : MonoBehaviour
{
    [SerializeField] public List<Vector3> controlPoints = new(); // The controlPoints (c)
    [SerializeField] private int degree = 2; // The degree of the polynomial (d)
    [SerializeField] private float[] knots; // Knot vector (t)

    public GameObject obj;

    private bool _forward;
    private float _npcMoveValue;

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

        Vector3[] a = new Vector3[degree + 1];

        for (int j = 0; j <= degree; j++)
        {
            a[degree - j] = controlPoints[my - j];
        }

        for (int k = degree; k > 0; k--)
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
        while (my >= 0 && my < knots.Length && x < knots[my] && my > degree)
            my--;

        return my;
    }

    void MoveNPC(float dt)
    {
        Vector3 current;        // Current pos
        Vector3 next;           // Next pos
        Vector3 trajectory;     // Trajectory we will move in (Current - Next)
        
        Debug.Log(_npcMoveValue);
        
        if (_forward) // move forwards
        {
            if (_npcMoveValue < 1) // while t < 1
            {
                current = EvaluateBSplineSimple(_npcMoveValue);
                _npcMoveValue += dt;
                next = EvaluateBSplineSimple(_npcMoveValue);

                trajectory = next - current;

                obj.transform.position = trajectory;
                
            }
            if (_npcMoveValue >= 1)
            {
                _npcMoveValue = 1;
                _forward = !_forward;
            }
        }
        else // move backwards
        {
            if (_npcMoveValue > 0) // while t > 0
            {
                current = EvaluateBSplineSimple(_npcMoveValue);
                _npcMoveValue -= dt;
                next = EvaluateBSplineSimple(_npcMoveValue);

                trajectory = next - current;
                
                obj.transform.position = trajectory;
            }
            if (_npcMoveValue <= 0)
            {
                _npcMoveValue = 0;
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
        
        float h = 0.05f, tmin = 0.0f, tmax = 1.0f;
        var prev = EvaluateBSplineSimple(tmin);
        for (var t = tmin + h; t <= tmax; t += h)
        {
            var current = EvaluateBSplineSimple(t);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(prev, current);
            prev = current;
        }
    }
}