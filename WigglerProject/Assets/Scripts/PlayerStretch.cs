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

    private float stretchVerticalVelocity;

    bool isOnSlope;
    //spring joint values
    //private SpringJoint headJoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    bool isStretching;
    Collider[] colliders;

    [SerializeField] private SpringVisualConnection springVisualConnection;

    private List<Vector3> positions = new List<Vector3>();
    private Vector3 stretchVelocity;

    public bool isMetal;
    private float sturdyRatio;

    private bool isOnHoney;
    private void OnEnable()
    {
        InputManager.instance.controls.Gameplay.Stretch.started += ctx => StartCoroutine(OnPlayerStretchedEvent());
        InputManager.instance.controls.Gameplay.Stretch.canceled += ctx => OnPlayerRetracted();
        PlayerController.isOnSturdyEvent += OnSturdyEvent;
        PlayerController.isOnHoneyEvent += b => isOnHoney = b;
        
    }

    private void OnSturdyEvent(float arg1, bool arg2)
    {
        sturdyRatio = arg1;
        isMetal = arg2;   
    }


    void OnDisable()
    {
        InputManager.instance.controls.Gameplay.Stretch.started -= ctx => StartCoroutine(OnPlayerStretchedEvent());
        InputManager.instance.controls.Gameplay.Stretch.canceled -= ctx => OnPlayerRetracted();
        PlayerController.isOnSturdyEvent -= OnSturdyEvent;
        PlayerController.isOnHoneyEvent -= _ => isOnHoney = false;
    }
    void Start()
    {
        playerCam = Camera.main.transform;
        colliders = new Collider[1];

        bodyOffset = PlayerReferenceManager.instance.bodyOffset;
        tailOffset = PlayerReferenceManager.instance.tailOffset;

        segments = PlayerReferenceManager.instance.segments.ToArray();
        headSegment = segments[0].characterController;
        bodySegment = segments[1].characterController;
        tailSegment = segments[2].characterController;

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

        if (!isStretching)
            return;
        switch (stretchState)
        {
            case StretchState.Retracting:
                break;
            case StretchState.Stretching:
                HandleStretchInput();
                SteerEvent();
                StretchEvent();
                CollisionDetection();
                break;
            case StretchState.Stuck:
                break;


        }




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

        shouldStretchForward = false;
        
        if (Physics.OverlapSphereNonAlloc(headSegment.transform.position, detectionRadius,  colliders, PlayerReferenceManager.instance.playerCollisionMask) > 0)
        {
            var other = colliders[0].transform;
            if (isOnHoney)
            {
                if (other.TryGetComponent(out Rigidbody rb))
                {
                    if (rb.isKinematic)
                    {
                        shouldStretchForward = true;
                    }

                }

                else
                {
                    if (other.gameObject.isStatic)
                    {
                        shouldStretchForward = true;
                    }
                }
            }

            else
            {
                if (other.transform.TryGetComponent(out IStickable stickable))
                {
                    shouldStretchForward = true;
                }
            }
            

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
        


        Debug.Log("OnPlayerStretchedEvent");
        yield return null;
        //bodySegment.MovePosition(headSegment.position - (headSegment.transform.forward *bodyOffset));
        // tailSegment.MovePosition(bodySegment.position - (bodySegment.transform.forward * tailOffset));

        cachedHeadPosition = headSegment.transform.position;
        cachedBodyPosition = bodySegment.transform.position;
        cachedTailPosition = tailSegment.transform.position;
        currentStretchTime = 0;

        
        
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

        
        Vector3 targetVelocity = targetDirection * (stretchSpeed * sturdyRatio);




        Vector3 dirToBody = (headSegment.transform.position - bodySegment.transform.position);

        float distance = dirToBody.magnitude;


        if (distance > maxDistanceHead * sturdyRatio)
        {
            float dotProduct = Vector3.Dot(targetVelocity, dirToBody.normalized);

            if (dotProduct > 0)
            {
                targetVelocity -= dotProduct * dirToBody.normalized;
            }
        }



        if ((PlayerReferenceManager.instance.isGrounded && stretchVerticalVelocity < 0))
        {
            stretchVerticalVelocity = -2f;
        }

        else if(PlayerReferenceManager.instance.isOnSlope) 
        {
            stretchVerticalVelocity +=  PlayerReferenceManager.instance.gravity* (1/sturdyRatio) * Time.deltaTime;
        }

        else
        {
            float yOffset = headSegment.transform.position.y - cachedBodyPosition.y;

            if (yOffset > 0)
            {
                stretchVerticalVelocity += PlayerReferenceManager.instance.gravity * (1/sturdyRatio) * Time.deltaTime;
            }

            else
            {
                stretchVerticalVelocity = Mathf.Max(stretchVerticalVelocity, 0f);
            }
        }
        


        
        stretchVelocity = targetVelocity + Vector3.up * (stretchVerticalVelocity );
        
        
        
        
        headSegment.Move(stretchVelocity * Time.deltaTime);


        /*if (headSegment.transform.position.y < cachedBodyPosition.y)
        {
            float yOffset = headSegment.transform.position.y - cachedBodyPosition.y;
            
            headSegment.Move(Vector3.up * yOffset);
            
        }*/

    }

    private bool GetSlopeInfo(out RaycastHit hit)
    {
        float radius = detectionRadius;
        Vector3 origin = headSegment.transform.position + Vector3.up * 0.1f;

        if (Physics.SphereCast(origin, radius, Vector3.down, out hit,
                maxDetectionDistance + radius, PlayerReferenceManager.instance.groundMask))
        {
            return true;
        }

        return false;
    }
    
    private bool IsDownhillSlope(RaycastHit hit)
    {
        Vector3 slopeNormal = hit.normal;

        // Direction of slope downhill
        Vector3 slopeDown = Vector3.Cross(slopeNormal, Vector3.Cross(Vector3.up, slopeNormal));

        // If your stretch direction aligns with downhill, apply gravity
        float dot = Vector3.Dot(stretchDirection.normalized, slopeDown.normalized);

        return dot > 0.1f; // threshold
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
    
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(!isStretching)
            return;

        if (isMetal)
        {
            if (hit.transform.TryGetComponent(out IBreakable breakable))
            {
                breakable.Break();
            }
        }

        
        if (hit.transform.TryGetComponent(out EnvironmentObject environmentObject))
        {
            if (isOnHoney || environmentObject.OnHoney)
            {
                Debug.Log(environmentObject.name);
                stretchState = StretchState.Stuck;
                PlayerController.OnGrabEvent?.Invoke(environmentObject);
            }
        }
    }


    void OnPlayerRetracted()
    {
        if (!isStretching)
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
        


        yield return new WaitForFixedUpdate();

        Sequence stretchSequence = DOTween.Sequence();

        List<Vector3> listOfPositions = new List<Vector3>();


        var stretchPositions = springVisualConnection.GetPositions();

        int amount = stretchPositions.Length;

        ;
        if (!shouldStretchForward && !isMetal)
        {
            //Vector3 targetHeadDir = (bodySegment.transform.position + bodySegment.transform.forward) -headSegment.transform.position;
            //Vector3 targetHeadPosition = bodySegment.transform.position + (bodySegment.transform.forward * bodyOffset);




            for (int i = amount - 1; i > -1; i--)
            {
                listOfPositions.Add(stretchPositions[i]);
            }

            listOfPositions.Add(bodySegment.transform.position + (bodySegment.transform.forward * bodyOffset));

            positions = listOfPositions;





            stretchSequence.Append(headSegment.transform.DOPath(listOfPositions.ToArray(), stretchRetractTime,
                PathType.CatmullRom, PathMode.Full3D)).SetEase(Ease.OutQuad);



        }

        else
        {

            for (int i = 0; i < amount; i++)
            {
                listOfPositions.Add(stretchPositions[i]);
            }


            listOfPositions.Add(headSegment.transform
                .position - (headSegment.transform.forward * bodyOffset));


            List<Vector3> tailPositions = listOfPositions;

            for (int i = 0; i < tailPositions.Count; i++)
            {
                var pos = tailPositions[i];
                tailPositions[i] = pos - (tailOffset * bodySegment.transform.forward);
            }


            stretchSequence.Append(bodySegment.transform.DOPath(listOfPositions.ToArray(), stretchRetractTime,
                PathType.CatmullRom, PathMode.Full3D)).SetEase(Ease.OutQuad); ;

            stretchSequence.Join(tailSegment.transform.DOPath(tailPositions.ToArray(), stretchRetractTime,
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
       
        shouldStretchForward = false;
        //PlayerReferenceManager.instance.Honeyfied(false);

        if (stretchState == StretchState.Stuck)
        {
            PlayerController.OnReleaseEvent?.Invoke();
        }
        
        stretchState = StretchState.None;
        
        PlayerReferenceManager.instance.SetState(PlayerState.Locomotion);




        //yield return null;

    }



    private void OnDrawGizmosSelected()
    {


        if (!Application.isPlaying)
            return;
        Gizmos.color = Color.aliceBlue;

        //Gizmos.DrawWireSphere(headSegment.position, detectionRadius);
        Gizmos.DrawLine(headSegment.transform.position, headSegment.transform.position + headSegment.transform.forward * maxDetectionDistance);
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
