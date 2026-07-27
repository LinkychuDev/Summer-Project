using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

#region setup

public enum PlayerState
{
    Default,
    Locomotion,
    Stretching,
    Stuck,
    Swinging,
    Launching
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
   public CharacterController characterController {get; private set;}
    public bool isGrounded {get;  set;}
    public Transform groundCheck;

    public float mass = 50;
    public PlayerSpringConnector springConnector {get; private set;}
    public void Initialise()
    {
        characterController = t.GetComponent<CharacterController>();
        
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
    [HideInInspector] public CharacterController headSegment;
    [HideInInspector] public CharacterController bodySegment;
    [HideInInspector] public CharacterController tailSegment;
    public LayerMask playerCollisionMask;
    
   
    [HideInInspector] public float bodyOffset;
    [HideInInspector] public float tailOffset;

    public int segmentIndexSpacing = 2;

    public List<CachedPosition> cachedHeadMovementPositions = new List<CachedPosition>();
    public int maxIterations = 30;

    public float minMoveDistance = 1;
    [SerializeField] private Transform headClose;

    [Header("Debug")] [SerializeField] public float distanceRadius = 1;
    
    public static Action<PlayerState> OnStateChange;
    
   // public bool isGrounded;
    public bool canStretch = true;
   
   


   // public bool isOnCoyoteTime;
    public bool useCoyoteTime;


    public GameObject[] groundedCircle;

    public float groundedCircleMaxDistance;

    public bool launched;
    public bool isOnSlope;

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

        headSegment = segments[0].characterController;
        bodySegment = segments[1].characterController;
        tailSegment = segments[2].characterController;
        
        bodyOffset = Mathf.Abs(segments[1].spacingToNextSegment);
        tailOffset = Mathf.Abs(segments[2].spacingToNextSegment);
        
        cachedHeadMovementPositions.Add(new CachedPosition(headClose.position, headSegment.transform.forward, headSegment.velocity));
      
        
        
        
        SetState(PlayerState.Locomotion);

    }


    private void Update()
    {
        //Handle Inputs
        DisplayCharacterCircles();
        
        UpdateCachedHeadPositions(headClose.position, headSegment.transform.forward, headSegment.velocity);
    }
    

    void DisplayCharacterCircles()
    {
        for(int i = 0; i < segments.Count; i++)
        {
            if(Physics.Raycast(segments[i].characterController.transform.position, -segments[i].characterController.transform.up, out RaycastHit hit, groundedCircleMaxDistance, groundMask))
            {
                Vector3 direction = Vector3.ProjectOnPlane(groundedCircle[i].transform.up, hit.normal);

                var rot = Quaternion.LookRotation(direction);

                groundedCircle[i].transform.rotation = rot;

                groundedCircle[i].transform.position = hit.point;

                if(!groundedCircle[i].activeSelf)
                {
                    groundedCircle[i].SetActive(true);
                }
            }

            else
            {
                groundedCircle[i].SetActive(false);
            }
        }
    }
    

    public void SetState(PlayerState newState)
    {
        switch (newState)
        {
            case PlayerState.Stretching:
               
                
                //segments[0].rb.useGravity = false;
                break;
            case PlayerState.Locomotion:
               
                PlayerStretch.stretchState = PlayerStretch.StretchState.None;
              
                
                //segments[0].rb.useGravity = true;
                break;
            case PlayerState.Stuck:
             
                break;
            case PlayerState.Swinging:
              
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

   
   

    


    private void FixedUpdate()
    {
     
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