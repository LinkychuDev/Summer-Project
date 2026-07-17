using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RotaryHeart.Lib.PhysicsExtension;
using UnityEngine;
using UnityEngine.InputSystem;
using Physics = UnityEngine.Physics;


public class PlayerStretch : MonoBehaviour
{
    public enum StretchState
    {
        None,
        Stretching,
        Retracting,
        Stuck
    }
    public float stretchDistanceHead;
    public float stretchDistanceTail;
    public float stretchRetractTime = 4f;



    //public float stretchTime = 3f;
    //public bool isStretching;
    //public InputActionReference stretchButton;

    //public InputActionReference stretchInput;
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
    private Transform playerCam;
    [SerializeField] private float stretchSpeed = 10f;
    [SerializeField] private float maxStretchSpeed = 10f;

    [SerializeField] private float desiredPointsPerLine = 6;

    [SerializeField] private Transform headTarget;
    private float bodyOffset;
    private float tailOffset;

    //private float currentStretchDistance;



    [Header("Spring Joint")]
    private SpringJoint springJoint;

    [SerializeField] private float stiffness = 5000f;
    [SerializeField] private float damper = 4f;
    [SerializeField] private float connectedMassStrength = 0.01f;

    [SerializeField] private float pushbackRatio = 0.0001f;
    //spherecast detection
    [Header("Collision Detection")]
    [SerializeField] float detectionRadius = 0.5f;
    [SerializeField] private float maxDetectionDistance = 0.1f;

    public static StretchState stretchState = StretchState.None;

    [SerializeField] private bool shouldStretchForward;

    private PlayerSwing swing;
    private PlayerMovement movement;

    private Vector3 slopeDir;

    bool isOnSlope;
    //spring joint values
    //private SpringJoint headJoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    bool isStretching;
    Collider[] colliders;

    [SerializeField] private SpringVisualConnection springVisualConnection;

    private List<Vector3> positions = new List<Vector3>();

    private void OnEnable()
    {
        InputManager.instance.controls.Gameplay.Stretch.started += ctx => StartCoroutine(OnPlayerStretchedEvent());
        InputManager.instance.controls.Gameplay.Stretch.canceled += ctx => OnPlayerRetracted();
    }


    void OnDisable()
    {
        InputManager.instance.controls.Gameplay.Stretch.started -= ctx => StartCoroutine(OnPlayerStretchedEvent());
        InputManager.instance.controls.Gameplay.Stretch.canceled -= ctx => OnPlayerRetracted();
    }
    void Start()
    {
        playerCam = Camera.main.transform;
        colliders = new Collider[1];

        bodyOffset = PlayerReferenceManager.instance.bodyOffset;
        tailOffset = PlayerReferenceManager.instance.tailOffset;

        segments = PlayerReferenceManager.instance.segments.ToArray();
        headSegment = segments[0].rb;
        bodySegment = segments[1].rb;
        tailSegment = segments[2].rb;

        swing = GetComponent<PlayerSwing>();

        maxDistanceHead = bodyOffset + stretchDistanceHead;

        movement = GetComponent<PlayerMovement>();

        /*collisionRadius = collisionRadiusOffset + headSegment.GetComponent<SphereCollider>().radius;
        colliders = new Collider[maxColliders];
        collisionMask = PlayerReferenceManager.instance.playerCollisionMask;*/
        //CreateJoint();
        //headSegment.GetComponent<SphereCollider>().radius
        //sphere collision
    }

    void Update()
    {
        if (PlayerReferenceManager.instance.currentState == PlayerState.Locomotion)
            return;
        if (!PlayerReferenceManager.instance.canStretch)
            return;
        moveInput = InputManager.instance.controls.Gameplay.Move.ReadValue<Vector2>();
        isStretching = PlayerReferenceManager.instance.currentState == PlayerState.Stretching;

        //isStretching = stretchButton.action.IsPressed();
        if (stretchState == StretchState.Retracting)
            return;
        if (PlayerReferenceManager.instance.currentState != PlayerState.Stretching)
            return;






    }

    void HandleStretchInput()
    {


        Vector3 forward = playerCam.transform.forward;
        Vector3 right = playerCam.transform.right;
        forward.y = 0;
        forward.Normalize();
        right.y = 0;
        right.Normalize();

        //float stretchValue = 1;

        stretchDirection = moveInput.x * right + moveInput.y * forward;
        stretchDirection.y = 0;

    }


