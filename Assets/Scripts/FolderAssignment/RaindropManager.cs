using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class RaindropManager : MonoBehaviour
{
    //[SerializeField] private GameObject RainDropPrefab;
    [SerializeField] private Vector2 SpawnSize = new (240,180);
    [SerializeField] private Raindrop RainDrop;

    public Triangulation surface;
    
    private bool _raining;
    public float spawnRate = 1f;
    private float _spawnRate;
    
    private void Start()
    {
        _spawnRate = spawnRate;
        //InvokeRepeating(nameof(SpawnRainDrop), 0.0f,0.8f);
    }

    private void FixedUpdate()
    {
        _spawnRate -= Time.fixedDeltaTime;
        if (_raining && _spawnRate <= 0)
        {
            SpawnRainDrop();
            _spawnRate = spawnRate;
        }
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
        RainDrop.surface = surface;
        GameObject RainDropGameObject = Instantiate(RainDrop.GameObject(), GetRandomRainDropPosition(), Quaternion.identity);
        RainDropGameObject.transform.parent = this.transform;
        
        Destroy(RainDropGameObject, 5);
    }

    public void StartRain()
    {
        _raining = true;
    }

    public void StopRain()
    {
        _raining = false;
    }
}
