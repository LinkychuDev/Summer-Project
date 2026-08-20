using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerSwing : MonoBehaviour
{
    public bool isSwinging;
    public Transform hookPoint;


    //supposed to be 120
    [SerializeField] private float swingLimitMin,  swingLimitMax;

    [SerializeField] private float swingSetUpDuration = 0.2f;

    [SerializeField] private float swingLength = 3f;


    private float currentSwingAngle;

    private Rigidbody headSegment;
    private Rigidbody bodySegment;
    private Rigidbody tailSegment;

    //public InputActionReference swingInputReference;


    public Vector2 swingInput;


    public Vector3 swingDirection;


    [SerializeField] private float swingSpeed = 30f;


    [SerializeField] private float swingDamp;

    [SerializeField] private Vector3 headTargetRotation = new(-90f, 0f, 0f);

    [SerializeField] private float launchSpeed;

    private float lastSwingAngle;
    [SerializeField] private bool useGravity = true;


    public Vector3 cachedSwingVelocity;
    //[SerializeField] private InputActionReference swingHeldInputReference;

    
    [SerializeField] private float headForwardLaunch = 4;
    private HoneySwingTest hookReference;

    public Vector3 swingVelocity;

    [SerializeField] private float swingAcceleration;
    //Vector3 accumulatedAngularVelocity;

    [SerializeField] private float headLaunchRatio, bodyLaunchRatio, tailLaunchRatio;

    private float cachedDampingBody, cachedDampingBodyAngular, cachedDampingTail, cachedDampingTailAngular;
    public bool isHeldDown;
    [SerializeField] private float upwardForce;
    [SerializeField] private float downTime;


    [SerializeField] private float airTime = 0.3f;

    // private bool enterSwingRecoveryMode;
    [SerializeField] private float swingTurnSpeed = 10f;

    [Header("Collision Detection")]
    //public float groundCheckDistance;
    public float collisionDistance = 0.01f;

    private Collider tailCollider;
    private Collider headCollider;
    private Collider bodyCollider;

    public bool isLaunching;

    [SerializeField] private Transform bodyOpen;
    [SerializeField] private Transform headClose;
    [SerializeField] private Transform bodyClose;
    [SerializeField] private Transform tailOpen;
    private Vector3 headClosePos;
    private Vector3 bodyOpenPos;
    private Vector3 bodyClosePos;
    private Vector3 tailOpenPos;

    public Vector3 swingDirNormalized;

    private float swingMagnitude;
    [SerializeField] private float curvePower = 0.05f;
    [SerializeField] private float swingPower = 0.8f;

    [SerializeField] private SpringJoint bodyJoint;


    [SerializeField] private float swingSpringStrength, swingSpringDamper, swingSpringMassScale;


   

    private void Start()
    {
        headSegment = PlayerReferenceManager.instance.headSegment;
        bodySegment = PlayerReferenceManager.instance.bodySegment;
        tailSegment = PlayerReferenceManager.instance.tailSegment;
    }


    public void StartSwing(HoneySwingTest hook)
    {
        if (hook == null)
            return;


        isSwinging = false;
        
        //make this a sequence
        //stretchState = StretchState.Swinging;
        //Vector3 anchorPoint = hook.transform.position - hook.swingAnchor;


        hookReference = hook;
        hookPoint = hookReference.swingAnchor;
        hookPoint.transform.localPosition = Vector3.zero;


        var direction = (headSegment.transform.position - hookPoint.position).normalized;
        //headSegment.isKinematic = true;
        headSegment.transform.rotation = hookPoint.rotation;
        headSegment.transform.position = hookPoint.position;

        headCollider = headSegment.GetComponent<SphereCollider>();
        bodyCollider = bodySegment.GetComponent<SphereCollider>();
        tailCollider = tailSegment.GetComponent<SphereCollider>();


        var hookSequence = DOTween.Sequence();


        headClosePos = headClose.localPosition;
        bodyOpenPos = bodyOpen.localPosition;
        bodyClosePos = bodyClose.localPosition;
        tailOpenPos = tailOpen.localPosition;

        float distance = Vector3.Distance(bodySegment.position, hookPoint.position);


        hookSequence.Append(headSegment.transform.DOMove(hookPoint.transform.position, swingSetUpDuration));

        //hookSequence.Join(headSegment.transform.DORotate(headSegment.rotation.eulerAngles + headTargetRotation, swingSetUpDuration));
        //headSegment.transform.SetParent(hookPoint, true);


        var targetBodyPosition =
            hookPoint.position + swingLength * (Vector3.down * PlayerReferenceManager.instance.bodyOffset);
        
       

        var targetTailPosition = targetBodyPosition + Vector3.down * PlayerReferenceManager.instance.tailOffset;
        hookSequence.Join(bodySegment.transform.DOMove(targetBodyPosition, swingSetUpDuration)).SetEase(Ease.OutBounce);


        hookSequence.Join(tailSegment.transform.DOMove(targetTailPosition, swingSetUpDuration)).SetEase(Ease.OutBounce);


        hookSequence.OnComplete(() =>
        {
            bodySegment.transform.position = new Vector3(headSegment.transform.position.x,
                bodySegment.transform.position.y, headSegment.transform.position.z);
            bodySegment.transform.rotation = hookPoint.rotation;


            tailSegment.transform.position = new Vector3(headSegment.transform.position.x,
                tailSegment.transform.position.y, headSegment.transform.position.z);
            tailSegment.transform.rotation = hookPoint.rotation;
            
            swingDirection = headSegment.transform.forward;

            //headSegment.isKinematic = f;

           
            bodyJoint = bodySegment.gameObject.AddComponent<SpringJoint>();
            
            bodyJoint.autoConfigureConnectedAnchor = false;
            bodyJoint.connectedAnchor = hookPoint.position;


            bodySegment.linearDamping = swingDamp;


            bodyJoint.spring = swingSpringStrength;
            bodyJoint.damper = swingSpringDamper;
            bodyJoint.massScale = swingSpringMassScale;

            bodyJoint.minDistance = swingLimitMin;
            
            bodyJoint.maxDistance = swingLimitMax;
            
            
            //bodySegment.AddForce(swingSpeed * , ForceMode.VelocityChange);
            isHeldDown = true;
            isSwinging = true;
        });


        //swingLength = Mathf.Abs(swingLength);
    }


    private void Update()
    {
        if(PlayerReferenceManager.instance.currentState != PlayerState.Swinging)
            return;
        if (isSwinging)
        {
            swingInput = InputManager.instance.controls.Gameplay.Move.ReadValue<Vector2>();
            isHeldDown = InputManager.instance.isStretchHeldDown;
        }

        
    }

    void FixedUpdate()
    {
        if(PlayerReferenceManager.instance.currentState != PlayerState.Swinging)
            return;
        if (isSwinging)
        {
            if (isHeldDown)
            {
                bodySegment.AddForce(PlayerReferenceManager.instance.gravity * Vector3.up, ForceMode.Acceleration);

                SwingEvent();
            }

            else
            {
                ReleaseEvent();
            }


        }
    }

    private void PositionSegments()
    {
        tailSegment.MovePosition(bodySegment.transform.position -
                                 bodySegment.transform.up * PlayerReferenceManager.instance.tailOffset);


        Vector3 ropeDir = (bodySegment.position - hookPoint.position).normalized;

        Vector3 tangent = Vector3.Cross(ropeDir, hookPoint.right).normalized;

        var bodyRot = Quaternion.LookRotation(tangent, Vector3.up);
        bodySegment.transform.rotation =
            Quaternion.Slerp(bodySegment.transform.rotation, bodyRot,
                Time.deltaTime * swingTurnSpeed);

        var tailRot = Quaternion.LookRotation(tangent, Vector3.up);
        tailSegment.transform.rotation =
            Quaternion.Slerp(tailSegment.transform.rotation, tailRot,
                Time.deltaTime * swingTurnSpeed);
    }

    private void SwingEvent()
    {
        var input = swingInput.y;
        
        bodySegment.AddForce(swingDirection * (input * swingSpeed), ForceMode.Acceleration);

        PositionSegments();
        
        /*
        Vector3 dir = (bodySegment.position - hookPoint.position).normalized;

        swingAcceleration = Vector3.Dot(bodySegment.linearVelocity, dir);
        cachedSwingVelocity = bodySegment.linearVelocity;
        */

    }


    private void ReleaseEvent()
    {
        if (hookReference == null)
            return;
        isSwinging = false;
        Debug.Log("Triggered");
       
        Destroy(bodyJoint);

        cachedSwingVelocity = bodySegment.linearVelocity;

        headClose.localPosition = new Vector3(headClosePos.x, headClosePos.y, headClosePos.z);
        bodyOpen.localPosition = new Vector3(bodyOpenPos.x, bodyOpenPos.y, bodyOpenPos.z);
        bodyClose.localPosition = new Vector3(bodyClosePos.x, bodyClosePos.y, bodyClosePos.z);
        tailOpen.localPosition = new Vector3(tailOpenPos.x, tailOpenPos.y, tailOpenPos.z);
        
        /*
        //PlayerReferenceManager.instance.launched = true;

        var launchDir = swingVelocity.normalized;

        //  forward bias
        launchDir = (launchDir + swingDirection * swingPower).normalized;


        // curved velocity
        var curvedLaunch = Mathf.Pow(swingVelocity.magnitude, curvePower) * launchSpeed;

        //jumpVelocity

        


        var launchVelocityHead = launchDir * (curvedLaunch * headLaunchRatio * headForwardLaunch);

        var launchVelocityBody = launchDir * (curvedLaunch * bodyLaunchRatio);
        var launchVelocityTail = launchDir * (curvedLaunch * tailLaunchRatio);
        */
        
        var launchVelocityHead = bodySegment.transform.forward * (headLaunchRatio * launchSpeed);
        var launchVelocityBody = bodySegment.transform.forward * (bodyLaunchRatio * launchSpeed);
        var launchVelocityTail = tailSegment.transform.forward * (tailLaunchRatio * launchSpeed);
        
        
        Debug.Log("Launch Velocity Head: " + launchVelocityHead);
        
       Vector3 jumpVel = Vector3.up * upwardForce;
        
        
        
        var finalLaunchVelocityTail = launchVelocityTail + (jumpVel * tailLaunchRatio);
        
        //Vector3 headDistanceToTail = (headSegment.position - tailSegment.position).normalized + finalLaunchVelocityTail;
       // Vector3 bodyDistanceToTail = (bodySegment.position - tailSegment.position).normalized + finalLaunchVelocityTail;
        
        var finalLaunchVelocityHead = launchVelocityHead + headSegment.transform.forward * (headForwardLaunch * headLaunchRatio) + (jumpVel * headLaunchRatio);
        var finalLaunchVelocityBody = launchVelocityBody +  bodySegment.transform.forward * bodyLaunchRatio + (jumpVel * bodyLaunchRatio);
        
        
        
        Debug.Log("Final Launch Velocitty Head: " + finalLaunchVelocityHead);
        
        Debug.Log("Final Launch Velocitty Body: " + finalLaunchVelocityBody);
       
        headSegment.AddForce(finalLaunchVelocityHead + (cachedSwingVelocity * headLaunchRatio), ForceMode.VelocityChange);
        bodySegment.AddForce(finalLaunchVelocityBody, ForceMode.VelocityChange);
        bodySegment.AddForce(finalLaunchVelocityTail + (cachedSwingVelocity * tailLaunchRatio), ForceMode.VelocityChange);
        
        

        
        hookReference = null;
        hookPoint = null;

        StartCoroutine(ResetSwing());



        // PlayerReferenceManager.instance.SetState(PlayerState.Locomotion);

        // Apply forces



        // ResetSwing();
    }
    


    private IEnumerator ResetSwing()
    {
        yield return new WaitForSeconds(airTime);
        
        var sequence = DOTween.Sequence();
        sequence.Append(headSegment.transform.DOMove(
            bodySegment.transform.position + bodySegment.transform.forward *
            (PlayerReferenceManager.instance.bodyOffset + PlayerReferenceManager.instance.bodyOffset), downTime));
        sequence.Join(tailSegment.transform.DOMove(
            bodySegment.transform.position - bodySegment.transform.forward * PlayerReferenceManager.instance.tailOffset,
            downTime));


        sequence.OnComplete(() => { PlayerReferenceManager.instance.SetState(PlayerState.Locomotion); });


        //headSegment.isKinematic = true;
    }


    private bool HasHitSomething(Collider segmentCollider)
    {
        if (Physics.CheckSphere(segmentCollider.attachedRigidbody.position,
                segmentCollider.bounds.extents.x + collisionDistance,
                PlayerReferenceManager.instance.playerCollisionMask)) return true;

        return false;
    }

   
}