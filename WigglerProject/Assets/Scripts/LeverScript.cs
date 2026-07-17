using System;
using UnityEngine;
using UnityEngine.Events;

public class LeverScript : EnvironmentObject
{
    public UnityEvent OnLeverPulled;
    public bool isRepeating;
    private float maxPullDistance;
    public float pullDistance;
    public Transform leverOrigin;
        
    void Start()
    {
        maxPullDistance = pullDistance + Vector3.Distance(leverOrigin.transform.position, rb.position);
    }

    public override void GrabMove(Vector3 headRigidbodyPosition)
    {
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
}
