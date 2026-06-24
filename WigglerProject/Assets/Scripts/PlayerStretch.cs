using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerStretch : MonoBehaviour
{
    public enum StretchState
    {
        None,
        Stretching,
        Retracting,
        Swinging
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

    private HingeJoint headJoint;
    private HingeJoint bodyJoint;
    private HingeJoint tailJoint;

    private Transform hookPoint;

    [SerializeField] private float swingLimit = 75;

    [SerializeField] private float swingSpeed;

    [SerializeField] private float swingSetUpDuration = 0.2f;

    [SerializeField] private float swingLength = 3f;
    
    [SerializeField] private AnimationCurve swingCurve;
    
    private bool isSwingingSetUp;

    private float currentSwingAngle;
    float currentSwingTime;
  
    
    //spring joint values
    //private SpringJoint headJoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    private void OnEnable()
    {
        stretchButton.action.started += ctx => StartCoroutine(OnPlayerStretchedEvent());
        stretchButton.action.canceled += ctx => OnPlayerRetracted();
    }


    void OnDisable()
    {
        stretchButton.action.started -= ctx => StartCoroutine(OnPlayerStretchedEvent());
        stretchButton.action.canceled -= ctx => OnPlayerRetracted();
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
        
        /*collisionRadius = collisionRadiusOffset + headSegment.GetComponent<SphereCollider>().radius;
        colliders = new Collider[maxColliders];
        collisionMask = controller.playerCollisionMask;*/
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
       
        switch (stretchState)
        {
            case StretchState.Swinging:
                if(!isSwingingSetUp)
                    return;
                SwingEvent();
                break;
            case StretchState.Retracting:
                break;
            default:
                SteerEvent();
                StretchEvent();
                break;
                
        }
    }
    
    

    private void OnCollisionEnter(Collision other)
    {
        if(controller.state == PlayerState.Locomotion)
            return;
        if(stretchState == StretchState.Swinging)
            return;
        switch (stretchState)
        {
            case StretchState.Stretching:
                if (other.gameObject.TryGetComponent(out HoneySwingTest honeyTest))
                {
                    honeyTest.DisableCollisions();
                    StartSwing(honeyTest);
                     
                }
                break;
            case StretchState.Retracting:
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    void StartSwing(HoneySwingTest hook)
    {
        
        isSwingingSetUp = false;
        currentSwingTime = 0;
        //make this a sequence
        stretchState = StretchState.Swinging;
        //Vector3 anchorPoint = hook.transform.position - hook.swingAnchor;

        
        hookPoint = hook.swingAnchor;
        hookPoint.transform.localPosition = Vector3.zero;
        hookPoint.rotation = Quaternion.Euler(0, 0, 0);
        
        headSegment.isKinematic = true;
        
        
        Sequence hookSequence = DOTween.Sequence();
        
        
        
       // bodySegment.transform.SetParent(headSegment.transform, true);
        //tailSegment.transform.SetParent(bodySegment.transform, true);
        
        
        
        
        //headSegment.transform.SetParent(hookPoint, true);
        
        hookSequence.Append(headSegment.transform.DOMove(hookPoint.transform.position, 0.1f));
        
        
        hookSequence.Join(bodySegment.DOMove(hookPoint.position  + (-hookPoint.transform.up * (bodyOffset + swingLength)), 0.1f)).SetEase(Ease.OutBounce);
        
        
        hookSequence.Join(tailSegment.DOMove(hookPoint.position + (-hookPoint.transform.up  * (tailOffset + bodyOffset+ swingLength)), 0.1f).SetEase(Ease.OutBounce));


        hookSequence.OnComplete(() =>
        {
            headSegment.isKinematic = false;
            bodySegment.isKinematic = false;
            tailSegment.isKinematic = false;


            headSegment.useGravity = true;
            bodySegment.useGravity = true;
            tailSegment.useGravity = true;


            headJoint = headSegment.gameObject.AddComponent<HingeJoint>();
            bodyJoint = bodySegment.gameObject.AddComponent<HingeJoint>();
            tailJoint = tailSegment.gameObject.AddComponent<HingeJoint>();
            
            
            
            headJoint.autoConfigureConnectedAnchor = false;
            headJoint.connectedBody = hook.GetComponent<Rigidbody>();
            headJoint.connectedAnchor = headSegment.transform.InverseTransformPoint(hookPoint.position);
            
            bodyJoint.connectedBody = headSegment;
            tailJoint.connectedBody = bodySegment;


            List<HingeJoint> currentJoints = new List<HingeJoint>();
            currentJoints.Add(headJoint);
            currentJoints.Add(bodyJoint);
            currentJoints.Add(tailJoint);


            for (int i = 0; i < currentJoints.Count; i++)
            {
                currentJoints[i].useLimits = true;
                currentJoints[i].limits = new JointLimits
                {
                    min = -swingLimit,
                    max = swingLimit,
                };
            }
            isSwingingSetUp = true;
        });

        //swingLength = Mathf.Abs(swingLength);


    }




    void SwingEvent()
    {
        //move object like pendulums
        
        
        //desired angle
       // float angle = swingLimit * Mathf.Sin(Time.time * swingSpeed);
        //total forces

       // var desiredForce = swingSpeed * Time.fixedDeltaTime * Time.fixedDeltaTime;
       // Vector3 direction = Vector3.Cross(stretchDirection, Vector3.down);
    
        //2 pi * squareroot of length/gravity
        
        //Vector3 distanceToAnchorBody = hookPoint.position - bodySegment.transform.position;
        //Vector3 distanceToAnchorTail = hookPoint.position - tailSegment.transform.position;
        
        
        
      //  Vector3 newBodyDir = Vector3.Cross(bodySegment.transform.forward,  -Vector3.up );



        //var  motor = headJoint.motor;
        float currentAngle = headSegment.rotation.z;
        //Debug.Log(currentSwingAngle);
        float inputY = moveInput.y;

        float direction = 0;

       // float inputDir = 0;

        //inputDir = moveInput.y;


        if (Mathf.Abs(inputY) > 0.01f)
        {
            direction = inputY;
        }
        
        

        else
        {
            if (currentAngle > swingLimit)
            {
                currentAngle = swingLimit;
                direction = -1;
            }
        
            else if (currentAngle < -swingLimit)
            {
                currentAngle = -swingLimit;
                direction = 1;
            }
        }

        //currentSwingTime += Time.deltaTime;
        



        float desiredAngle = swingLimit * (Mathf.Sin(Time.time * swingSpeed));

        currentAngle += desiredAngle;
        
        //hookPoint.rotation = Quaternion.Euler(0, 0, currentAngle);

        //don't want head moving too far

        //want middle segment moving far

        //want tail moving to the end point

        //swing arc


        //headSegment.angularVelocity = Vector3.forward * (direction * swingSpeed * Time.deltaTime);





        // float desiredAngle = Time.deltaTime * direction * swingSpeed ;


        //currentSwingAngle += desiredAngle;


        //currentSwingAngle = Mathf.Clamp(currentSwingAngle, -swingLimit, swingLimit);

        //hookPoint.localRotation = Quaternion.Euler(0, 0, currentSwingAngle);



        // float swingAngle = swingLimit * Mathf.Sin((Time.time + inputDir * (Time.time *swingSpeed)));

        //  float curvePos = Mathf.Abs(swingAngle/swingLimit);

        // float speedRatio = swingCurve.Evaluate(curvePos * Time.deltaTime);

        // Debug.Log("swing Ratio: " + speedRatio);

        // hookPoint.localRotation = Quaternion.Euler(0, 0, swingAngle * speedRatio);
        //hookPoint.rotation = Quaternion.Euler(0, 0, currentSwingAngle);





    }
    IEnumerator OnPlayerStretchedEvent()
    {

        
        //headSegment.isKinematic = true;

        if (stretchState != StretchState.None)
            yield break;

        if (headJoint != null)
        {
            Destroy(headJoint);
            
        }
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
        controller.SetState(PlayerState.Stretching);
        stretchState = StretchState.Stretching;


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
        constrainedOffset.y = 0;
        
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
            if (stretchState == StretchState.Stretching)
            {
                Quaternion targetRotation = Quaternion.LookRotation(stretchDirection.normalized, Vector3.up);
                headSegment.transform.rotation = (Quaternion.Slerp(headSegment.transform.rotation, targetRotation,
                    stretchTurnSpeed * Time.fixedDeltaTime));
            }

        }

        
        
    }
    
    void OnPlayerRetracted()
    {
        if (controller.state == PlayerState.Locomotion)
            return;
        if(stretchState == StretchState.Swinging)
            return;
        Debug.Log(stretchState);
        StartCoroutine(OnPlayerRetractedEvent());
    }

    public IEnumerator OnPlayerRetractedEvent()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log(stretchState);
        //yield return new WaitUntil(() => controller.state == PlayerState.Stretching);
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
            
        }
        
        
       
        
        stretchState =  StretchState.None;
        Honey = false;
        
        controller.SetState(PlayerState.Locomotion);
        
       
        
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


