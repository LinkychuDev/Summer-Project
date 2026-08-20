using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoneySwingTest : MonoBehaviour
{
    public Rigidbody swingAnchor;
    public Rigidbody hookRigidbody;
    //public SpringJoint hookJoint;
    
    public Vector3 originPosition;

    public float distance;

    public void OnValidate()
    {
        distance = Vector3.Distance(transform.position, swingAnchor.transform.position);
    }

    void Start()
    {
        hookRigidbody = GetComponent<Rigidbody>();
        originPosition = hookRigidbody.position;
        
        
        
        
    }
    
    
    public void DisableCollisions()
    {
        gameObject.GetComponent<Collider>().enabled = false;
    }

    public void EnableCollisions()
    {
        gameObject.GetComponent<Collider>().enabled = true;
    }
   
}