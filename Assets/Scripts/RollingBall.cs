using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


public class RollingBall : MonoBehaviour
{
    private float _radius = 0.015f;
    public MeshGenerator meshGenerator;

    [FormerlySerializedAs("_currentVelocity")] [SerializeField] private Vector3 currentVelocity;
    private Vector3 _previousVelocity;
    
    [FormerlySerializedAs("_currentPos")] [SerializeField] private Vector3 currentPos;
    private Vector3 _previousPos;

    private int _currentTriangle;
    private int _previousTriangle;

    private Vector3 _currentNormal;
    private Vector3 _previousNormal;

    public float xStart = 0.06f;
    public float zStart = 0.03f;

    private void Start()
    {
        // Set initial height
        var yStart = meshGenerator.GetSurfaceHeight(new Vector2(xStart, zStart));
        currentPos = new Vector3(xStart, yStart + _radius, zStart);
        _previousPos = currentPos;

        transform.position = currentPos;
    }

    void FixedUpdate()
    {
        if (meshGenerator)
        {
            Move();
            Correction();
        }
    }
    
    void Move()
    {
        // Iterate through each triangle 
        for (int i = 0; i < meshGenerator.triangles.Length; i += 3)
        {
            // Find the vertices of the triangle
            Vector3 p0 = meshGenerator.vertices[meshGenerator.triangles[i]];
            Vector3 p1 = meshGenerator.vertices[meshGenerator.triangles[i + 1]];
            Vector3 p2 = meshGenerator.vertices[meshGenerator.triangles[i + 2]];

            // Find the balls position in the xz-plane
            Vector2 pos = new Vector2(currentPos.x, currentPos.z);
            
            // Find which triangle the ball is currently on with barycentric coordinates
            Vector3 baryCoords = MeshGenerator.BarycentricCoordinates(
                new Vector2(p0.x, p0.z), 
                new Vector2(p1.x, p1.z),  
                new Vector2(p2.x, p2.z),  
                pos
                );
            
            if (baryCoords is { x: >= 0.0f, y: >= 0.0f, z: >= 0.0f })
            {
                // Current triangle index
                _currentTriangle = i / 3;
                // Calculate normal vector
                _currentNormal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                // Calculate acceleration vector
                var acceleration = new Vector3(_currentNormal.x * _currentNormal.y, 
                    _currentNormal.y * _currentNormal.y - 1, 
                    _currentNormal.z * _currentNormal.y) * -Physics.gravity.y;
                // Update velocity
                currentVelocity = _previousVelocity + acceleration * Time.fixedDeltaTime;
                _previousVelocity = currentVelocity;
                // Update position
                currentPos = _previousPos + currentVelocity * Time.fixedDeltaTime;
                _previousPos = currentPos;
                transform.position = currentPos;

                if (_currentTriangle != _previousTriangle)
                {
                    // COLLISION: The ball is on a new triangle
                    
                    // Calculate the normal (n) of the collision plane
                    var n = (_previousNormal + _currentNormal).normalized;
                    
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
                _previousNormal = _currentNormal;
            }
        }
        if (currentPos.x < 0.0 || currentPos.x > 0.8 ||
            currentPos.z < 0.0 || currentPos.z > 0.4)
        {
            FreeFall();
        }
    }

    void Correction()
    {
        // Find the point on the ground directly under the center of the ball
        Vector3 p = new Vector3(currentPos.x, 
            meshGenerator.GetSurfaceHeight(new Vector2(currentPos.x, currentPos.z)), 
            currentPos.z);

        // Update position
        currentPos = p + _radius * _currentNormal;
        transform.position = currentPos;
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
