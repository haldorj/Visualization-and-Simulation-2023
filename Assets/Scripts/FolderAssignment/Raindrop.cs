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

    public GameObject ball;

    public Triangulation surface;

    private void Start()
    {
        currentPos = transform.position;
        _previousPos = transform.position;
    }

    void FixedUpdate()
    {
        FreeFall();
    }

    void IndianaJones()
    {
        var height = surface.GetSurfaceHeight(new Vector2(currentPos.x, currentPos.z));
        var pos = new Vector3(transform.position.x, height, transform.position.z);

        Instantiate(ball, pos, Quaternion.identity);
        
        Destroy(this);
    }

    void FreeFall()
    {
        if (currentPos.y <= surface.GetSurfaceHeight(new Vector2(currentPos.x, currentPos.z)))
        {
            IndianaJones();
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
