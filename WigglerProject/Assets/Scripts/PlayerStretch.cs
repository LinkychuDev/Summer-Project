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

    private CharacterController headSegment;
    private CharacterController bodySegment;
    private CharacterController tailSegment;

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {

        stretchButton.action.started += OnPlayerStretched;
        stretchButton.action.canceled += OnPlayerRetracted;
    }

    void OnDisable()
    {
        stretchButton.action.started -= OnPlayerStretched;
        stretchButton.action.canceled -= OnPlayerRetracted;
    }

    void Start()
    {
        camera = Camera.main.transform;
        segments = PlayerStateReference.instance.segments.ToArray();
        headSegment = segments[0].characterController;
        bodySegment = segments[1].characterController;
        tailSegment = segments[2].characterController;
        bodyOffset = Mathf.Abs(segments[1].spacingToNextSegment);
        tailOffset = Mathf.Abs(segments[2].spacingToNextSegment);
        maxDistanceHead = bodyOffset + stretchDistanceHead;
        minDistanceHead = bodyOffset;
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = stretchInput.action.ReadValue<Vector2>();
        isStretching = stretchButton.action.IsPressed();
        
        if (isStretching)
        {
            if (isRetracting)
                return;

            StretchEvent();
        }

        else
        {
            if (PlayerStateReference.instance.state == PlayerState.Stretching)
            {
                isRetracting = true;
            }
        }
    }
    
   
    void OnPlayerStretched(InputAction.CallbackContext context)
    {
        cachedHeadPosition = headSegment.transform.position;
        cachedBodyPosition = bodySegment.transform.position;
        cachedTailPosition = tailSegment.transform.position;
        currentStretchTime = 0;
        PlayerStateReference.instance.SetState(PlayerState.Stretching);
        

    }


  

    void ClampSegmentPosition()
    {
        /*var currentHeadPos = headSegment.transform.position;
       
        Debug.Log($"Desired Head Position: {currentHeadPos}");
        var boundingPos = bodySegment.transform.position + new Vector3(stretchDistanceHead + bodyOffset, 0, stretchDistanceHead + bodyOffset);
        Debug.Log($"Desired Body Position: {boundingPos}");
        float posX =Mathf.Clamp(transform.position.x, -boundingPos.x, boundingPos.x);
        float posZ = Mathf.Clamp(transform.position.z, -boundingPos.z, boundingPos.z);
        headSegment.position = new Vector3(posX, transform.position.y, posZ);*/
    }
    
    void StretchEvent()
    {
        
        
        //take stretching position
        SteerEvent();
        
      //  currentStretchDistance = Mathf.Lerp(minDistanceHead, maxDistanceHead, currentStretchTime );

      if (moveInput.magnitude > 0.01f)
      {
          currentStretchTime += Time.deltaTime;
          Vector3 targetInitialPosition =
              bodySegment.transform.position + maxDistanceHead * headSegment.transform.forward;
          Vector3 targetStretchPosition = Vector3.Lerp(headSegment.transform.position, targetInitialPosition,
              currentStretchTime / stretchTime);
          

          Vector3 direction = headSegment.transform.position - bodySegment.transform.position;
          float distance = direction.magnitude;

          if (distance > maxDistanceHead)
          {
              Debug.Log($"Can't move \n Current Distance is {distance} \n Max Distance is {maxDistanceHead}");
          }

          else
          {
              Vector3 moveDelta = targetStretchPosition - headSegment.transform.position;
              // headSegment.MovePosition(currentStretchDistance * stretchSpeed * Time.deltaTime);
              headSegment.Move(moveDelta * Time.deltaTime);
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
        
        
        //rotate around middle segment position
        if(moveInput.magnitude > 0.01f)
        {
            
            Quaternion targetRotation = Quaternion.LookRotation(stretchDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, stretchTurnSpeed * Time.deltaTime);
            //var axis = Quaternion.AngleAxis(stretchTurnSpeed * Time.deltaTime, Vector3.up);

            //move in range of a circle around the bodyPosition



            //targetRotation.ToAngleAxis(out float angle, out var axis);
            //Debug.Log("Axis Angle: " + angle);
            //Debug.Log("Axis Axis: " + axis);
            //transform.RotateAround(bodySegment.position, axis, Mathf.Min(angle * stretchTurnSpeed * Time.deltaTime));
        }

        
        
    }
    
    


    void OnPlayerRetracted(InputAction.CallbackContext context)
    {
        PlayerStateReference.instance.SetState(PlayerState.Locomotion);
        StartCoroutine(PlayerRetractEvent());
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
