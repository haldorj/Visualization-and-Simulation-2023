using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicsBall : MonoBehaviour
{
    [SerializeField]private float radius = 0.015f;
    public Triangulation surface;
    
    [SerializeField] private Vector3 accelerationVector;
    [SerializeField] private float acceleration;
    
    [SerializeField] private Vector3 currentVelocity;
    private Vector3 _previousVelocity;
    
    [SerializeField] private Vector3 currentPos;
    private Vector3 _previousPos;
    
    [SerializeField]private int currentTriangle;
    private int _previousTriangle;

    [SerializeField] private Vector3 currentNormal;
    private Vector3 _previousNormal;

    public float xStart = 0.06f;
    public float zStart = 0.03f;

    [SerializeField] private bool moving = true;
    [SerializeField] private float elapsedTime;
    
    private float _minX = float.MaxValue;
    private float _minZ = float.MaxValue;
    private float _maxX = float.MinValue;
    private float _maxZ = float.MinValue;

    private float _timer = 1f;
    public bool isRain;
    [SerializeField] private bool splineExists;
    [SerializeField] private List<Vector3> controlPoints;

    [SerializeField] private BSplineCurve spline;

    private void Awake()
    {
        radius = transform.localScale.z / 2;

        xStart = transform.position.x;
        zStart = transform.position.z;
    }

    private void Start()
    {
        // Set initial height
        var yStart = surface.GetSurfaceHeight(new Vector2(xStart, zStart));
        currentPos = new Vector3(xStart, yStart + radius, zStart);
        _previousPos = currentPos;
        transform.position = currentPos;

        FindExtremeValues();

        splineExists = false;
    }

    private void FixedUpdate()
    {
        if (surface && moving)
        {
            Move();
            
            _timer += Time.fixedDeltaTime;

            if (_timer >= 1f)
            {
                controlPoints.Add(currentPos + Vector3.up*radius);
                _timer = 0;
            }
        }
        
        if (!moving && !splineExists && isRain)
        {
            controlPoints.Add(currentPos + Vector3.up * radius);
            GenerateBSpline();
        }
        
        if (!moving)
            Destroy(this);
    }

    private void GenerateBSpline()
    {
        spline.controlPoints = controlPoints;
        GameObject SplineGameObject = Instantiate(spline.GameObject(), Vector3.zero, Quaternion.identity);
        SplineGameObject.transform.parent = this.transform;
        
        splineExists = true;
    }
    
    private void Move()
    {
        // AREA CHECK
        if (currentPos.x < _minX || currentPos.x > _maxX ||
            currentPos.z < _minZ || currentPos.z > _maxZ)
        {
            moving = false;
        }

        // Iterate through each triangle 
        for (int i = 0; i < surface.triangles.Length; i += 3)
        {
            // Find the vertices of the triangle
            Vector3 p0 = surface.vertices[surface.triangles[i]];
            Vector3 p1 = surface.vertices[surface.triangles[i + 1]];
            Vector3 p2 = surface.vertices[surface.triangles[i + 2]];

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
                currentTriangle = i / 3;
                // Calculate normal vector

                currentNormal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                // Calculate acceleration vector
                accelerationVector = new Vector3(currentNormal.x * currentNormal.y, 
                    currentNormal.y * currentNormal.y - 1, 
                    currentNormal.z * currentNormal.y) * -Physics.gravity.y;
                acceleration = accelerationVector.magnitude;
                // Update velocity
                currentVelocity = _previousVelocity + accelerationVector * Time.fixedDeltaTime;
                _previousVelocity = currentVelocity;

                //Debug.Log("Velocity: " + currentVelocity.magnitude);
                
                // Update position
                currentPos = _previousPos + currentVelocity * Time.fixedDeltaTime;
                _previousPos = currentPos;
                transform.position = currentPos;

                //float distanceTraveled = (new Vector3(_previousPos.x, _previousPos.y-radius, _previousPos.z) - start).magnitude;

                if (currentTriangle != _previousTriangle)
                {
                    // COLLISION: The ball is on a new triangle
                    
                    // Calculate the normal (n) of the collision plane
                    var n = (_previousNormal + currentNormal).normalized;
                    Correction(n);
                    
                    // Update the velocity vector r = v − 2(v · n)n
                    var velocityAfter = _previousVelocity - 2 * Vector3.Dot(_previousVelocity, n) * n;
                    
                    currentVelocity = velocityAfter + accelerationVector * Time.fixedDeltaTime;
                    _previousVelocity = currentVelocity;
                    
                    // Update the position in the direction of the new velocity vector
                    currentPos = _previousPos + currentVelocity * Time.fixedDeltaTime;
                    transform.position = currentPos;
                    _previousPos = currentPos;
                }
                
                // Update triangle index and normal
                _previousTriangle = currentTriangle;
                _previousNormal = currentNormal;
            }
        }
        Correction(Vector3.up);
    }
    
    void Move2()
    {
        int cellSize = surface.cellSize;

        // Find the cell indices for the current position
        int cellX = Mathf.FloorToInt((currentPos.x - _minX) / cellSize);
        int cellZ = Mathf.FloorToInt((currentPos.z - _minZ) / cellSize);

        // Iterate through nearby cells (adjust the range based on your needs)
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
            {
                int checkCellX = cellX + offsetX;
                int checkCellZ = cellZ + offsetZ;

                // Check if the cell indices are valid
                if (checkCellX >= 0 && checkCellX < surface.numCellsX &&
                    checkCellZ >= 0 && checkCellZ < surface.numCellsZ)
                {
                    // Iterate through triangles in the current cell
                    int cellIndex = checkCellX + checkCellZ * surface.numCellsX;

                    Debug.Log(surface.Grid[cellIndex].Count);
                    
                    for (int i = 0; i < surface.Grid[cellIndex].Count; i += 3)
                    {
                        // Get the triangle indices for the current cell
                        int triangleIndex0 = surface.Grid[cellIndex][i];
                        int triangleIndex1 = surface.Grid[cellIndex][i + 1];
                        int triangleIndex2 = surface.Grid[cellIndex][i + 2];

                        // Find the vertices of the triangle
                        Vector3 p0 = surface.vertices[surface.triangles[triangleIndex0]];
                        Vector3 p1 = surface.vertices[surface.triangles[triangleIndex1]];
                        Vector3 p2 = surface.vertices[surface.triangles[triangleIndex2]];
                        
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
                            currentTriangle = i / 3;
                            // Calculate normal vector

                            currentNormal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                            // Calculate acceleration vector
                            accelerationVector = new Vector3(currentNormal.x * currentNormal.y, 
                                currentNormal.y * currentNormal.y - 1, 
                                currentNormal.z * currentNormal.y) * -Physics.gravity.y;
                            acceleration = accelerationVector.magnitude;
                            // Update velocity
                            currentVelocity = _previousVelocity + accelerationVector * Time.fixedDeltaTime;
                            _previousVelocity = currentVelocity;

                            //Debug.Log("Velocity: " + currentVelocity.magnitude);
                            
                            // Update position
                            currentPos = _previousPos + currentVelocity * Time.fixedDeltaTime;
                            _previousPos = currentPos;
                            transform.position = currentPos;

                            //float distanceTraveled = (new Vector3(_previousPos.x, _previousPos.y-radius, _previousPos.z) - start).magnitude;

                            if (currentTriangle != _previousTriangle)
                            {
                                // COLLISION: The ball is on a new triangle
                                
                                // Calculate the normal (n) of the collision plane
                                var n = (_previousNormal + currentNormal).normalized;
                                
                                // Update the velocity vector r = v − 2(v · n)n
                                var velocityAfter = _previousVelocity - 2 * Vector3.Dot(_previousVelocity, n) * n;
                                
                                currentVelocity = velocityAfter + accelerationVector * Time.fixedDeltaTime;
                                _previousVelocity = currentVelocity;
                                
                                // Update the position in the direction of the new velocity vector
                                currentPos = _previousPos + currentVelocity * Time.fixedDeltaTime;
                                transform.position = currentPos;
                                _previousPos = currentPos;
                            }
                            
                            // Update triangle index and normal
                            _previousTriangle = currentTriangle;
                            _previousNormal = currentNormal;
                        }
                    }
                }
            }
        }
    }
    

    private void Correction(Vector3 normal)
    {
        // Find the point on the ground directly under the center of the ball
        var point = new Vector3(currentPos.x, 
            surface.GetSurfaceHeight(new Vector2(currentPos.x, currentPos.z)), 
            currentPos.z);
        
        // if the edge of the ball is under the plane
        if (currentPos.y - radius < point.y)
        {
            // Update position (distance of radius in the direction of the normal)
            currentPos = point + radius * normal;
            transform.position = currentPos;
        }
    }
    
    private void FreeFall()
    {
        if (currentPos.y > -20)
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
    
    private void FindExtremeValues()
    {
        if (surface.vertices == null || surface.vertices.Length == 0) return;

        foreach (var vertex in surface.vertices)
        {
            _minX = Mathf.Min(_minX, vertex.x);
            _minZ = Mathf.Min(_minZ, vertex.z);
            _maxX = Mathf.Max(_maxX, vertex.x);
            _maxZ = Mathf.Max(_maxZ, vertex.z);
        }
    }
}
