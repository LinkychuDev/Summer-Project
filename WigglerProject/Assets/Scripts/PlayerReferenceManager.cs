using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

#region PlayerStateSetup




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
    public Vector3 lastRotation;
    public Vector3 cachedVelocity;
    public CachedPosition(Vector3 lastPosition, Vector3 lastRotation, Vector3 cachedVelocity)
    {
        this.lastPosition = lastPosition;
        this.lastRotation = lastRotation;
        this.cachedVelocity = cachedVelocity;
    }
}

#endregion


public class PlayerReferenceManager : MonoBehaviour, IStickable
{

    public LayerMask groundMask;
    public static PlayerReferenceManager instance;
    public List<Segment> segments = new List<Segment>();
    public PlayerState currentState;
    public PlayerState lastState;
    public float gravity = -15f;
    public Sequence sequence;
    [HideInInspector] public Rigidbody headSegment, bodySegment, tailSegment;
    [SerializeField] private float bounceMultiplier = 200f;
    public LayerMask playerCollisionMask;
    public float honeyOffsetDown = 1;
    private PlayerStretch _stretch;
    
    [HideInInspector] public float bodyOffset;
    [HideInInspector] public float tailOffset;

    public int segmentIndexSpacing = 2;

    public List<CachedPosition> cachedHeadMovementPositions = new List<CachedPosition>();
    public int maxIterations = 30;

    public float minMoveDistance = 1;
    [SerializeField] private Transform headClose;

    [Header("Debug")] [SerializeField] public float distanceRadius = 1;
    
    public static Action<PlayerState> OnStateChange;

    public bool isHoney;
    public bool isGrounded;

    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        foreach (Segment segment in segments)
        {
            segment.Initialise();
        }

        headSegment = segments[0].rb;
        bodySegment = segments[1].rb;
        tailSegment = segments[2].rb;
        
        bodyOffset = Mathf.Abs(segments[1].spacingToNextSegment);
        tailOffset = Mathf.Abs(segments[2].spacingToNextSegment);
        
        cachedHeadMovementPositions.Add(new CachedPosition(headClose.position, headSegment.transform.forward, headSegment.linearVelocity));
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
                PlayerStretch.stretchState = PlayerStretch.StretchState.None;
                headSegment.constraints = RigidbodyConstraints.FreezeRotation;
                bodySegment.constraints = RigidbodyConstraints.FreezeRotation;
                tailSegment.constraints = RigidbodyConstraints.FreezeRotation;
                //segments[0].rb.useGravity = true;
                break;
            case PlayerState.Stuck:
                headSegment.linearVelocity = Vector3.zero;
                isHoney = true;
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


        
        lastState = currentState;
        currentState = newState;
        
        if (lastState != newState)
        {
            OnStateChange?.Invoke(currentState);
        }
    }

    public PlayerState GetLastState()
    {
        return lastState;
    }

   
   

    public bool CanStick()
    {
        return currentState == PlayerState.Stretching;
    }


    private void FixedUpdate()
    {
       UpdateCachedHeadPositions(headClose.position, headSegment.transform.forward, headSegment.linearVelocity);
        //UpdateSegments();
    }

    public void UpdateCachedHeadPositions(Vector3 position, Vector3 forwardVector, Vector3 cachedVelocity)
    {
        
        if (cachedHeadMovementPositions.Count > 0)
        {
            if(Vector3.Distance(cachedHeadMovementPositions[0].lastPosition, position) < minMoveDistance)
                return;
            if (cachedHeadMovementPositions[0].lastPosition != position)
            {
                cachedHeadMovementPositions.Insert(0, new CachedPosition(position, forwardVector, cachedVelocity));
            }
        }

        else
        {
            //initial
            cachedHeadMovementPositions.Add(new CachedPosition(position, forwardVector, cachedVelocity));
        }
        
      
        
        if (cachedHeadMovementPositions.Count > maxIterations)
        {
            cachedHeadMovementPositions.RemoveAt(cachedHeadMovementPositions.Count - 1);
        }



        


        Debug.Log(cachedHeadMovementPositions.Count);
        
        // UpdateSegments();
    }

    
    public void ClearCachedHeadPositions()
    {
        cachedHeadMovementPositions.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if(cachedHeadMovementPositions.Count == 0)
            return;
        Gizmos.color = Color.red;

        for (int i = 0; i < cachedHeadMovementPositions.Count; i++)
        {
            Gizmos.DrawWireSphere(cachedHeadMovementPositions[i].lastPosition, distanceRadius);
        }
      
    }
}