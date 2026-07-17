using System;
using UnityEngine;

public class HoneyDropper : MonoBehaviour
{
    public Vector3 spawnPosition = Vector3.down;
    
    public float spawnTime = 0.5f;
    
    float spawnTimer = 0;
    
    public GameObject honeyObject;

    [SerializeField] private float speed = 4f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnTime)
        {
            var blob = Instantiate(honeyObject, transform.TransformPoint( spawnPosition), Quaternion.identity);
            blob.transform.parent = transform;
            spawnTimer = 0;
            
        }
    }

    
}
