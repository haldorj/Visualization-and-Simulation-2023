using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raindrop : MonoBehaviour
{
    [SerializeField] private Vector3 currentVelocity;
    private Vector3 _previousVelocity = Vector3.zero;
    
    [SerializeField] private Vector3 currentPos;
    private Vector3 _previousPos;
    
    public Triangulation surface;

    public PhysicsBall ball;
    
    private Renderer _meshRenderer;
    
    private void Start()
    {
        currentPos = transform.position;
        _previousPos = transform.position;
        
        _meshRenderer = GetComponent<Renderer>();
    }

    void FixedUpdate()
    {
        FreeFall();
        
    }

    private void Swap()
    {
        // Turn off rendering
        if (_meshRenderer)
            _meshRenderer.enabled = false;
        
        var height = surface.GetSurfaceHeight(new Vector2(currentPos.x, currentPos.z));
        var pos = new Vector3(transform.position.x, height, transform.position.z);
        ball.surface = surface;

        Instantiate(ball, pos, Quaternion.identity);
        
        
        Destroy(this);
    }

    void FreeFall()
    {
        if (currentPos.y <= surface.GetSurfaceHeight(new Vector2(currentPos.x, currentPos.z)))
        {
            Swap();
        }
        
        // Update velocity
        currentVelocity = _previousVelocity + Physics.gravity * Time.fixedDeltaTime;
        _previousVelocity = currentVelocity;
        // Update position
        currentPos = _previousPos + currentVelocity * Time.fixedDeltaTime;
        _previousPos = currentPos;

        transform.position = currentPos;
    }
}
