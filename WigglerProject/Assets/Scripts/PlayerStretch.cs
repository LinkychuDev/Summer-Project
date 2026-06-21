using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStretch : MonoBehaviour
{
    public float stretchDistanceHead;
    public float stretchDistanceTail;
    public float stretchRetractTime = 4f;

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
    float minDistanceHead;
    public bool isStretching;
    private Segment[] segments;

    private bool atMaxStretchHeight = false;
    public bool isRetracting;
    private Transform camera;
    [SerializeField] private float stretchSpeed = 10f;

    [SerializeField] private float desiredPointsPerLine = 6;

    [SerializeField] private Transform headTarget;
    private float bodyOffset;
    private float tailOffset;
    
    private float currentStretchDistance;
    
    //spherecast detection
    [Header("Collision Detection")]
    public float collisionDetectionDistance = 2f;
    

    public float collisionDetectionRadius;

    public float correctionOffset = 0.3f;
    //public 
    
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
        segments = PlayerStateReference.instance.segments.ToArray();
        headSegment = segments[0].rb;
        bodySegment = segments[1].rb;
        tailSegment = segments[2].rb;
        bodyOffset = Mathf.Abs(segments[1].spacingToNextSegment);
        tailOffset = Mathf.Abs(segments[2].spacingToNextSegment);
        maxDistanceHead = bodyOffset + stretchDistanceHead;
        minDistanceHead = bodyOffset;
        
        
        //sphere collision
    }

    void Update()
    {
        moveInput = stretchInput.action.ReadValue<Vector2>();
        isStretching = stretchButton.action.IsPressed();



        
        
       
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
       
        if(isRetracting)
            return;
        SteerEvent();
        StretchEvent();
        
        /*
        else if (!isStretching && PlayerStateReference.instance.state == PlayerState.Stretching)
        {
            OnPlayerRetracted();
        }
        */
        
       
        
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
        
        
       // bodySegment.transform.LookAt(cachedHeadPosition);
       // tailSegment.transform.LookAt(cachedBodyPosition);
        PlayerStateReference.instance.SetState(PlayerState.Stretching);


    }
    
    
    void StretchEvent()
    {
        
        Debug.Log("Event Called");
        //take stretching position
      
        
      //  currentStretchDistance = Mathf.Lerp(minDistanceHead, maxDistanceHead, currentStretchTime );

        Vector3 finalPosition = headSegment.transform.position + stretchDirection * (stretchSpeed * Time.deltaTime);
        
        Vector3 direction = bodySegment.transform.position - finalPosition;
        
        float distance = direction.magnitude;

        if (distance > maxDistanceHead)
        {
            finalPosition = bodySegment.transform.position + maxDistanceHead * -direction.normalized;
        }

        headSegment.MovePosition(finalPosition);
        
      

    }


    private void OnCollisionEnter(Collision other)
    {
        
        if(PlayerStateReference.instance.state != PlayerState.Stretching)
            return;
        if (isStretching)
        {
            if (other.transform.TryGetComponent(out EnvironmentTest test))
            {
                test.Hit();
            }
        }
        
        else if (isRetracting)
        {
            
        }
    }

    void CollisionCheckEvent()
    {
        if (Physics.SphereCast(headSegment.position, collisionDetectionRadius, transform.forward, out RaycastHit hit,
                collisionDetectionDistance, PlayerStateReference.instance.GetMask(gameObject.layer)))
        {
            if (hit.transform.TryGetComponent(out EnvironmentTest test))
            {
                test.Hit();
            }
        }
    }
   
    

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
            
            
            
            //var axis = Quaternion.AngleAxis(stretchTurnSpeed * Time.deltaTime, Vector3.up);

            //move in range of a circle around the bodyPosition

            
            //targetRotation.ToAngleAxis(out float angle, out var axis);
            //Debug.Log("Axis Angle: " + angle);
            //Debug.Log("Axis Axis: " + axis);
            //transform.RotateAround(bodySegment.position, axis, Mathf.Min(angle * stretchTurnSpeed * Time.deltaTime));
        }

        
        
    }

    private void LateUpdate()
    {
        if (PlayerStateReference.instance.state == PlayerState.Stretching && isStretching)
        {
           // UpdateSegmentsRotation();
        }
       
    }

    void UpdateSegmentsRotation()
    {
        Vector3 direction = headSegment.transform.position - bodySegment.transform.position ;

        Quaternion bodyRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            
        bodySegment.transform.rotation = Quaternion.Slerp(bodySegment.transform.rotation, bodyRotation, Mathf.Lerp(1, stretchTurnSpeed, 0.8f )* Time.deltaTime);


        Vector3 tailDirection = (bodySegment.transform.position - tailSegment.transform.position).normalized;
        tailSegment.transform.rotation = Quaternion.Slerp(tailSegment.transform.rotation, Quaternion.LookRotation(tailDirection, Vector3.up), stretchTurnSpeed * Time.deltaTime);
        tailSegment.transform.position = bodySegment.transform.position - tailSegment.transform.forward *  Mathf.Abs(segments[2].spacingToNextSegment) ;
        
    }

    void OnPlayerRetracted()
    {
        isStretching  = false;
        isRetracting = false;
        
        
        PlayerStateReference.instance.SetState(PlayerState.Locomotion);
       // StartCoroutine(PlayerRetractEvent());
    }

    IEnumerator OnPlayerRetractedEvent()
    {
        if (PlayerStateReference.instance.state != PlayerState.Stretching)
            yield break;
        isRetracting = true;
        currentStretchTime = 0;
     
        tailSegment.transform.LookAt(bodySegment.transform.position + bodySegment.transform.forward);
        bodySegment.isKinematic = false;
        tailSegment.isKinematic = false;
        yield return new WaitForFixedUpdate();
        
        
        cachedHeadPosition = headSegment.transform.position;
        Vector3 distanceToHead = (cachedHeadPosition - cachedBodyPosition).normalized;
        Vector3 distanceToBody = (cachedBodyPosition - cachedTailPosition).normalized;
        Vector3 targetBodyBodyPos = cachedHeadPosition - Mathf.Abs(segments[1].spacingToNextSegment) * (distanceToHead);
       
        
        //Quaternion cachedBodyRotation = bodySegment.transform.rotation;



        
        Vector3 targetTailPosition = targetBodyBodyPos - Mathf.Abs(segments[2].spacingToNextSegment) * bodySegment.transform.forward;
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(bodySegment.DOMove(targetBodyBodyPos, stretchRetractTime).OnUpdate(() => 
            bodySegment.transform.LookAt(headSegment.transform.position + headSegment.transform.forward))).SetEase(Ease.OutBounce);
        sequence.Insert(0.1f, tailSegment.DOMove(targetTailPosition, stretchRetractTime)
            .OnUpdate(() => tailSegment.transform.LookAt(bodySegment.transform.position + bodySegment.transform.forward))).SetEase(Ease.OutBounce);
        
        
       
        yield return sequence.WaitForCompletion();
        
        yield return new WaitForFixedUpdate();
        OnPlayerRetracted();
        
        //yield return null;

    }

    void UpdateTargetRotation(Transform segment, Vector3 distanceToSegment)
    {
        Debug.Log("Called");
        segment.rotation = Quaternion.Slerp(segment.rotation, Quaternion.LookRotation(distanceToSegment, Vector3.up), stretchTurnSpeed * Time.fixedDeltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * collisionDetectionDistance);
        Gizmos.DrawWireSphere(transform.position + transform.forward * collisionDetectionDistance, collisionDetectionRadius);
    }
}
