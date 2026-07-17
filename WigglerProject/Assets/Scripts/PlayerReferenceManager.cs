using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

#region setup

public enum PlayerState
{
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
    public float honeyCooldown = 1;
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
    public bool canStretch = true;
    [SerializeField] private GameObject HoneyVisualiser;
    [SerializeField] private GameObject StretchVisualiser;


    public bool isOnCoyoteTime;
    public bool useCoyoteTime;


    public GameObject[] groundedCircle;

    public float groundedCircleMaxDistance;

    public bool launched;
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
        DisplayCharacterCircles();
    }
    

    void DisplayCharacterCircles()
    {
        for(int i = 0; i < segments.Count; i++)
        {
            if(Physics.Raycast(segments[i].rb.position, -segments[i].rb.transform.up, out RaycastHit hit, groundedCircleMaxDistance, groundMask))
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
                headSegment.isKinematic = false;
                bodySegment.isKinematic = true;
                tailSegment.isKinematic = true;
               
                headSegment.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                
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
        bool canStick;
        if (isHoney)
        {
            if (currentState == PlayerState.Stretching)
            {

                canStick = true;
            }

            else
            {
                canStick = false;
            }
           
        }

        else
        {
            canStick = false;
        }

        Debug.Log("CanStick: " + canStick);
        

        return canStick;
        //return currentState == PlayerState.Stretching;
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

   

    public void Honeyfied(bool val)
    {
        StopCoroutine(HoneyCooldown());
        isHoney = val;
        HoneyVisualiser.SetActive(val);

        if (val)
        {
            StartCoroutine(HoneyCooldown());
        }

    }

    IEnumerator HoneyCooldown()
    {
        if (PlayerStretch.stretchState == PlayerStretch.StretchState.Retracting)
        {
            yield return new WaitUntil(() =>
                PlayerStretch.stretchState != PlayerStretch.StretchState.Retracting ||
                currentState != PlayerState.Stretching);
        }
        else
        {
            yield return new WaitForSeconds(honeyCooldown);
        }
        isHoney = false;
        HoneyVisualiser.SetActive(false);
        
        //Honeyfied(false);
    }
}