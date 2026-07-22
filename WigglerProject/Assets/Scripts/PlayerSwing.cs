using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerSwing : MonoBehaviour
{

    public bool isSwinging;
    public Transform hookPoint;



    [SerializeField] private float swingLimit = 95;

    [SerializeField] private float swingSetUpDuration = 0.2f;

    [SerializeField] private float swingLength = 3f;




    private float currentSwingAngle;

    CharacterController headSegment;
    CharacterController bodySegment;
    CharacterController tailSegment;

    //public InputActionReference swingInputReference;


    public Vector2 swingInput;





    Vector3 swingDirection;


    [SerializeField] private float swingSpeed = 30f;


    [SerializeField] private float swingDamp;

    [SerializeField] Vector3 headTargetRotation = new Vector3(-90f, 0f, 0f);

    [SerializeField] private float launchSpeed;

    float lastSwingAngle;
    [SerializeField] bool useGravity = true;

    //[SerializeField] private InputActionReference swingHeldInputReference;

    private HoneySwingTest hookReference;

    Vector3 swingVelocity;

    [SerializeField] private float swingAcceleration;
    //Vector3 accumulatedAngularVelocity;

    [SerializeField] private float autoCenterStrength = 40f;

    [SerializeField] private float headLaunchRatio, bodyLaunchRatio, tailLaunchRatio;

    float cachedDampingBody, cachedDampingBodyAngular, cachedDampingTail, cachedDampingTailAngular;
    private bool isHeldDown;
    [SerializeField] private float upwardForce;
    [SerializeField] private float downTime;


    [SerializeField] private float airTime = 0.3f;
    // private bool enterSwingRecoveryMode;
    [SerializeField] private float swingTurnSpeed = 10f;

    [Header("Collision Detection")]
    //public float groundCheckDistance;
    public float collisionDistance = 0.01f;
    Collider tailCollider;
    Collider headCollider;
    Collider bodyCollider;


    [SerializeField] private Transform bodyOpen;
    [SerializeField] private Transform headClose;
    [SerializeField] private Transform bodyClose;
    [SerializeField] private Transform tailOpen;
    Vector3 headClosePos;
    Vector3 bodyOpenPos;
    Vector3 bodyClosePos;
    Vector3 tailOpenPos;






    [Header("Parabola")]
    [SerializeField] private float maxHeight;
    [SerializeField] private float minHeight;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private int parabolaPoints;
    [SerializeField] private LineRenderer trajectoryLine;
    private float parabolaHeight;
    [SerializeField] private float curveLength;
    List<Vector3> pathPoints = new List<Vector3>();

    void Start()
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
        trajectoryLine.positionCount = parabolaPoints;
        trajectoryLine.enabled = true;

        //make this a sequence
        //stretchState = StretchState.Swinging;
        //Vector3 anchorPoint = hook.transform.position - hook.swingAnchor;


        hookReference = hook;
        hookPoint = hookReference.swingAnchor;
        hookPoint.transform.localPosition = Vector3.zero;


        Vector3 direction = (headSegment.transform.position - hookPoint.position).normalized;
        //headSegment.isKinematic = true;
        headSegment.transform.rotation = hookPoint.rotation;
        headSegment.transform.position = hookPoint.position;

        headCollider = headSegment.GetComponent<SphereCollider>();
        bodyCollider = bodySegment.GetComponent<SphereCollider>();
        tailCollider = tailSegment.GetComponent<SphereCollider>();






        Sequence hookSequence = DOTween.Sequence();


        headClosePos = headClose.localPosition;
        bodyOpenPos = bodyOpen.localPosition;
        bodyClosePos = bodyClose.localPosition;
        tailOpenPos = tailOpen.localPosition;

        pathPoints.Clear();


        hookSequence.Append(headSegment.transform.DOMove(hookPoint.transform.position, swingSetUpDuration));

        //hookSequence.Join(headSegment.transform.DORotate(headSegment.rotation.eulerAngles + headTargetRotation, swingSetUpDuration));
        //headSegment.transform.SetParent(hookPoint, true);

      


        Vector3 targetBodyPosition =
            hookPoint.position + (swingLength * (Vector3.down * (PlayerReferenceManager.instance.bodyOffset)));

        Vector3 targetTailPosition = targetBodyPosition + (Vector3.down * (PlayerReferenceManager.instance.tailOffset));
        hookSequence.Join(bodySegment.transform.DOMove(targetBodyPosition, swingSetUpDuration)).SetEase(Ease.OutBounce);


        hookSequence.Join(tailSegment.transform.DOMove(targetTailPosition, swingSetUpDuration)).SetEase(Ease.OutBounce);


        hookSequence.OnComplete(() =>
        {



            bodySegment.transform.position = new Vector3(headSegment.transform.position.x, bodySegment.transform.position.y, headSegment.transform.position.z);
            bodySegment.transform.rotation = hookPoint.rotation;


            tailSegment.transform.position = new Vector3(headSegment.transform.position.x, tailSegment.transform.position.y, headSegment.transform.position.z);
            tailSegment.transform.rotation = hookPoint.rotation;

            






            //bodySegment.AddForce(swingSpeed * , ForceMode.VelocityChange);
            isSwinging = true;



        });


        //swingLength = Mathf.Abs(swingLength);


    }


    


    private void Update()
    {
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
                StartCoroutine(ReleaseEvent());
            }
        }


    }

    void SwingEvent()
    {


        float input = swingInput.y;

        // Head stays fixed at hook point
        headSegment.transform.position = hookPoint.position;

        swingDirection = -headSegment.transform.right;


        // Rope direction (from head to body)
        Vector3 ropeDir = (bodySegment.transform.position - headSegment.transform.position).normalized;

        // Tangent direction (direction of swing)
        Vector3 tangent = Vector3.Cross(swingDirection, ropeDir).normalized;

        // Apply input acceleration
        swingVelocity += tangent * (input * swingSpeed * Time.deltaTime);

        // Apply gravity projected onto tangent
        float gravityAlongTangent = Vector3.Dot(PlayerReferenceManager.instance.gravity * Vector3.up, tangent);
        swingVelocity += tangent * (gravityAlongTangent * Time.deltaTime);

        // Apply damping
        swingVelocity *= swingDamp;

        // Move body
        Vector3 newBodyPos = bodySegment.transform.position + swingVelocity * Time.deltaTime;



        // Compute current angle
        // Compute current angle
        float angle = Vector3.SignedAngle(Vector3.down, (newBodyPos - headSegment.transform.position).normalized, swingDirection);


        float gravityForce = -Mathf.Sin(angle * Mathf.Deg2Rad) * Mathf.Abs(PlayerReferenceManager.instance.gravity);
        swingVelocity += tangent * (gravityForce * Time.deltaTime);

        // Only clamp when angle EXCEEDS limits
        if (angle > swingLimit)
        {
            angle = swingLimit;
        }
        else if (angle < -swingLimit)
        {
            angle = -swingLimit;
        }

        // Reconstruct ropeDir ONLY when clamped
        Quaternion rot = Quaternion.AngleAxis(angle, swingDirection);
        Vector3 ropeDirClamped = rot * Vector3.down;

        // Enforce rope length
        newBodyPos = headSegment.transform.position + ropeDirClamped * swingLength;


        // Apply final body position
        bodySegment.transform.position = newBodyPos;



        // Tail follows body
        tailSegment.transform.position = bodySegment.transform.position + (-bodySegment.transform.up * PlayerReferenceManager.instance.tailOffset);

        // Rotate body toward swing direction
        Quaternion bodyRot = Quaternion.LookRotation(tangent, Vector3.up);

        bodySegment.transform.rotation =
            Quaternion.Slerp(bodySegment.transform.rotation, bodyRot, Time.deltaTime * swingTurnSpeed);// tweak rotation speed);

        // Rotate tail to follow body
        Quaternion tailRot = Quaternion.LookRotation(tangent, Vector3.up);
        tailSegment.transform.rotation = Quaternion.Slerp(tailSegment.transform.rotation, tailRot, Time.deltaTime * swingTurnSpeed);


    }



    IEnumerator ReleaseEvent()
    {

        if (hookReference == null)
            yield break;
        isSwinging = false;
        Debug.Log("Triggered");
        isSwinging = false;
    

        PlayerReferenceManager.instance.launched = true;

        Vector3 launchDir = swingVelocity.normalized;

        // Mario forward bias
        launchDir = (launchDir + swingDirection * 0.3f).normalized;

        // Mario upward boost
        Vector3 jumpVel = Vector3.up * (upwardForce * 1.5f);

        // Mario curved velocity
        float curvedLaunch = Mathf.Pow(swingVelocity.magnitude, 0.85f) * launchSpeed;

        // Apply forces
        bodySegment.Move(launchDir * curvedLaunch * bodyLaunchRatio * Time.deltaTime);
        tailSegment.Move(launchDir * curvedLaunch * tailLaunchRatio * Time.deltaTime);

        bodySegment.Move(jumpVel * bodyLaunchRatio * Time.deltaTime);
        tailSegment.Move(jumpVel * tailLaunchRatio * Time.deltaTime);


        yield return new WaitForSeconds(airTime);
        ResetSwing();

    }

    public Vector3 ParabolaCalculator(Vector3 start, Vector3 end, float timeStep)
    {
        Vector3 linePoint = Vector3.Lerp(start, end, timeStep);

        float parabola = -4f * minHeight * (timeStep * timeStep - timeStep);

        linePoint.y += parabola;

        return linePoint;
    }



    void ResetSwing()
    {

        headClose.localPosition = new Vector3(headClosePos.x, headClosePos.y, headClosePos.z);
        bodyOpen.localPosition = new Vector3(bodyOpenPos.x, bodyOpenPos.y, bodyOpenPos.z);
        bodyClose.localPosition = new Vector3(bodyClosePos.x, bodyClosePos.y, bodyClosePos.z);
        tailOpen.localPosition = new Vector3(tailOpenPos.x, tailOpenPos.y, tailOpenPos.z);


        bodySegment.transform.forward = Vector3.right;
        tailSegment.transform.forward = Vector3.right;

    
        hookReference = null;
        hookPoint = null;

        trajectoryLine.enabled = false;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(headSegment.transform.DOMove(tailSegment.transform.position + (tailSegment.transform.forward * (PlayerReferenceManager.instance.bodyOffset + PlayerReferenceManager.instance.tailOffset)), downTime));
        sequence.Join(bodySegment.transform.DOMove(tailSegment.transform.position + (tailSegment.transform.forward * PlayerReferenceManager.instance.bodyOffset), downTime));
        sequence.Play();


        sequence.OnComplete(() =>
        {
            PlayerReferenceManager.instance.SetState(PlayerState.Locomotion);
        });



        




        //headSegment.isKinematic = true;




    }










    bool HasHitSomething(Collider segmentCollider)
    {
        if (Physics.CheckSphere(segmentCollider.attachedRigidbody.position, segmentCollider.bounds.extents.x + (collisionDistance), PlayerReferenceManager.instance.playerCollisionMask))
        {
            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (isSwinging)
        {

            for (int i = 0; i < pathPoints.Count; i++)
            {
                if (i != pathPoints.Count - 1)
                {
                    Gizmos.color = Color.chartreuse;
                    Gizmos.DrawWireSphere(pathPoints[i], 0.2f);
                }

                else
                {
                    Gizmos.color = Color.crimson;
                    Gizmos.DrawWireSphere(pathPoints[i], 0.2f);
                }
            }


            Gizmos.color = Color.slateBlue;
            Gizmos.DrawLine(pathPoints[^1], pathPoints[^1] + (Vector3.down * 10000));
        }
    }
}
