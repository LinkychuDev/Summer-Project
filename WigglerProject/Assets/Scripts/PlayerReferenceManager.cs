using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RotaryHeart.Lib.PhysicsExtension;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Physics = UnityEngine.Physics;

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
   public Rigidbody rb {get; private set;}
    public bool isGrounded {get;  set;}
    public Transform groundCheck;

    
    public float mass = 50;
    public PlayerSpringConnector springConnector {get; private set;}
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
    [HideInInspector] public Rigidbody headSegment;
    [HideInInspector] public Rigidbody bodySegment;
    [HideInInspector] public Rigidbody tailSegment;
    public LayerMask playerCollisionMask;
    public LayerMask playerMask;
   
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

    public Renderer headRenderer;
    public Material headMaterial;

    public Vector3 playerGravityDir = Vector3.up;
    
    public bool isOnCoyoteTime;
    public bool useCoyoteTime;


    public GameObject[] groundedCircle;

    public float groundedCircleMaxDistance;

    public bool launched;
    public bool isOnSlope;
    public bool useGravity;
    public bool isGrounded;

    public PlayerStretch playerStretch;

    public bool isSoaked;
    public float soakedDuration = 2f;

    public Transform playerCam;

    
    
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

        headSegment = segments[0].rb;
        bodySegment = segments[1].rb;
        tailSegment = segments[2].rb;
        
        bodyOffset = Mathf.Abs(segments[1].spacingToNextSegment);
        tailOffset = Mathf.Abs(segments[2].spacingToNextSegment);
        
       cachedHeadMovementPositions.Add(new CachedPosition(headClose.position, headSegment.transform.forward, headSegment.linearVelocity));
        
        headMaterial = headRenderer.material;
        
        playerGravityDir = Vector3.up;

        playerMask = 1 << headSegment.gameObject.layer;
        
        playerStretch = headSegment.GetComponent<PlayerStretch>();
        SetState(PlayerState.Locomotion);

    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
    }

    private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        playerCam = Camera.main.transform;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
    }

    public void DisableInput()
    {
        InputManager.instance.controls.Gameplay.Disable();
    }

    public void EnableInput()
    {
        InputManager.instance.controls.Gameplay.Enable();
    }
    
    private void Update()
    {
        //Handle Inputs
        DisplayCharacterCircles();
        
        UpdateCachedHeadPositions(headClose.position, headSegment.transform.forward, headSegment.linearVelocity);
    }
    

    void DisplayCharacterCircles()
    {
        for(int i = 0; i < segments.Count; i++)
        {
            if(Physics.Raycast(segments[i].rb.transform.position, -segments[i].rb.transform.up, out RaycastHit hit, groundedCircleMaxDistance, groundMask))
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
        bool freezeHead = false;
        bool freezeBodyandTail = false;
        switch (newState)
        {
            case PlayerState.Stretching:
                useGravity = false;
                
                freezeBodyandTail = true;
                //segments[0].rb.useGravity = false;
                break;
            case PlayerState.Locomotion:
                freezeBodyandTail = false;
                useGravity = true;
                playerStretch.UpdateStretchState(PlayerStretch.StretchState.None);
                //segments[0].rb.useGravity = true;
                break;
            case PlayerState.Stuck:
                freezeBodyandTail = true;
                break;
            case PlayerState.Swinging:
                useGravity = false;
                break;
            
                
        }
        
        



        headSegment.isKinematic = freezeHead;
        bodySegment.isKinematic = freezeBodyandTail;
        tailSegment.isKinematic = freezeBodyandTail;
        
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

    public Vector3[] GetSegmentPositions()
    {
        Vector3[] pos =  new Vector3[3];

        pos[0] = headSegment.position;
        pos[1] = bodySegment.position;
        pos[2] = tailSegment.position;
        return pos;
    }
   

   
}