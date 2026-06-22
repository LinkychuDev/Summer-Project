using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerStretch : MonoBehaviour
{
    public enum StretchState
    {
        None,
        Stretching,
        Retracting
    }
    public float stretchDistanceHead;
    public float stretchDistanceTail;
    public float stretchRetractTime = 4f;


    public bool Honey;
    //public float stretchTime = 3f;
    //public bool isStretching;
    public InputActionReference stretchButton;

    public InputActionReference stretchInput;
    private Vector2 moveInput;
    Vector3 stretchDirection;

    private Rigidbody headSegment;
    private Rigidbody bodySegment;
    private Rigidbody tailSegment;

    private float currentStretchTime;
    Vector3 cachedHeadPosition;
    Vector3 cachedBodyPosition;
    Vector3 cachedTailPosition;
    
    Vector3 targetHeadPosition;

    public float stretchTurnSpeed = 15f;
    //public float stretchSpeed = 2f;
    private float maxDistanceHead;
    private Segment[] segments;
    private Transform camera;
    [SerializeField] private float stretchSpeed = 10f;
    [SerializeField] private float maxStretchSpeed = 10f;

    [SerializeField] private float desiredPointsPerLine = 6;

    [SerializeField] private Transform headTarget;
    private float bodyOffset;
    private float tailOffset;
    
    private float currentStretchDistance;
    
    //spherecast detection
    [Header("Collision Detection")] 
    public float collisionRadiusOffset = 0.05f;
    private float collisionRadius;
    public float correctionOffset = 0.3f;

    public int maxColliders = 1;
    
    private Collider[] colliders;
    
    private LayerMask collisionMask;
    //public 
    
    public StretchState stretchState = StretchState.None;

    PlayerController controller;

    //spring joint values
    //private SpringJoint headJoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    private void OnEnable()
    {
        stretchButton.action.started += ctx => StartCoroutine(OnPlayerStretchedEvent());
        stretchButton.action.canceled += ctx => StartCoroutine(OnPlayerRetractedEvent());
    }


    void OnDisable()
    {
        
    }
    void Start()
    {
        camera = Camera.main.transform;
        controller = GetComponent<PlayerController>();
        
        
        
        segments = controller.segments.ToArray();
        headSegment = segments[0].rb;
        bodySegment = segments[1].rb;
        tailSegment = segments[2].rb;
        bodyOffset = Mathf.Abs(segments[1].spacingToNextSegment);
        tailOffset = Mathf.Abs(segments[2].spacingToNextSegment);
        maxDistanceHead = bodyOffset + stretchDistanceHead;
        
        collisionRadius = collisionRadiusOffset + headSegment.GetComponent<SphereCollider>().radius;
        colliders = new Collider[maxColliders];
        collisionMask = controller.playerCollisionMask;
        //CreateJoint();
        //headSegment.GetComponent<SphereCollider>().radius
        //sphere collision
    }

    void Update()
    {
        moveInput = stretchInput.action.ReadValue<Vector2>();
        //isStretching = stretchButton.action.IsPressed();
        if(stretchState == StretchState.Retracting)
            return;
        if(controller.state != PlayerState.Stretching)
            return;
        


        
        
       
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
       
      
        if(controller.state != PlayerState.Stretching)
            return;
        CollisionCheck();
        if(stretchState == StretchState.Retracting)
            return;
        SteerEvent();
        StretchEvent();
        
        /*
        else if (!isStretching && controller.state == PlayerState.Stretching)
        {
            OnPlayerRetracted();
        }
        */
        
       
        
    }

    void CollisionCheck()
    {
        
        int numberOfColliders = Physics.OverlapSphereNonAlloc(headSegment.position, collisionRadius, colliders, collisionMask,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < numberOfColliders; i++)
        {
            if (stretchState == StretchState.Stretching)
            {
                if (colliders[i].transform.TryGetComponent(out IStretchInteractable stretchHit))
                {
                    stretchHit.OnStretchEvent(headSegment);
                }
            }
        
            else if (stretchState == StretchState.Retracting)
            {
                if (colliders[i].transform.TryGetComponent(out IRetractInteractable stretchHit))
                {
                    stretchHit.OnRetractEvent(headSegment);
                }
            }
        }
    }
    IEnumerator OnPlayerStretchedEvent()
    {

        
        //headSegment.isKinematic = true;
        
        bodySegment.isKinematic = true;
        tailSegment.isKinematic = true;
        Debug.Log("OnPlayerStretchedEvent");
        yield return null;
        //bodySegment.MovePosition(headSegment.position - (headSegment.transform.forward *bodyOffset));
       // tailSegment.MovePosition(bodySegment.position - (bodySegment.transform.forward * tailOffset));
        
        cachedHeadPosition = headSegment.transform.position;
        cachedBodyPosition = bodySegment.transform.position;
        cachedTailPosition = tailSegment.transform.position;
        currentStretchTime = 0;

       // CreateJoint();
       // bodySegment.transform.LookAt(cachedHeadPosition);
       // tailSegment.transform.LookAt(cachedBodyPosition);
        controller.SetState(PlayerState.Stretching);


    }


    
    
    void StretchEvent()
    {
        
        Debug.Log("Event Called");
        //take stretching position
      
        
      //  currentStretchDistance = Mathf.Lerp(minDistanceHead, maxDistanceHead, currentStretchTime );

        Vector3 targetDirection = stretchDirection.normalized;
        
        Vector3 targetVelocity = targetDirection * stretchSpeed;
      
       
        Vector3 currentOffset = (headSegment.position + targetDirection) - bodySegment.transform.position;
        
        
        Vector3 currentVelocity = headSegment.linearVelocity;
            

        currentVelocity.y = 0;
        Vector3 velocityChange = targetVelocity - currentVelocity;

        velocityChange = Vector3.ClampMagnitude(velocityChange, maxStretchSpeed);
        headSegment.AddForce(velocityChange, ForceMode.VelocityChange);
        
        
        Vector3 constrainedOffset = headSegment.position - bodySegment.transform.position;
        
        if (constrainedOffset.magnitude > maxDistanceHead)
        {
           Debug.Log("Potentially Over board");
            
           float outwardForce = Vector3.Dot(headSegment.linearVelocity, constrainedOffset.normalized);
           if (outwardForce > 0)
           {
               headSegment.AddForce(-constrainedOffset.normalized * (outwardForce * correctionOffset), ForceMode.VelocityChange);
               
               Debug.Log("Constrained");
           }
           
           
           headSegment.position = bodySegment.position + constrainedOffset.normalized * maxDistanceHead;

           
          
        }

        
        
   


    }


   
    /*
    private void OnCollisionEnter(Collision other)
    {
        
        if(controller.state != PlayerState.Stretching)
            return;
        
        if (isStretching)
        {
            if (other.transform.TryGetComponent(out IStretchInteractable stretchHit))
            {
                stretchHit.OnStretchEvent(headSegment);
            }
        }
        
        else if (isRetracting)
        {
            if (other.transform.TryGetComponent(out IRetractInteractable retractHit))
            {
                retractHit.OnRetractEvent(headSegment);
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        
        if(controller.state != PlayerState.Stretching)
            return;
        
        if (isStretching)
        {
            if (other.transform.TryGetComponent(out IStretchInteractable stretchHit))
            {
                stretchHit.OnStretchEvent(headSegment);
            }
        }
        
        else if (isRetracting)
        {
            if (other.transform.TryGetComponent(out IRetractInteractable stretchHit))
            {
                stretchHit.OnRetractEvent(headSegment);
            }
        }
    }
    */
    
    void SteerEvent()
    {
       
        Vector3 forward = camera.transform.forward;
        Vector3 right = camera.transform.right;
        forward.y = 0;
        forward.Normalize();
        right.y = 0;
        right.Normalize();

        //float stretchValue = 1;
        
        stretchDirection = moveInput.x * right + moveInput.y * forward;
        stretchDirection.y = 0;
        
        
        //rotate around middle segment position
        if(moveInput.magnitude > 0.01f)
        {
            
            Quaternion targetRotation = Quaternion.LookRotation(stretchDirection.normalized, Vector3.up);
            headSegment.transform.rotation =(Quaternion.Slerp(headSegment.transform.rotation, targetRotation, stretchTurnSpeed * Time.fixedDeltaTime));
            
        }

        
        
    }
    
    void OnPlayerRetracted()
    {
        stretchState =  StretchState.None;
        Honey = false;
        
        controller.SetState(PlayerState.Locomotion);
       // StartCoroutine(PlayerRetractEvent());
    }

    public IEnumerator OnPlayerRetractedEvent()
    {
        
        if (controller.state == PlayerState.Locomotion)
            yield break;
        yield return new WaitUntil(() => controller.state == PlayerState.Stretching);
        stretchState = StretchState.Retracting;
       
            
        currentStretchTime = 0;
     
        tailSegment.transform.LookAt(bodySegment.transform.position + bodySegment.transform.forward);
        bodySegment.isKinematic = false;
        tailSegment.isKinematic = false;
        yield return new WaitForFixedUpdate();
        

        if (!Honey)
        {
            Vector3 targetHeadDir = (bodySegment.transform.position + bodySegment.transform.forward) -headSegment.transform.position;
            Vector3 targetHeadPosition = bodySegment.transform.position + (bodySegment.transform.forward * bodyOffset);

            Sequence stretchSequence = DOTween.Sequence();
            stretchSequence.Append(headSegment.DOMove(targetHeadPosition, stretchRetractTime)).SetEase(Ease.OutBounce);
            yield return stretchSequence.WaitForCompletion();
            yield return new WaitForFixedUpdate();
            OnPlayerRetracted();
        }

        else
        {
            
            Sequence stretchSequence = DOTween.Sequence();
            cachedHeadPosition = headSegment.transform.position;
            Vector3 distanceToHead = (cachedHeadPosition - cachedBodyPosition).normalized;
            Vector3 distanceToBody = (cachedBodyPosition - cachedTailPosition).normalized;
            Vector3 targetBodyBodyPos = cachedHeadPosition - Mathf.Abs(segments[1].spacingToNextSegment) * (distanceToHead);
       
        
            //Quaternion cachedBodyRotation = bodySegment.transform.rotation;



        
            Vector3 targetTailPosition = targetBodyBodyPos - Mathf.Abs(segments[2].spacingToNextSegment) * bodySegment.transform.forward;

        
            stretchSequence.Append(bodySegment.DOMove(targetBodyBodyPos, stretchRetractTime).OnUpdate(() => 
                bodySegment.transform.LookAt(headSegment.transform.position + headSegment.transform.forward))).SetEase(Ease.OutBounce);
            stretchSequence.Insert(0.1f, tailSegment.DOMove(targetTailPosition, stretchRetractTime)
                .OnUpdate(() => tailSegment.transform.LookAt(bodySegment.transform.position + bodySegment.transform.forward))).SetEase(Ease.OutBounce);
            yield return stretchSequence.WaitForCompletion();
            yield return new WaitForFixedUpdate();
            OnPlayerRetracted();

        }
        
        
       
       
        
       
        
        //yield return null;

    }

    void UpdateTargetRotation(Transform segment, Vector3 distanceToSegment)
    {
        Debug.Log("Called");
        segment.rotation = Quaternion.Slerp(segment.rotation, Quaternion.LookRotation(distanceToSegment, Vector3.up), stretchTurnSpeed * Time.fixedDeltaTime);
    }

  
}

public interface IStretchInteractable
{
    public void OnStretchEvent(Rigidbody segment);
    
}


public interface IRetractInteractable
{
    public void OnRetractEvent(Rigidbody segment);
}


