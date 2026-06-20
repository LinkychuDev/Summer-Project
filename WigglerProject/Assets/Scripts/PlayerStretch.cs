using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStretch : MonoBehaviour
{
    public float stretchDistanceHead;
    public float stretchDistanceTail;
    public float stretchTime = 4f;

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
  
    //public 
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   

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
        Debug.Log("PlayerState: " + PlayerStateReference.instance.state.ToString());
        Debug.Log("moveInput: " + moveInput);

        
       
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (isStretching && PlayerStateReference.instance.state != PlayerState.Stretching)
        {
            OnPlayerStretched();
        }
        
        else if (isStretching && PlayerStateReference.instance.state == PlayerState.Stretching)
        {
            StretchEvent();
        }
        
        else if (!isStretching && PlayerStateReference.instance.state == PlayerState.Stretching)
        {
            OnPlayerRetracted();
        }
        
       
        
    }
    
   
    void OnPlayerStretched()
    {
        cachedHeadPosition = headSegment.transform.position;
        cachedBodyPosition = bodySegment.transform.position;
        cachedTailPosition = tailSegment.transform.position;
        currentStretchTime = 0;
        PlayerStateReference.instance.SetState(PlayerState.Stretching);
        

    }
    
    
    void StretchEvent()
    {
        
        Debug.Log("Event Called");
        //take stretching position
        SteerEvent();
        
      //  currentStretchDistance = Mathf.Lerp(minDistanceHead, maxDistanceHead, currentStretchTime );

      if (moveInput.magnitude > 0.01f)
      {
          Debug.Log("Moving");
          //currentStretchTime += Time.fixedDeltaTime;
          Vector3 initialTargetPosition =
              bodySegment.transform.position + maxDistanceHead * headSegment.transform.forward;
         // Vector3 targetStretchPosition = Vector3.Lerp(headSegment.position, targetInitialPosition,
              //currentStretchTime / stretchTime);
          
              
              
              
          //move player
          Vector3 targetStretchPosition = headSegment.position + stretchDirection * (stretchSpeed * Time.fixedDeltaTime);
          
          

          Vector3 direction = headSegment.position - bodySegment.transform.position;
          float distance = direction.magnitude;

          if (distance > maxDistanceHead)
          {
              //clamp stretch position
              targetStretchPosition = bodySegment.transform.position + maxDistanceHead * direction.normalized;
              Debug.Log($"Can't move \n Current Distance is {distance} \n Max Distance is {maxDistanceHead}");
          }

          //collision check here
          
          //Vector3 finalVelocity = segments[0].collisionDetection.CollideAndSlide(targetStretchPosition, headSegment.position, 0, true, targetStretchPosition);
        //  if (finalVelocity == Vector3.zero)
          //{
         //     finalVelocity = headSegment.position;
        //  }
         // headSegment.MovePosition(finalVelocity);
       //   
        //  headSegment.MovePosition(targetStretchPosition);
      }

      CollisionCheckEvent();
      

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
            headSegment.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, stretchTurnSpeed * Time.fixedDeltaTime));
            //var axis = Quaternion.AngleAxis(stretchTurnSpeed * Time.deltaTime, Vector3.up);

            //move in range of a circle around the bodyPosition



            //targetRotation.ToAngleAxis(out float angle, out var axis);
            //Debug.Log("Axis Angle: " + angle);
            //Debug.Log("Axis Axis: " + axis);
            //transform.RotateAround(bodySegment.position, axis, Mathf.Min(angle * stretchTurnSpeed * Time.deltaTime));
        }

        
        
    }
    
    


    void OnPlayerRetracted()
    {
        isStretching  = false;
        isRetracting = false;
        PlayerStateReference.instance.SetState(PlayerState.Locomotion);
       // StartCoroutine(PlayerRetractEvent());
    }

    IEnumerator PlayerRetractEvent()
    {
        if (PlayerStateReference.instance.state != PlayerState.Stretching)
            yield break;
        isRetracting = true;
        currentStretchTime = 0;
        cachedHeadPosition = headSegment.transform.position;
        Vector3 distanceToHead = (cachedBodyPosition - cachedHeadPosition).normalized;
        Vector3 distanceToBody = (cachedTailPosition - cachedBodyPosition).normalized;
        Vector3 targetBodyBodyPos = cachedHeadPosition - segments[1].spacingToNextSegment * (distanceToHead);
        Vector3 targetTailPosition = targetBodyBodyPos - segments[2].spacingToNextSegment * (distanceToBody);
        /*while (currentStretchTime < stretchTime)
        {
            currentStretchTime += Time.deltaTime;
            float percent = currentStretchTime / stretchTime;
            bodySegment.MovePosition(Vector3.Lerp(cachedBodyPosition, targetBodyBodyPos, percent));
            tailSegment.MovePosition(Vector3.Lerp(cachedTailPosition, targetTailPosition, percent));
            yield return null;

        }*/
        
        isRetracting = false;
        atMaxStretchHeight = false;
        isStretching = false;
        PlayerStateReference.instance.SetState(PlayerState.Locomotion);
       


    }

    
}