    // Update is called once per frame
    void FixedUpdate()
    {


        if (PlayerReferenceManager.instance.currentState != PlayerState.Stretching)
            return;

        if (!PlayerReferenceManager.instance.canStretch)
            return;
        switch (stretchState)
        {
            case StretchState.Retracting:
                break;
            case StretchState.Stretching:
                HandleStretchInput();
                SlopeDetection();
                SteerEvent();
                StretchEvent();
                CollisionDetection();
                break;


        }
    }


    void SlopeDetection()
    {
        isOnSlope = movement.IsOnSlope(stretchDirection, ref slopeDir);


    }


    void OnTriggerEnter(Collider other)
    {
        if(stretchState == StretchState.Retracting)
            return;
        
        if(isStretching)
        {
            if (other.CompareTag("Honey"))
            {
                if (other.gameObject.TryGetComponent(out HoneySwingTest honeyTest))
                {
                    //honeyTest.DisableCollisions();
                    DestroySpringJoint();

                    PlayerReferenceManager.instance.SetState(PlayerState.Swinging);

                    //replace with events
                    swing.StartSwing(honeyTest);
                }
            }
        }
    }



    void CollisionDetection()
    {
        if(stretchState == StretchState.Retracting)
            return;
        if (Physics.OverlapSphereNonAlloc(headSegment.position, detectionRadius,  colliders, PlayerReferenceManager.instance.playerCollisionMask) > 0)
        {
            var other = colliders[0].transform;
            if(colliders[0].TryGetComponent(out Rigidbody rb))
            {
                 if (rb.isKinematic)
                {
                    shouldStretchForward = true;
                }

                else
                {
                   shouldStretchForward = false;
                }
                        
            }

            else
            {
                if(colliders[0].gameObject.isStatic)
                {
                    shouldStretchForward = true;
                }
            }

        }

        else
        {
            shouldStretchForward = false;
        }
    }

    bool CanStretch()
    {

        if (PlayerReferenceManager.instance.canStretch)
        {
            if (PlayerReferenceManager.instance.isGrounded)
            {
                return true;
            }

            else if (PlayerReferenceManager.instance.isOnCoyoteTime)
            {
                return true;
            }

            return false;
        }




        return false;
    }

    void ShouldStretchForward(bool shouldStretch)
    {
        if (shouldStretch)
        {
            stretchState = StretchState.Stuck;

        }
    }

    IEnumerator OnPlayerStretchedEvent()
    {

        if (!CanStretch())
            yield break;
        //headSegment.isKinematic = true;

        if (stretchState != StretchState.None)
            yield break;

        if (!PlayerReferenceManager.instance.canStretch)
            yield break;

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

        if (headSegment.TryGetComponent(out SpringJoint joint))
        {
            Destroy(joint);
        }

        springJoint = headSegment.gameObject.AddComponent<SpringJoint>();
        springJoint.connectedBody = bodySegment;
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.connectedAnchor = Vector3.zero;
        springJoint.minDistance = 0;
        springJoint.maxDistance = stretchDistanceHead;
        springJoint.spring = stiffness;
        springJoint.connectedMassScale = connectedMassStrength;
        springJoint.damper = damper;
        bodySegment.isKinematic = true;
        PlayerReferenceManager.instance.SetState(PlayerState.Stretching);
        stretchState = StretchState.Stretching;
        //cachedStretchPositions.Add(headSegment.transform);

    }




    void StretchEvent()
    {

        Debug.Log("Event Called");
        //take stretching position


        //  currentStretchDistance = Mathf.Lerp(minDistanceHead, maxDistanceHead, currentStretchTime );



        Vector3 targetDirection = stretchDirection.normalized;


        if (isOnSlope)
        {
            targetDirection = slopeDir;
        }
        Vector3 targetVelocity = targetDirection * stretchSpeed;







        Vector3 currentVelocity = headSegment.linearVelocity;


        currentVelocity.y = 0;
        Vector3 velocityChange = targetVelocity - currentVelocity;

        velocityChange = Vector3.ClampMagnitude(velocityChange, maxStretchSpeed);

        headSegment.AddForce(velocityChange, ForceMode.VelocityChange);





        /*Vector3 constrainedOffset = headSegment.position - bodySegment.transform.position;
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



        }*/

    }

