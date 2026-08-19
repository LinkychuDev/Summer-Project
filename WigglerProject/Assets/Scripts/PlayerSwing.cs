using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerSwing : MonoBehaviour
{
    public bool isSwinging;
    public Transform hookPoint;


    [SerializeField] private float swingLimit = 95;

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

    //[SerializeField] private InputActionReference swingHeldInputReference;

    
    [SerializeField] private float headForwardLaunch = 4;
    private HoneySwingTest hookReference;

    public Vector3 swingVelocity;

    [SerializeField] private float swingAcceleration;
    //Vector3 accumulatedAngularVelocity;

    [SerializeField] private float headLaunchRatio, bodyLaunchRatio, tailLaunchRatio;

    private float cachedDampingBody, cachedDampingBodyAngular, cachedDampingTail, cachedDampingTailAngular;
    private bool isHeldDown;
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


            //bodySegment.AddForce(swingSpeed * , ForceMode.VelocityChange);
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
            isHeldDown = InputManager.instance.controls.Gameplay.Stretch.ReadValue<float>() > 0;
            if (isHeldDown)
            {
                SwingEvent();
            }

            else
            {
                ReleaseEvent();
            }
        }

        
    }

    private void SwingEvent()
    {
        var input = swingInput.y;

        swingMagnitude = Mathf.Abs(input);

        // Head stays fixed at hook point
        headSegment.transform.position = hookPoint.position;

        swingDirection = -headSegment.transform.right;


        // Rope direction (from head to body)
        var ropeDir = (bodySegment.transform.position - headSegment.transform.position).normalized;

        // Tangent direction (direction of swing)
        var tangent = Vector3.Cross(swingDirection, ropeDir).normalized;

        // Apply input acceleration
        swingVelocity += tangent * (input * swingSpeed * Time.deltaTime);

        // Apply gravity projected onto tangent
        var gravityAlongTangent = Vector3.Dot(PlayerReferenceManager.instance.gravity * Vector3.up, tangent);
        swingVelocity += tangent * (gravityAlongTangent * Time.deltaTime);

        
        // Apply damping
        swingVelocity *= swingDamp;

        // Move body
        var newBodyPos = bodySegment.transform.position + swingVelocity * Time.deltaTime;


        // Compute current angle
        // Compute current angle
        var angle = Vector3.SignedAngle(Vector3.down, (newBodyPos - headSegment.transform.position).normalized,
            swingDirection);


        var gravityForce = -Mathf.Sin(angle * Mathf.Deg2Rad) * (PlayerReferenceManager.instance.gravity);
        swingVelocity += tangent * (gravityForce * Time.deltaTime);

        // Only clamp when angle EXCEEDS limits
        if (angle > swingLimit)
            angle = swingLimit;
        else if (angle < -swingLimit) angle = -swingLimit;

        // Reconstruct ropeDir ONLY when clamped
        var rot = Quaternion.AngleAxis(angle, swingDirection);
        var ropeDirClamped = rot * Vector3.down;

        // Enforce rope length
        newBodyPos = headSegment.transform.position + ropeDirClamped * swingLength;


        // Apply final body position
        bodySegment.transform.position = newBodyPos;


        // Tail follows body
        tailSegment.transform.position = bodySegment.transform.position +
                                         -bodySegment.transform.up * PlayerReferenceManager.instance.tailOffset;

        // Rotate body toward swing direction
        var bodyRot = Quaternion.LookRotation(tangent, Vector3.up);

        bodySegment.transform.rotation =
            Quaternion.Slerp(bodySegment.transform.rotation, bodyRot,
                Time.deltaTime * swingTurnSpeed); // tweak rotation speed);

        // Rotate tail to follow body
        var tailRot = Quaternion.LookRotation(tangent, Vector3.up);
        tailSegment.transform.rotation =
            Quaternion.Slerp(tailSegment.transform.rotation, tailRot, Time.deltaTime * swingTurnSpeed);
        
        
      
    }


    private void ReleaseEvent()
    {
        if (hookReference == null)
            return;
        isSwinging = false;
        Debug.Log("Triggered");
        isSwinging = false;


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


        var launchVelocityHead = swingDirection * headLaunchRatio;
        var launchVelocityBody = swingDirection * bodyLaunchRatio;
        var launchVelocityTail = swingDirection * tailLaunchRatio;
        
        var jumpVel = Mathf.Sqrt(-2 * PlayerReferenceManager.instance.gravity * upwardForce);


        var finalLaunchVelocityHead = launchVelocityHead + tailSegment.transform.up * (jumpVel * headLaunchRatio);
        var finalLaunchVelocityBody = launchVelocityBody + tailSegment.transform.up * (jumpVel * bodyLaunchRatio);
        var finalLaunchVelocityTail = launchVelocityTail + tailSegment.transform.up * (jumpVel * tailLaunchRatio);
        
        
        Debug.Log("Final Launch Velocitty Head: " + finalLaunchVelocityHead);
        PlayerReferenceManager.instance.launched = true;
        
        
        
        headSegment.AddForce(finalLaunchVelocityHead * swingMagnitude, ForceMode.VelocityChange);
        bodySegment.AddForce(finalLaunchVelocityBody * swingMagnitude, ForceMode.VelocityChange);
        bodySegment.AddForce(finalLaunchVelocityTail * swingMagnitude, ForceMode.VelocityChange);
        
        
        

        headClose.localPosition = new Vector3(headClosePos.x, headClosePos.y, headClosePos.z);
        bodyOpen.localPosition = new Vector3(bodyOpenPos.x, bodyOpenPos.y, bodyOpenPos.z);
        bodyClose.localPosition = new Vector3(bodyClosePos.x, bodyClosePos.y, bodyClosePos.z);
        tailOpen.localPosition = new Vector3(tailOpenPos.x, tailOpenPos.y, tailOpenPos.z);
        
        hookReference = null;
        hookPoint = null;
 

        
        
        
        PlayerReferenceManager.instance.SetState(PlayerState.Locomotion);

        // Apply forces
        
        
        
        // ResetSwing();
    }
    


    private void ResetSwing()
    {
        headClose.localPosition = new Vector3(headClosePos.x, headClosePos.y, headClosePos.z);
        bodyOpen.localPosition = new Vector3(bodyOpenPos.x, bodyOpenPos.y, bodyOpenPos.z);
        bodyClose.localPosition = new Vector3(bodyClosePos.x, bodyClosePos.y, bodyClosePos.z);
        tailOpen.localPosition = new Vector3(tailOpenPos.x, tailOpenPos.y, tailOpenPos.z);


        bodySegment.transform.forward = Vector3.right;
        tailSegment.transform.forward = Vector3.right;


        hookReference = null;
        hookPoint = null;
        
        var sequence = DOTween.Sequence();
        sequence.Append(headSegment.transform.DOMove(
            tailSegment.transform.position + tailSegment.transform.forward *
            (PlayerReferenceManager.instance.bodyOffset + PlayerReferenceManager.instance.tailOffset), downTime));
        sequence.Join(bodySegment.transform.DOMove(
            tailSegment.transform.position + tailSegment.transform.forward * PlayerReferenceManager.instance.bodyOffset,
            downTime));
        sequence.Play();


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