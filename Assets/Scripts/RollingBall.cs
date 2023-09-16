using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class RollingBall : MonoBehaviour
{
    private float _radius = 0.015f;
    public MeshGenerator meshGenerator;
    
    private Vector3 _currentVelocity;
    private Vector3 _previousVelocity;
    
    private Vector3 _currentPos;
    private Vector3 _previousPos;

    private int _currentTriangle;
    private int _previousTriangle;
    
    private Vector3 _currentNormal;
    private Vector3 _previousNormal;
    
    public float xStart = 0.06f;
    public float zStart = 0.03f;

    private void Start()
    {
        _currentPos = new Vector3(xStart, 
            0.095f + _radius, 
            zStart);
        
        //Debug.Log(meshGenerator.GetSurfaceHeight(new Vector2(xStart,zStart)));
        
        _previousPos = _currentPos;
    }

    void FixedUpdate()
    {
        
        if (meshGenerator)
            Move();
        else
            FreeFall();
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
                _currentTriangle = i / 3;
                // Calculate normal vector
                _currentNormal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                // Calculate acceleration vector
                var acceleration = new Vector3(_currentNormal.x * _currentNormal.y, 
                    _currentNormal.y * _currentNormal.y - 1, 
                    _currentNormal.z * _currentNormal.y) * -Physics.gravity.y;
                // Update velocity
                _currentVelocity = _previousVelocity + acceleration * Time.fixedDeltaTime;
                _previousVelocity = _currentVelocity;
                // Update position
                _currentPos = _previousPos + _currentVelocity * Time.fixedDeltaTime;
                _previousPos = _currentPos;
                transform.position = _currentPos;

                if (_currentTriangle != _previousTriangle)
                {
                    // The ball is on a new triangle
                    
                    // Calculate the normal of the collision plane
                    var x = (_previousNormal + _currentNormal) / (_previousNormal + _currentNormal).magnitude;
                    // Correct the position upwards in the direction of the normal (x)
                    
                    // Update the velocity vector r = d − 2(d * n)n
                    var velocityAfter = _currentVelocity - 2 * Vector3.Dot(_currentVelocity, x.normalized) * x.normalized;
                    _currentVelocity = velocityAfter + acceleration * Time.fixedDeltaTime;
                    _previousVelocity = _currentVelocity;
                    
                    // Update the position in the direction of the new velocity vector
                    _currentPos = _previousPos + _currentVelocity * Time.fixedDeltaTime;
                    _previousPos = _currentPos;
                    transform.position = _currentPos;
                }

                _previousTriangle = _currentTriangle;
                _previousNormal = _currentNormal;
            }
            // if (_currentPos.x < 0.0 || _currentPos.x > 0.8 ||
            //     _currentPos.z < 0.0 || _currentPos.z > 0.4)
            // {
            //     FreeFall();
            // }
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
        Gizmos.DrawSphere(transform.position, _radius + 0.01f);
    }
}
