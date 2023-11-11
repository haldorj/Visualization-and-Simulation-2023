using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class RaindropManager : MonoBehaviour
{
    [SerializeField] private GameObject RainDropPrefab;
    [SerializeField] private Vector2 SpawnSize = new (240,180);

    private void Start()
    {
        InvokeRepeating(nameof(SpawnRainDrop), 0.0f,0.2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 size = new Vector3(SpawnSize.x, 1, SpawnSize.y);
        Gizmos.DrawWireCube(transform.position, size*2);
    }

    Vector3 GetRandomRainDropPosition()
    {
        float x = Random.Range(-SpawnSize.x, SpawnSize.x);
        float z = Random.Range(-SpawnSize.y, SpawnSize.y);

        Vector3 position = transform.position;
        position.x += x;
        position.z += z;

        return position;
    }
    
    void SpawnRainDrop()
    {
        GameObject RainDropGameObject = Instantiate(RainDropPrefab, GetRandomRainDropPosition(), Quaternion.identity);
        RainDropGameObject.transform.parent = this.transform;
        
        Destroy(RainDropGameObject, 5);
    }
}
