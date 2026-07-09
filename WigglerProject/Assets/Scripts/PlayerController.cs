using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public enum PlayerState
{
    Locomotion,
    Stretching,
    Stuck,
    Swinging
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
     public SegmentType type;
    public float spacingToNextSegment;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public bool isGrounded;

    [HideInInspector] public PlayerSpringConnector springConnector;
    public void Initialise()
    {
        rb = t.GetComponent<Rigidbody>();
        
        if (this.type != SegmentType.Head)
        {
            springConnector = t.GetComponent<PlayerSpringConnector>();
        }
        
        
       
       
    }

    
    
  
    
    
}


public class CachedPosition
{
    public Vector3 lastPosition;
    public Quaternion lastRotation;
    public CachedPosition(Vector3 lastPosition, Quaternion lastRotation)
    {
        this.lastPosition = lastPosition;
        this.lastRotation = lastRotation;
    }
}
public class PlayerController : MonoBehaviour, IStickable
{

    
    public List<Segment> segments = new List<Segment>();
    public PlayerState state;
    public PlayerState lastState;

    public Sequence sequence;
    [HideInInspector] public Rigidbody headSegment, bodySegment, tailSegment;
    [SerializeField] private float bounceMultiplier = 200f;
    public LayerMask playerCollisionMask;
    public float honeyOffsetDown = 1;
    private PlayerStretch _stretch;
    
    [HideInInspector] public float bodyOffset;
    [HideInInspector] public float tailOffset;

    public int segmentIndexSpacing = 2;

    public List<CachedPosition> cachedHeadPositions = new List<CachedPosition>();
    public int maxIterations = 30;
    

    [SerializeField] private Transform headClose;
    
    private void Awake()
    {
        foreach (Segment segment in segments)
        {
            segment.Initialise();
        }

        headSegment = segments[0].rb;
        bodySegment = segments[1].rb;
        tailSegment = segments[2].rb;
        
        bodyOffset = Mathf.Abs(segments[1].spacingToNextSegment);
        tailOffset = Mathf.Abs(segments[2].spacingToNextSegment);
        _stretch = GetComponent<PlayerStretch>();
        
        
        
        SetState(PlayerState.Locomotion);

    }


    private void Update()
    {
        //Handle Inputs
        
    }
    
    

    public void SetState(PlayerState newState)
    {
        switch (newState)
        {
            case PlayerState.Stretching:
                headSegment.isKinematic = false;
                bodySegment.isKinematic = true;
                tailSegment.isKinematic = true;
               
                headSegment.constraints = RigidbodyConstraints.FreezePositionY;
                
                //segments[0].rb.useGravity = false;
                break;
            case PlayerState.Locomotion:
                headSegment.isKinematic = false;
                bodySegment.isKinematic = false;
                tailSegment.isKinematic = false;
                _stretch.stretchState = PlayerStretch.StretchState.None;
                headSegment.constraints = RigidbodyConstraints.FreezeRotation;
                bodySegment.constraints = RigidbodyConstraints.FreezeRotation;
                tailSegment.constraints = RigidbodyConstraints.FreezeRotation;
                //segments[0].rb.useGravity = true;
                break;
            case PlayerState.Stuck:
                headSegment.linearVelocity = Vector3.zero;
                _stretch.Honey = true;
                headSegment.isKinematic = true;
                bodySegment.isKinematic = true;
                tailSegment.isKinematic = true;
                break;
            case PlayerState.Swinging:
                headSegment.isKinematic = true;
                bodySegment.isKinematic = true;
                tailSegment.isKinematic = true;
                break;
                
        }
        
        lastState = state;
        state = newState;
    }

    public PlayerState GetLastState()
    {
        return lastState;
    }


    public void Bounce(float bounceHeight)
    {
        foreach (Segment segment in segments)
        {
            segment.rb.AddForce(Vector3.up * bounceHeight * bounceMultiplier * Time.deltaTime, ForceMode.VelocityChange);
            Debug.Log("Supposed to bounce");
        }
    }

    public bool CanStick()
    {
        return state == PlayerState.Stretching;
    }


    private void FixedUpdate()
    {
      
        //UpdateSegments();
    }

    public void UpdateCachedHeadPositions(Vector3 position, Quaternion rotation)
    {
        if (cachedHeadPositions.Count > 0)
        {
            if (cachedHeadPositions[0].lastPosition != position)
            {
                cachedHeadPositions.Insert(0, new CachedPosition(position, rotation));

            }
        }

        else
        {
            cachedHeadPositions.Add(new CachedPosition(position, rotation));
        }
        
      
        if (cachedHeadPositions.Count > maxIterations)
        {
            cachedHeadPositions.RemoveAt(cachedHeadPositions.Count - 1);
        }
        
        // UpdateSegments();
    }

    
    public void ClearCachedHeadPositions()
    {
        cachedHeadPositions.Clear();
    }
    
}