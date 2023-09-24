using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


public class RollingBall : MonoBehaviour
{
    private float _radius = 0.015f;
    public TriangleSurface triangleSurface;

    [FormerlySerializedAs("_currentVelocity")] [SerializeField] private Vector3 currentVelocity;
    private Vector3 _previousVelocity;
    
    [FormerlySerializedAs("_currentPos")] [SerializeField] private Vector3 currentPos;
    private Vector3 _previousPos;
    
    private int _currentTriangle;
    private int _previousTriangle;

    [FormerlySerializedAs("_currentNormal")] [SerializeField] private Vector3 currentNormal;
    private Vector3 _previousNormal;

    public float xStart = 0.06f;
    public float zStart = 0.03f;

    [SerializeField] private float elapsedTime;
    
    private void Awake()
    {
        transform.localScale = Vector3.one * _radius * 2;
    }

    private void Start()
    {
        // Set initial height
        var yStart = triangleSurface.GetSurfaceHeight(new Vector2(xStart, zStart));
        currentPos = new Vector3(xStart, yStart + _radius, zStart);
        _previousPos = currentPos;

        transform.position = currentPos;
    }

    void FixedUpdate()
    {
        if (triangleSurface)
        {
            Move();
        }
    }
    
    void Move()
    {
        // Iterate through each triangle 
        for (int i = 0; i < triangleSurface.triangles.Length; i += 3)
        {
            // Find the vertices of the triangle
            Vector3 p0 = triangleSurface.vertices[triangleSurface.triangles[i]];
            Vector3 p1 = triangleSurface.vertices[triangleSurface.triangles[i + 1]];
            Vector3 p2 = triangleSurface.vertices[triangleSurface.triangles[i + 2]];

            // Find the balls position in the xz-plane
            Vector2 pos = new Vector2(currentPos.x, currentPos.z);
            
            // Find which triangle the ball is currently on with barycentric coordinates
            Vector3 baryCoords = TriangleSurface.BarycentricCoordinates(
                new Vector2(p0.x, p0.z), 
                new Vector2(p1.x, p1.z),  
                new Vector2(p2.x, p2.z),  
                pos
                );
            
            if (baryCoords is { x: >= 0.0f, y: >= 0.0f, z: >= 0.0f })
            {
                elapsedTime += Time.fixedDeltaTime;
                // Current triangle index
                _currentTriangle = i / 3;
                // Calculate normal vector
                currentNormal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                // Calculate acceleration vector
                var acceleration = new Vector3(currentNormal.x * currentNormal.y, 
                    currentNormal.y * currentNormal.y - 1, 
                    currentNormal.z * currentNormal.y) * -Physics.gravity.y;
                Debug.Log(acceleration.magnitude);
                // Update velocity
                currentVelocity = _previousVelocity + acceleration * Time.fixedDeltaTime;
                _previousVelocity = currentVelocity;
                Debug.Log(currentVelocity.magnitude);
                // Update position
                currentPos = _previousPos + currentVelocity * Time.fixedDeltaTime;
                _previousPos = currentPos;
                transform.position = currentPos;

                if (_currentTriangle != _previousTriangle)
                {
                    // COLLISION: The ball is on a new triangle
                    
                    // Calculate the normal (n) of the collision plane
                    var n = (_previousNormal + currentNormal).normalized;
                    
                    // Update the velocity vector r = v − 2(v · n)n
                    var velocityAfter = _previousVelocity - 2 * Vector3.Dot(_previousVelocity, n) * n;
                    
                    currentVelocity = velocityAfter + acceleration * Time.fixedDeltaTime;
                    _previousVelocity = currentVelocity;
                    
                    // Update the position in the direction of the new velocity vector
                    currentPos = _previousPos + currentVelocity * Time.fixedDeltaTime;
                    transform.position = currentPos;
                    _previousPos = currentPos;
                }
                
                // Update triangle index and normal
                _previousTriangle = _currentTriangle;
                _previousNormal = currentNormal;
            }
        }
        //Basic area check to verify that the ball is on the plane
        if (currentPos.x < 0.0 || currentPos.x > 0.8 ||
            currentPos.z < 0.0 || currentPos.z > 0.4)
        {
            FreeFall();
        }
        else
        {
            Correction();
        }
    }

    void Correction()
    {
        // Find the point on the ground directly under the center of the ball
        var point = new Vector3(currentPos.x, 
            triangleSurface.GetSurfaceHeight(new Vector2(currentPos.x, currentPos.z)), 
            currentPos.z);
        
        // if the edge of the ball is under the plane
        if (currentPos.y -_radius < point.y)
        {
            // Update position (distance of radius in the direction of the normal)
            currentPos = point + _radius * currentNormal;
            transform.position = currentPos;
        }
    }

    void FreeFall()
    {
        if (currentPos.y > -3)
        {
            // Update velocity
            currentVelocity = _previousVelocity + Physics.gravity * Time.fixedDeltaTime;
            _previousVelocity = currentVelocity;
            // Update position
            currentPos = _previousPos + currentVelocity * Time.fixedDeltaTime;
            _previousPos = currentPos;

            transform.position = currentPos;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, _radius + 0.001f);
    }
}
