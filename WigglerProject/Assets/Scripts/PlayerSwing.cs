using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerSwing : MonoBehaviour
{
    public bool isSwinging;
    public Rigidbody hookPoint;


    //supposed to be 120
    [SerializeField] public float swingLimitMin;
    [SerializeField] public float swingLimitMax;

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


    [SerializeField] public float swingDamp;

    //[SerializeField] private Vector3 headTargetRotation = new(-90f, 0f, 0f);

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

    private SpringJoint hookJoint;


    [SerializeField] public float swingSpringStrength;
    [SerializeField] public float swingSpringDamper;
    [SerializeField] public float swingSpringMassScale;


    private Vector3 originalHookPosition;
    private Rigidbody hookRigidBody;

    public float dotProduct;


    public float swingVelocityTime = 5f;
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
        hookRigidBody = hookReference.hookRigidbody;
        originalHookPosition = hookReference.originPosition;

        hookReference.OnValidate();

        var direction = ( hookRigidBody.position -headSegment.transform.position).normalized;
        
         dotProduct = Vector3.Dot(direction, hookRigidBody.transform.forward);
        
         //headSegment.isKinematic = true;
        headSegment.transform.forward = hookRigidBody.transform.forward * Mathf.Sign(dotProduct);
      
        

        var hookSequence = DOTween.Sequence();


        headClosePos = headClose.localPosition;
        bodyOpenPos = bodyOpen.localPosition;
        bodyClosePos = bodyClose.localPosition;
        tailOpenPos = tailOpen.localPosition;
        
        
        
        bodySegment.transform.parent = headSegment.transform;
        tailSegment.transform.parent = headSegment.transform;
        headSegment.transform.parent = hookReference.transform;

        float distance = Vector3.Distance(bodySegment.position, hookRigidBody.position);


        hookSequence.Append(headSegment.transform.DOMove(hookRigidBody.transform.position, swingSetUpDuration));

        //hookSequence.Join(headSegment.transform.DORotate(headSegment.rotation.eulerAngles + headTargetRotation, swingSetUpDuration));
        //headSegment.transform.SetParent(hookPoint, true);


        var targetBodyPosition =
            hookRigidBody.transform.position + (Vector3.down * PlayerReferenceManager.instance.bodyOffset);
        
       

        var targetTailPosition = targetBodyPosition + Vector3.down * PlayerReferenceManager.instance.tailOffset;
        hookSequence.Join(bodySegment.transform.DOMove(targetBodyPosition, swingSetUpDuration)).SetEase(Ease.OutBounce);


        hookSequence.Join(tailSegment.transform.DOMove(targetTailPosition, swingSetUpDuration)).SetEase(Ease.OutBounce);


        hookSequence.OnComplete(() =>
        {
            bodySegment.transform.position = new Vector3(headSegment.transform.position.x,
                bodySegment.transform.position.y, headSegment.transform.position.z);
            bodySegment.transform.rotation = hookRigidBody.rotation;


            tailSegment.transform.position = new Vector3(headSegment.transform.position.x,
                tailSegment.transform.position.y, headSegment.transform.position.z);
            tailSegment.transform.rotation = hookRigidBody.rotation;
            
            swingDirection = headSegment.transform.forward;

            hookRigidBody.isKinematic = false;
            hookRigidBody.useGravity = true;
            hookJoint = hookRigidBody.gameObject.AddComponent<SpringJoint>();
            

            hookJoint.autoConfigureConnectedAnchor = false;
            hookJoint.connectedAnchor = hookPoint.position;

            hookJoint.axis = headSegment.transform.right;

            hookRigidBody.linearDamping = swingDamp;
            

            hookJoint.spring = swingSpringStrength;
            hookJoint.damper = swingSpringDamper;
            hookJoint.massScale = swingSpringMassScale;

            hookJoint.minDistance = swingLimitMin;
            
            hookJoint.maxDistance = swingLimitMax;
            
            
            

            //headSegment.isKinematic = f;





            StartCoroutine(SwingTimer());
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

    IEnumerator SwingTimer()
    {
        while (isSwinging && isHeldDown && !PlayerReferenceManager.instance.hasSwungForAwhile)
        {
            yield return new WaitForSeconds(swingVelocityTime);
            PlayerReferenceManager.instance.hasSwungForAwhile = true;
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
                //hookRigidBody.AddForce(PlayerReferenceManager.instance.gravity * Vector3.up, ForceMode.Acceleration);

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
        headSegment.MovePosition(hookRigidBody.transform.position);
        
        bodySegment.MovePosition(hookRigidBody.transform.position -
                                 hookRigidBody.transform.up * PlayerReferenceManager.instance.bodyOffset);

        tailSegment.MovePosition(hookRigidBody.transform.position -
                                 hookRigidBody.transform.up * (PlayerReferenceManager.instance.bodyOffset + PlayerReferenceManager.instance.tailOffset));


        Vector3 ropeDir = (hookRigidBody.position - hookPoint.position).normalized;

        Vector3 tangent = Vector3.Cross(ropeDir, hookRigidBody.transform.right).normalized;

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
        
        hookRigidBody.AddForce(swingDirection * (input * swingSpeed), ForceMode.Acceleration);
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
       
        PlayerReferenceManager.instance.hasSwungForAwhile = false;
        cachedSwingVelocity = hookRigidBody.linearVelocity;

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
        
        var launchVelocityHead = hookRigidBody.transform.forward * (headLaunchRatio * launchSpeed);
        var launchVelocityBody = hookRigidBody.transform.forward * (bodyLaunchRatio * launchSpeed);
        var launchVelocityTail = hookRigidBody.transform.forward * (tailLaunchRatio * launchSpeed);
        
        
     
        
       Vector3 jumpVel = Vector3.up * upwardForce;
        
        
        
        
        
        //Vector3 headDistanceToTail = (headSegment.position - tailSegment.position).normalized + finalLaunchVelocityTail;
       // Vector3 bodyDistanceToTail = (bodySegment.position - tailSegment.position).normalized + finalLaunchVelocityTail;
        
        var finalLaunchVelocityHead = launchVelocityHead + headSegment.transform.forward * (headForwardLaunch * headLaunchRatio) + (jumpVel * headLaunchRatio);
        var finalLaunchVelocityBody = launchVelocityBody +  bodySegment.transform.forward * bodyLaunchRatio + (jumpVel * bodyLaunchRatio);
        var finalLaunchVelocityTail = launchVelocityTail + bodySegment.transform.forward * tailLaunchRatio + (jumpVel * tailLaunchRatio);
        
        
 
       
        headSegment.AddForce(finalLaunchVelocityHead * cachedSwingVelocity.magnitude * swingInput.magnitude , ForceMode.VelocityChange);
       bodySegment.AddForce(finalLaunchVelocityBody * cachedSwingVelocity.magnitude * swingInput.magnitude , ForceMode.VelocityChange);
        tailSegment.AddForce(finalLaunchVelocityTail * cachedSwingVelocity.magnitude * swingInput.magnitude, ForceMode.VelocityChange);
        
        
        
        
        hookReference = null;
        hookPoint = null;

        StartCoroutine(ResetSwing());



        // PlayerReferenceManager.instance.SetState(PlayerState.Locomotion);

        // Apply forces



        // ResetSwing();
    }
    


    private IEnumerator ResetSwing()
    {
       Destroy(hookJoint);
        yield return new WaitForSeconds(airTime);
        
        
        bodySegment.transform.parent = PlayerReferenceManager.instance.transform;
        tailSegment.transform.parent = PlayerReferenceManager.instance.transform;
        headSegment.transform.parent = PlayerReferenceManager.instance.transform;
        hookRigidBody.linearVelocity = Vector3.zero;
        hookRigidBody.isKinematic = true;
        hookRigidBody.useGravity = false;
        var sequence = DOTween.Sequence();
        /*sequence.Append(headSegment.transform.DOMove(
            bodySegment.transform.position + bodySegment.transform.forward *
            (PlayerReferenceManager.instance.bodyOffset + PlayerReferenceManager.instance.bodyOffset), downTime));
        sequence.Join(tailSegment.transform.DOMove(
            bodySegment.transform.position - bodySegment.transform.forward * PlayerReferenceManager.instance.tailOffset,
            downTime));*/
        sequence.Append(hookRigidBody.DOMove(originalHookPosition, downTime));

        
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