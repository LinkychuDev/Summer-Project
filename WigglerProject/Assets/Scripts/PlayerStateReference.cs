using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum PlayerState
{
    Locomotion,
    Stretching
}

[System.Serializable]
public class Segment
{
    public enum SegmentType
    {
        Head,
        Body,
        Tail
    }

    public Transform t;
    public Transform visual;
    public float reactTime;
    public SegmentType type;
    public float spacingToNextSegment;
    public Rigidbody rb;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public CollisionDetection collisionDetection;

    public void Initialise()
    {
        rb = t.GetComponent<Rigidbody>();
        collisionDetection = t.GetComponent<CollisionDetection>();
       
    }

    
    
  
    
    
}
public class PlayerStateReference : MonoBehaviour
{
    public static PlayerStateReference instance {get; private set;}
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        foreach (Segment segment in segments)
        {
            segment.Initialise();
        }
    }
    
    public List<Segment> segments = new List<Segment>();
    public PlayerState state;

    public void SetState(PlayerState newState)
    {
        switch (newState)
        {
            case PlayerState.Stretching:
                //segments[0].rb.useGravity = false;
                break;
            case PlayerState.Locomotion:
                //segments[0].rb.useGravity = true;
                break;
        }
        state = newState;
    }

    
}