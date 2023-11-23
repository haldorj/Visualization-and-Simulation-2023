using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BSplineCurve : MonoBehaviour
{
    [SerializeField] public List<Vector3> controlPoints = new(); // The controlPoints (c) add these in editor.
    [SerializeField] private float[] knots; // array storing knots (t)
    private const int Degree = 2; // The degree of the polynomial (d)

    public GameObject obj;

    [SerializeField]private bool forward;
    private float _tValue;

    private const float H = 0.05f;
    private float _tmin = 0.0f;
    private float _tmax;

    public float yOffset;

    public bool isNpc;

    private void Awake()
    {
        if (controlPoints != null)
            InitializeKnotVector();
        
        // initial position
        if (obj)
        {
            obj.transform.position = controlPoints[0];
            obj.transform.position += new Vector3(0f, yOffset, 0f);
        }
        
        forward = true;
    }

    private void FixedUpdate()
    {
        if (obj)
            MoveNpc(Time.fixedDeltaTime);
    }
    
    public void InitializeKnotVector()
    {
        if (Degree == 2)
        {
            int index = 0;
            
            List<float> k = new List<float>(); // knots

            for (int i = 0; i <= Degree; i++)
            {
                k.Add(index);
            }

            // c - d - 1
            for (int i = 0; i <= controlPoints.Count - Degree - 1; i++)
            {
                index++;
                k.Add(index);
            }
            
            for (int i = 0; i <= Degree - 1; i++)
            {
                k.Add(index);
            }

            knots = k.ToArray();
            _tmax = k.Last();

            // knots = new float[]
            // {
            //     0, 0, 0, 1, 2, 3, 4, 4, 4 //2nd
            // }
            //
            //_tmax = 4.0f;
        }

        if (Degree == 3)
        {
            knots = new float[]
            {
                0, 0, 0, 0, 1, 2, 3, 3, 3, 3 //3rd
            };
            _tmax = 3.0f;
        }
    }

    private Vector3 EvaluateBSplineSimple(float x)
    {
        int my = FindKnotInterval(x);

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

    private int FindKnotInterval(float x)
    {
        int my = controlPoints.Count - 1; // index of last control point

        // Separate the conditions
        while (my >= 0 && my < knots.Length && x < knots[my] && my > Degree)
            my--;

        return my;
    }

    void MoveNpc(float dt)
    {
        Vector3 current;        // Current pos
        Vector3 next;           // Next pos
        Vector3 trajectory;     // Trajectory we will move in (Current - Next)
        
        if (forward) // move forwards
        {
            if (_tValue < _tmax)
            {
                current = EvaluateBSplineSimple(_tValue);
                _tValue += dt;
                next = EvaluateBSplineSimple(_tValue);

                trajectory = next - current;

                if (!isNpc)
                {
                    // "Floating" effect
                    obj.transform.position += trajectory + new Vector3(0, Mathf.Sin(_tValue * 5) * 0.07f, 0);
                }
                else
                {
                    obj.transform.position += trajectory;
                }
                    
            }

            if (!(_tValue >= _tmax)) return;
            // Correct position
            if (!isNpc) return;
            _tValue = _tmax;
            obj.transform.position = EvaluateBSplineSimple(_tmax);
            forward = !forward;
        }
        else // move backwards
        {
            if (!isNpc) return;
            
            if (_tValue > _tmin)
            {
                current = EvaluateBSplineSimple(_tValue);
                _tValue -= dt;
                next = EvaluateBSplineSimple(_tValue);

                trajectory = next - current;
                
                obj.transform.position += trajectory;
            }

            if (!(_tValue <= _tmin)) return;
            
            // Correct position
            _tValue = _tmin;
            obj.transform.position = EvaluateBSplineSimple(_tmin);
            forward = !forward;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (knots.Length <= 0 || controlPoints.Count <= 0) return;
        
        // foreach (var point in controlPoints)
        // {
        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawSphere(point, 0.02f);
        // }
        //
        // for (int i = 0; i < controlPoints.Capacity - 1; i++)
        // {
        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawLine(controlPoints[i], controlPoints[i + 1]);
        // }
        
        var prev = EvaluateBSplineSimple(_tmin);
        for (var t = _tmin + H; t <= _tmax; t += H)
        {
            var current = EvaluateBSplineSimple(t);
            Gizmos.color = Color.blue;
            
            Gizmos.DrawLine(prev, current);
            prev = current;
        }
    }
}