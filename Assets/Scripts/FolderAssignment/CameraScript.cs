using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraScript : MonoBehaviour
{
    private Vector3 _hitPoint;
    private Camera _cam;

    public PhysicsBall ball;
    private bool _shouldSpawn;

    public GameObject parentObj;
    
    private void Awake() 
    {
        _shouldSpawn = true;
        _cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && _shouldSpawn)
        {
            InstantiateBall();
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            _shouldSpawn = true;
        }
    }

    private void InstantiateBall() 
    {
        RaycastHit hit;

        if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out hit, 1000)) 
        {
            Triangulation hitTriangulation = hit.collider.gameObject.GetComponent<Triangulation>();

            _hitPoint = hit.point;

            print(_hitPoint.ToString());
            ball.surface = hitTriangulation;
            GameObject ballGameObject = Instantiate(ball.GameObject(), _hitPoint, Quaternion.identity);

            ballGameObject.transform.parent = parentObj.transform;
        }
        _shouldSpawn = false;
    }
}
