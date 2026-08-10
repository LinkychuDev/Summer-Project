using System;
using UnityEngine;

public class GravityTest : MonoBehaviour
{
    private Rigidbody cachedBody => GetComponent<Rigidbody>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        cachedBody.AddForce(PlayerReferenceManager.instance.gravity * transform.up, ForceMode.Acceleration);
    }
}
