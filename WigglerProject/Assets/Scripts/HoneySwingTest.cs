using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoneySwingTest : StickableObject, IPlayerHint
{
    public Rigidbody swingAnchor;
    public Rigidbody hookRigidbody;
    //public SpringJoint hookJoint;
    
    public Vector3 originPosition;

    public float distance;

  
    public LineRenderer lineRenderer;



    public override bool CanStick()
    {
        return canStick;
    }
    public void OnValidate()
    {
        distance = Vector3.Distance(transform.position, swingAnchor.transform.position);
        
        lineRenderer?.SetPosition(0, lineRenderer.transform.InverseTransformPoint(swingAnchor.transform.position));
        lineRenderer?.SetPosition(1, lineRenderer.transform.InverseTransformPoint(hookRigidbody.position));
    }

    
    
    void Start()
    {
        hookRigidbody = GetComponent<Rigidbody>();
        originPosition = hookRigidbody.position;
        
        
        
        
    }

    void Update()
    {
        lineRenderer.SetPosition(0, lineRenderer.transform.InverseTransformPoint(swingAnchor.transform.position));
        lineRenderer.SetPosition(1, lineRenderer.transform.InverseTransformPoint(hookRigidbody.position));
    }


   


    public void DisableCollisions()
    {
        gameObject.GetComponent<Collider>().enabled = false;
    }

    public void EnableCollisions()
    {
        gameObject.GetComponent<Collider>().enabled = true;
    }

    public PlayerActionEvent PlayerActionEvent { get; } = PlayerActionEvent.SwingAction;
}