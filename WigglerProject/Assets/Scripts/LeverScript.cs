using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class LeverScript : EnvironmentObject
{
    public UnityEvent OnLeverPulledEvent, StartupEvent;
    public bool isRepeating;
    private float maxPullDistance;
    public float pullDistance;
    public Transform leverOrigin;
    Vector3 originalPosition;
    public bool activated;

    Vector3 lastPosition;
    public float resetTime;

    private Tween pullTween;
    
    public LineRenderer pullLineRenderer;

    private float originalOffset;
    
    protected override void Awake()
    {
        base.Awake();
        originalPosition = rb.transform.position;
        
    }
    void Start()
    {
        StartupEvent?.Invoke();
        maxPullDistance = pullDistance + originalOffset;
        
    }

    

    protected override void Update()
    {
        base.Update();
        pullLineRenderer.SetPosition(0, leverOrigin.transform.position);
        pullLineRenderer.SetPosition(1, rb.position);
    }

    public override void GrabMove()
    {
        //rb.transform.forward = forwardVector;
         //base.GrabMove(headRigidbodyPosition, forwardVector, grabSpeed);
       
         var distance = Vector3.Distance(rb.position, leverOrigin.transform.position);
         
         Debug.Log($"Lever Distance: " + distance);
       
        if (distance > maxPullDistance)
        {
            if (!activated)
            {
                OnLeverPulledEvent?.Invoke();
                activated = true;
                
               
            }
            
        }
        
        
        
        
        
        
    }


    public override void ResetGrab()
    {
        base.ResetGrab();
        GrabIdle();
    }

    public override void GrabIdle()
    {
        
        Vector3 targetPosition = Vector3.MoveTowards(rb.position, leverOrigin.transform.position, resetTime * Time.deltaTime );
        rb.MovePosition(targetPosition);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
}
