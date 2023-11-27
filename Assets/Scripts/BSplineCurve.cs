using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class BSplineCurve : MonoBehaviour
{
    [SerializeField] public List<Vector3> controlPoints = new(); // The controlPoints (c) add these in editor.
    [SerializeField] private float[] knots; // array storing knots (t)
    private const int Degree = 2; // The degree of the polynomial (d)

    public GameObject obj;

    [SerializeField] private bool forward;
    [FormerlySerializedAs("_tValue")] [SerializeField]private float tValue;

    private const float H = 0.1f;
    private float _tmin = 0.0f;
    private float _tmax;

    public float yOffset;

    public bool isNpc;

    [SerializeField]private List<Vector3> positions;
    [SerializeField]private List<float> tValues = new();

    private const float SphereRadius = 3f;
    public Material ballMaterial;

    private void Awake()
    {
        if (controlPoints != null)
        {
            InitializeKnotVector();
            GeneratePositions();
        }

        // initial position
        obj = CreateObj();
        obj.SetActive(false);
        if (obj)
        {
            obj.transform.position = controlPoints[0];
            obj.transform.position += new Vector3(0f, yOffset, 0f);
        }

        forward = true;
    }

    private void FixedUpdate()
    {
        if (obj && obj.activeSelf)
            MoveNpc(Time.fixedDeltaTime);

        AreaCheck();
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
        
        while (my >= 0 && my < knots.Length && x < knots[my] && my > Degree)
            my--;

        return my;
    }

    public void GeneratePositions()
    {
        for (int i = 0; i <= controlPoints.Count - Degree; i++)
        {
            positions.Add(EvaluateBSplineSimple(i));
            tValues.Add((float)i);
        }
    }

    void AreaCheck()
    {
        PhysicsBall[] physicsBalls = FindObjectsOfType<PhysicsBall>();

        foreach (var ball in physicsBalls)
        {
            if (ball.isRain) return;
            for (int i = 0; i < positions.Count; i++)
            {
                float distance = Vector3.Distance(positions[i], ball.transform.position);
                if (distance <= 7)
                {
                    obj.SetActive(true);
                    obj.transform.position = positions[i];
                    obj.transform.localPosition += Vector3.up * SphereRadius;
                    tValue = tValues[i];
                    
                    ball.GameObject().SetActive(false);
                    Destroy(ball);
                }
            }
        }
    }

    GameObject CreateObj()
    {
        
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = Vector3.one * (SphereRadius * 2);
        sphere.GetComponent<Renderer>().material = ballMaterial;

        obj = sphere;
        yOffset = SphereRadius;

        sphere.transform.parent = this.transform;
        
        return sphere;
    }


    void MoveNpc(float dt)
    {
        Vector3 current;        // Current pos
        Vector3 next;           // Next pos
        Vector3 trajectory;     // Trajectory we will move in (Current - Next)
        
        if (forward) // move forwards
        {
            if (tValue < _tmax)
            {
                current = EvaluateBSplineSimple(tValue);
                tValue += dt;
                next = EvaluateBSplineSimple(tValue);

                trajectory = next - current;

                if (!isNpc)
                {
                    // "Floating" effect
                    const float speed = 5;
                    const float amplitude = 0.07f;
                    obj.transform.position += trajectory + Vector3.up * (Mathf.Sin(tValue * speed) * amplitude);
                }
                else
                {
                    obj.transform.position += trajectory;
                }
                    
            }

            if (!(tValue >= _tmax)) return;
            // Correct position
            if (!isNpc) return;
            tValue = _tmax;
            obj.transform.position = EvaluateBSplineSimple(_tmax);
            forward = !forward;
        }
        else // move backwards
        {
            if (!isNpc) return;
            
            if (tValue > _tmin)
            {
                current = EvaluateBSplineSimple(tValue);
                tValue -= dt;
                next = EvaluateBSplineSimple(tValue);

                trajectory = next - current;
                
                obj.transform.position += trajectory;
            }

            if (!(tValue <= _tmin)) return;
            
            // Correct position
            tValue = _tmin;
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
        
        foreach (var point in positions)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(point, 1f);
        }
    }
}