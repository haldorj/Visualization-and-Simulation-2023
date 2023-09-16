using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class RollingBall : MonoBehaviour
{
    private float _radius = 0.015f;
    public MeshGenerator meshGenerator;

    public float mass = 1.0f;
    
    private Vector3 _currentVelocity;
    private Vector3 _previousVelocity;
    
    private Vector3 _currentPos;
    private Vector3 _previousPos;

    private void Awake()
    {
        _currentPos = new Vector3(0.0f, 0.097f + _radius, 0.0f);
        _previousPos = _currentPos;
    }

    void FixedUpdate()
    {
        
        if (meshGenerator)
            Move();
        else
        {
            FreeFall();
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
            Vector2 pos = new Vector2(_currentPos.x, _currentPos.z);
            
            // Find which triangle the ball is currently on with barycentric coordinates
            Vector3 baryCoords = MeshGenerator.BarycentricCoordinates(
                new Vector2(p0.x, p0.z), 
                new Vector2(p1.x, p1.z),  
                new Vector2(p2.x, p2.z),  
                pos
                );
            
            if (baryCoords is { x: >= 0.0f, y: >= 0.0f, z: >= 0.0f })
            {
                // Calculate normal vector
                Vector3 n = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                // Calculate acceleration vector
                Vector3 acceleration = new Vector3(n.x * n.y, n.y * n.y - 1, n.z * n.y) * -Physics.gravity.y;
                
                // Update velocity
                _currentVelocity = _previousVelocity + acceleration * Time.fixedDeltaTime;
                _previousVelocity = _currentVelocity;
                // Update position
                _currentPos = _previousPos + _currentVelocity * Time.fixedDeltaTime;
                _previousPos = _currentPos;
                
                transform.position = _currentPos;
                
                Debug.Log(_currentPos);
            }
            else
            {
                FreeFall();
            }
        }
    }

    void FreeFall()
    {
        // Update velocity
        _currentVelocity = _previousVelocity + Physics.gravity * Time.fixedDeltaTime;
        _previousVelocity = _currentVelocity;
        // Update position
        _currentPos = _previousPos + _currentVelocity * Time.fixedDeltaTime;
        _previousPos = _currentPos;
                
        transform.position = _currentPos;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, _radius);
    }
}
