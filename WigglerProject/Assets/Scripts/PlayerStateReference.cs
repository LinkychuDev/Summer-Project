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
    public CharacterController characterController;
    

    public void Initialise()
    {
        characterController= t.GetComponent<CharacterController>();
       
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
    public PlayerState state { get; private set; }

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