    void SteerEvent()
    {


        //rotate around middle segment position
        if (moveInput.magnitude > 0.01f)
        {
            if (stretchState == StretchState.Stretching)
            {
                Quaternion targetRotation;

                if (isOnSlope)
                {
                    targetRotation = Quaternion.LookRotation(slopeDir, Vector3.up);
                }

                else
                {
                    targetRotation = Quaternion.LookRotation(stretchDirection.normalized, Vector3.up);
                }
                headSegment.transform.rotation = Quaternion.Slerp(headSegment.transform.rotation, targetRotation,
                    stretchTurnSpeed * Time.fixedDeltaTime);
            }

        }



    }


    void OnPlayerRetracted()
    {
        if (PlayerReferenceManager.instance.currentState != PlayerState.Stretching)
            return;
        Debug.Log(stretchState);
        DestroySpringJoint();

        StartCoroutine(OnPlayerRetractedEvent());
    }

    private void DestroySpringJoint()
    {
        if (springJoint != null)
        {
            Destroy(springJoint);
            springJoint = null;
        }
    }

    public IEnumerator OnPlayerRetractedEvent()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log(stretchState);
        //yield return new WaitUntil(() => PlayerPlayerReferenceManager.instance.state == PlayerState.Stretching);
        stretchState = StretchState.Retracting;


        currentStretchTime = 0;

        tailSegment.transform.LookAt(bodySegment.transform.position + bodySegment.transform.forward);
        bodySegment.isKinematic = false;
        tailSegment.isKinematic = false;


        yield return new WaitForFixedUpdate();

        Sequence stretchSequence = DOTween.Sequence();

        List<Vector3> listOfPositions = new List<Vector3>();


        var stretchPositions = springVisualConnection.GetPositions();

        int amount = stretchPositions.Length;

        ;
        if (!shouldStretchForward)
        {
            //Vector3 targetHeadDir = (bodySegment.transform.position + bodySegment.transform.forward) -headSegment.transform.position;
            //Vector3 targetHeadPosition = bodySegment.transform.position + (bodySegment.transform.forward * bodyOffset);




            for (int i = amount - 1; i > -1; i--)
            {
                listOfPositions.Add(stretchPositions[i]);
            }

            listOfPositions.Add(bodySegment.position + (bodySegment.transform.forward * bodyOffset));

            positions = listOfPositions;





            stretchSequence.Append(headSegment.DOPath(listOfPositions.ToArray(), stretchRetractTime,
                PathType.CatmullRom, PathMode.Full3D)).SetEase(Ease.OutQuad);



        }

        else
        {

            for (int i = 0; i < amount; i++)
            {
                listOfPositions.Add(stretchPositions[i]);
            }


            listOfPositions.Add(headSegment.position - (headSegment.transform.forward * bodyOffset));


            List<Vector3> tailPositions = listOfPositions;

            for (int i = 0; i < tailPositions.Count; i++)
            {
                var pos = tailPositions[i];
                tailPositions[i] = pos - (tailOffset * bodySegment.transform.forward);
            }


            stretchSequence.Append(bodySegment.DOPath(listOfPositions.ToArray(), stretchRetractTime,
                PathType.CatmullRom, PathMode.Full3D)).SetEase(Ease.OutQuad); ;

            stretchSequence.Join(tailSegment.DOPath(tailPositions.ToArray(), stretchRetractTime,
                PathType.CatmullRom, PathMode.Full3D)).SetEase(Ease.OutQuad); ;


            /*Sequence stretchSequence = DOTween.Sequence();
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
            yield return new WaitForFixedUpdate();*/
        }




        yield return stretchSequence.WaitForCompletion();

        //add effect
        //yield return new WaitForFixedUpdate();
        stretchState = StretchState.None;
        shouldStretchForward = false;
        PlayerReferenceManager.instance.Honeyfied(false);
        PlayerReferenceManager.instance.SetState(PlayerState.Locomotion);




        //yield return null;

    }



    private void OnDrawGizmosSelected()
    {


        if (!Application.isPlaying)
            return;
        Gizmos.color = Color.aliceBlue;

        //Gizmos.DrawWireSphere(headSegment.position, detectionRadius);
        Gizmos.DrawLine(headSegment.position, headSegment.position + headSegment.transform.forward * maxDetectionDistance);
        if (springJoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(springJoint.connectedAnchor), 0.3f);

            Gizmos.color = Color.blueViolet;
            Gizmos.DrawWireSphere(transform.TransformPoint(springJoint.anchor), 0.3f);
        }
        if (positions.Count == 0)
            return;
        Gizmos.color = Color.cyan;

        for (int i = 0; i < positions.Count; i++)
        {
            Gizmos.DrawWireSphere(positions[i], PlayerReferenceManager.instance.distanceRadius);
        }

    }
}
