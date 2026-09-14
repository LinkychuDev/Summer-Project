using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using RotaryHeart.Lib.PhysicsExtension;
using UnityEngine;
using UnityEngine.InputSystem;
using Physics = UnityEngine.Physics;

public interface IStretchBashable
{
    public void StretchBash();
}
public class PlayerStretch: MovementBase
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

    private Rigidbody headSegment;
    private Rigidbody bodySegment;
    private Rigidbody tailSegment;

    private float currentStretchTime;
    Vector3 cachedHeadPosition;
    Vector3 cachedBodyPosition;
    Vector3 cachedTailPosition;

    Vector3 targetHeadPosition;
    
    //public float stretchSpeed = 2f;
    private float maxDistanceHead;
    

    [SerializeField] private Transform headTarget;
    private float bodyOffset;
    private float tailOffset;

    //private float currentStretchDistance;



    public Material stretchMaxMaterial;
    //spherecast detection
    [Header("Collision Detection")]
    [SerializeField] float detectionOffset = 0.5f;
    [SerializeField] private float maxDetectionDistance = 0.1f;

    private float detectionRadius;
    public StretchState stretchState = StretchState.None;

    public bool shouldStretchForward;

    private PlayerSwing swing;

    private PlayerGrab grab;
    public float currentStretchDistance;
    //private bool wasStartingStretchGrounded;

    //spring joint values
    //private SpringJoint headJoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

   
    Collider[] colliders;

    [SerializeField] private SpringVisualConnection springVisualConnection;

    private List<Vector3> positions = new List<Vector3>();
    private Vector3 stretchVelocity;

    public bool isMetal;
   
    public static Action<StretchState> onStretchStateChanged;
    private bool isOnHoney;
    [SerializeField] private bool isAtMaxSpring;
    
    [SerializeField]float yOffset;

    
    [Header("Spring Visual Effects")]
    [SerializeField] private int shakeVibrationStrength;
    [SerializeField] private float shakeDuration;
    [SerializeField] private float headShakeStrength = 1, bodyShakeStrength = 1, tailShakeStrength =1;
    private Transform headVisual,  bodyVisual, tailVisual;
    [SerializeField] private float knockbackDelay = -0.5f;
    
    public MMF_Player bashPlayer;

    private bool isSoundPaused;

    private bool activatedMaxHead;
    
    float holdDuration;

    public MMF_Player stretchWindUp, stretchWindDown;
    private MMF_MMSoundManagerSound windUpSound;

    public MMF_Player bashEffect, stickEffect;
  
    public MMF_Player headBop;


    private bool hasAlreadyPlayedStickOnce;
    public float detectionDistance = 0.35f;

    protected override void OnEnable()
    {
        base.OnEnable();
        InputManager.instance.controls.Gameplay.Stretch.started += ctx => OnPlayerStretchedEvent();
        InputManager.instance.controls.Gameplay.Stretch.canceled += ctx => OnPlayerRetracted();
        PlayerController.isOnHoneyEvent += b => isOnHoney = b;
        PlayerController.ClimbEvent += ClimbEvent;
        
        
    }

    private void ClimbEvent(bool b, Transform wallObject)
    {
        isClimbing = b;
    }

    private void ClimbEvent(bool obj)
    {
        isClimbing = obj;
    }

    private void OnSturdyEvent(float arg1, bool arg2)
    {
        sturdyRatio = arg1;
        isMetal = arg2;   
    }


    protected override void OnDisable()
    {
        base.OnDisable();
        InputManager.instance.controls.Gameplay.Stretch.started -= ctx => OnPlayerStretchedEvent();
        InputManager.instance.controls.Gameplay.Stretch.canceled -= ctx => OnPlayerRetracted();
        PlayerController.isOnHoneyEvent -= _ => isOnHoney = false;
        PlayerController.ClimbEvent -=  ClimbEvent;
    }
    protected override void Start()
    {
        
        colliders = new Collider[1];
        
        bodyOffset = PlayerReferenceManager.instance.bodyOffset;
        tailOffset = PlayerReferenceManager.instance.tailOffset;

        segments = PlayerReferenceManager.instance.segments;
        headSegment = segments[0].rb;
        bodySegment = segments[1].rb;
        tailSegment = segments[2].rb;

        rb = headSegment;
        swing = GetComponent<PlayerSwing>();

        maxDistanceHead = bodyOffset + stretchDistanceHead;
        headVisual = segments[0].visual;
        bodyVisual = segments[1].visual;
        tailVisual = segments[2].visual;

        grab = GetComponent<PlayerGrab>();

        UpdateStretchState(StretchState.None);
        detectionRadius = headSegment.GetComponent<SphereCollider>().radius + detectionOffset;

        windUpSound = stretchWindUp.GetFeedbackOfType<MMF_MMSoundManagerSound>();
      
        stretchWindDown.GetFeedbackOfType<MMF_MMSoundManagerSound>().PlaybackDuration =
            new Vector2(stretchRetractTime, stretchRetractTime);
        /*collisionRadius = collisionRadiusOffset + headSegment.GetComponent<SphereCollider>().radius;
        colliders = new Collider[maxColliders];
        collisionMask = PlayerReferenceManager.instance.playerCollisionMask;*/
        //CreateJoint();
        //headSegment.GetComponent<SphereCollider>().radius
        //sphere collision
    }

    private void Update()
    {
        if (PlayerReferenceManager.instance.currentState == PlayerState.Locomotion)
            return;
        if (!PlayerReferenceManager.instance.canStretch)
            return;
        input = InputManager.instance.controls.Gameplay.Move.ReadValue<Vector2>();
        moveDir = GetInputVector();
       
    }

    

    void FixedUpdate()
    {
        if (PlayerReferenceManager.instance.currentState == PlayerState.Locomotion)
            return;
        if (!IsStretching())
            return;
        switch (stretchState)
        {
            case StretchState.Retracting:
                break;
            case StretchState.Stretching:
                GroundCheck();
                if (!IsGrounded())
                {
                    HandleGravity();
                }

                HandleRotation();
                StretchEvent();
                CollisionDetection();

                break;
            case StretchState.Stuck:
                break;


        }




    }

   

    // Update is called once per frame


    bool IsStretching()
    {
        return PlayerReferenceManager.instance.currentState == PlayerState.Stretching && PlayerReferenceManager.instance.canStretch;
    }
  
    void OnTriggerEnter(Collider other)
    {
        if(stretchState == StretchState.Retracting)
            return;
        
        if(IsStretching())
        {
            if (other.CompareTag("Honey"))
            {
               
                if (other.gameObject.TryGetComponent(out HoneySwingTest honeyTest))
                {
                    //honeyTest.DisableCollisions();
                   if(!isOnHoney)
                   {
                       
                       Debug.Log(gameObject.name + " can stick: " + honeyTest.CanStick());
                       if (honeyTest.CanStick())
                       {
                           
                       }

                       else
                       {
                           return;
                       }
                      
                   }
                    
                   
                    
                    PlayerReferenceManager.instance.SetState(PlayerState.Swinging);

                    //replace with events
                    swing.StartSwing(honeyTest);
                }

               
            }

            else
            {
                if (other.TryGetComponent(out IStretchBashable bashable))
                {
                    bashPlayer.PlayFeedbacks();
                    bashEffect.PlayFeedbacks();
                    bashable.StretchBash();
                }
            }
        }
    }



    void CollisionDetection()
    {
        if(!IsStretching())
            return;
        
        bool shouldPlayEffect = false;
        shouldStretchForward = false;
        
        if (Physics.Raycast(headSegment.transform.position, headSegment.transform.forward,  out RaycastHit hit, detectionRadius))
        {
            var other = hit.transform;
            
           
            
            if (other.transform.TryGetComponent(out EnvironmentObject environmentObject))
            {
                if (isOnHoney || environmentObject.OnHoney)
                {
                    UpdateStretchState(StretchState.Stuck);
                    stickEffect.PlayFeedbacks(environmentObject.transform.position);
                    PlayerController.OnGrabEvent?.Invoke(environmentObject);
                    shouldPlayEffect = true;
                }
            }

            else
            {
               

                if (isMetal)
                {
                    if (other.transform.TryGetComponent(out IMetalBreakable metalBreak))
                    {
                        metalBreak.MetalBreak();
                    }
                }
                
                if (other.TryGetComponent(out IStretchBashable bashable))
                {
                    bashPlayer.PlayFeedbacks();
                    bashable.StretchBash();
                }
                
                if (other.gameObject.TryGetComponent(out IBreakable breakable))
                {
                    bashEffect.PlayFeedbacks();
                    breakable.Break();
                }
                
                
                if (other.transform.TryGetComponent(out StickableObject stickable))
                {
                    if((!stickable.CanStick()) || !isOnHoney)
                        return;
                    Debug.Log(other.name);
                    UpdateStretchState(StretchState.Stuck);
                   // stickEffect.PlayFeedbacks();
                    shouldPlayEffect = true;
                    shouldStretchForward = true;
                   // PlayerReferenceManager.instance.SetState(PlayerState.Stuck);
                }

                else if(other.transform.CompareTag("Honey"))
                {
                    UpdateStretchState(StretchState.Stuck);
                    shouldStretchForward = true;
                    shouldPlayEffect = true;
                }
                
                else if (isOnHoney)
                {
                    if (other.gameObject.CompareTag("PillarObject"))
                    {
                        shouldStretchForward = true;
                        UpdateStretchState(StretchState.Stuck);
                        shouldPlayEffect = true;
                    }

                }
            }
            
            
            
            
            

        }
        


        if (shouldPlayEffect)
        {
            if(hasAlreadyPlayedStickOnce)
                return;
            stickEffect.PlayFeedbacks();
            headBop.PlayFeedbacks();
            hasAlreadyPlayedStickOnce = true;
        }
        
    }

    bool CanStretch()
    {

        if (PlayerReferenceManager.instance.canStretch)
        {
            if (isGrounded)
            {
                return true;
            }
            
            if (IsGrounded())
            {
                return true;
            }

            else if (isOnCoyoteTime)
            {
                return true;
            }

            if (isClimbing)
                return true;

            return false;
        }




        return false;
    }
    

    void OnPlayerStretchedEvent()
    {

        if (!CanStretch()) 
            return;
        //headSegment.isKinematic = true;

        if (stretchState != StretchState.None)
            return;
        if (!PlayerReferenceManager.instance.canStretch)
            return;
        


      
       
        //bodySegment.MovePosition(headSegment.position - (headSegment.transform.forward *bodyOffset));
        // tailSegment.MovePosition(bodySegment.position - (bodySegment.transform.forward * tailOffset));

        cachedHeadPosition = headSegment.transform.position;
        cachedBodyPosition = bodySegment.transform.position;
        cachedTailPosition = tailSegment.transform.position;
        currentStretchTime = 0;

        yOffset = cachedHeadPosition.y - cachedBodyPosition.y;
        
        PlayerReferenceManager.instance.SetState(PlayerState.Stretching);
        UpdateStretchState(StretchState.Stretching);
        stretchWindUp.PlayFeedbacks();
       
       
        //cachedStretchPositions.Add(headSegment.transform);

    }




    void StretchEvent()
    {

       
        //take stretching position


        //  currentStretchDistance = Mathf.Lerp(minDistanceHead, maxDistanceHead, currentStretchTime );
        Vector3 currentVelocity = rb.linearVelocity;


        var vel = Vector3.Project(currentVelocity, PlayerReferenceManager.instance.playerGravityDir);
        
    
        var hz = currentVelocity - vel;
        
        

        Vector3 targetVelocity = moveDir * (speed * input.sqrMagnitude * sturdyRatio);
        
        Vector3 dirToBody = (headSegment.transform.position - bodySegment.transform.position);

        float distance = dirToBody.magnitude;

        currentStretchDistance = distance;

        if (distance > maxDistanceHead * sturdyRatio)
        {
            float dotProduct = Vector3.Dot(targetVelocity, dirToBody.normalized);

            if (dotProduct > 0)
            {
                targetVelocity -= dotProduct * dirToBody.normalized;

                if (!isAtMaxSpring)
                {
                    isAtMaxSpring = true;
                    activatedMaxHead = true;
                    PlayerReferenceManager.instance.headRenderer.material = stretchMaxMaterial;
                }
              
            }
        }

        else
        {
            if (isAtMaxSpring)
            {
                PlayerReferenceManager.instance.headRenderer.material = PlayerReferenceManager.instance.headMaterial;
            }
            isAtMaxSpring = false;
        }

        
        
        
        Vector3 velocityChange = targetVelocity - currentVelocity;
        
        
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxSpeed);


        
        moveVelocity = velocityChange;
        headSegment.AddForce(moveVelocity, ForceMode.VelocityChange);

        var pos = headSegment.position;

        
        //make sure this matches current gravity direction
       // pos.y = Mathf.Clamp(headSegment.position.y, yOffset, cachedBodyPosition.y + maxDistanceHead);


        //var scaledPos = Vector3.Scale(pos, transform.up);
        var offset = pos - cachedBodyPosition;
        
        float offsetAlongGravity = Vector3.Dot(offset, PlayerReferenceManager.instance.playerGravityDir);

        if (offsetAlongGravity < 0)
        {
            Vector3 correctedPos = pos - PlayerReferenceManager.instance.playerGravityDir * offsetAlongGravity;
            headSegment.position = correctedPos;
        }
        
        
        //if(z)
       // Debug.Log("Offset: " + offsetScaled);
        
        
        //Debug.Log("Scaled: " + scaledPos);
       // headSegment.position = pos;

    }

    protected override void HandleRotation()
    {


        //rotate around middle segment position
        if (input.magnitude > 0.01f)
        {
            if (stretchState == StretchState.Stretching)
            {
                Vector3 dir = moveDir;
                if (!isClimbing)
                {
                    dir.y = 0;
                }
                Quaternion targetRotation =  Quaternion.LookRotation(dir, PlayerReferenceManager.instance.playerGravityDir);
                headSegment.transform.rotation = Quaternion.Slerp(headSegment.transform.rotation, targetRotation,
                    turnSpeed * Time.fixedDeltaTime);
            }

        }



    }
    
    
  

    void OnPlayerRetracted()
    {
        if (!IsStretching())
            return;
      

        if (stretchState == StretchState.Stuck)
        {
            PlayerController.OnPreReleaseEvent?.Invoke();
        }

        StartCoroutine(OnPlayerRetractedEvent());
    }

    internal override Vector3 GetInputVector()
    {
        Vector3 forward = PlayerReferenceManager.instance.playerCam.transform.forward;
        Vector3 right = PlayerReferenceManager.instance.playerCam.transform.right;
        forward.y = 0;
        forward.Normalize();
        right.y = 0;
        right.Normalize();
        Vector3 up = PlayerReferenceManager.instance.playerCam.transform.up;
       

        if (isClimbing)
        {
           
            //Debug.Log("Right: " + right);
            
            /*
            Debug.Log("Climb Dir:  " + climbDir);
            
            Debug.Log("Player Cam Forward:" + playerCam.transform.forward);
            Debug.Log("Player Cam Right:" + playerCam.transform.right);
            forward = playerCam.transform.forward;
            
            Debug.Log("Forward Dir:  " + forward);

            right = Vector3.Cross(climbDir, forward);
            
            Debug.Log("Right Dir:  " + right);

            right = -Vector3.ProjectOnPlane(right, climbDir);
            forward = -Vector3.ProjectOnPlane(forward, climbDir);*/

            right = PlayerReferenceManager.instance.playerCam.transform.right;
            forward = PlayerReferenceManager.instance.playerCam.transform.up;

        }

       
        var inputVector = right * input.x + forward * input.y;

        
        if (isOnSlope)
        {
            inputVector = Vector3.ProjectOnPlane(inputVector, slopeHit.normal);
        }

       
        
        return inputVector.normalized;
    }


    void WaypointChanged(int  waypointIndex)
    {
        
    }
    public IEnumerator OnPlayerRetractedEvent()
    {
        
        yield return new WaitForEndOfFrame();
        isSoundPaused = false;
        activatedMaxHead = false;
        Debug.Log(stretchState);
        //yield return new WaitUntil(() => PlayerPlayerReferenceManager.instance.state == PlayerState.Stretching);
        UpdateStretchState(StretchState.Retracting);
    
        

        currentStretchTime = 0;

        tailSegment.transform.LookAt(bodySegment.transform.position + bodySegment.transform.forward);
        


        yield return new WaitForFixedUpdate();

        Sequence stretchSequence = DOTween.Sequence();

        List<Vector3> listOfPositions = new List<Vector3>();


        var stretchPositions = springVisualConnection.GetPositions();

        int amount = stretchPositions.Length;
        
        stretchWindDown.PlayFeedbacks();

        
        if (!ForwardCheck())
        {
            //Vector3 targetHeadDir = (bodySegment.transform.position + bodySegment.transform.forward) -headSegment.transform.position;
            //Vector3 targetHeadPosition = bodySegment.transform.position + (bodySegment.transform.forward * bodyOffset);


            headSegment.transform.parent = bodySegment.transform;

            positions.Clear();
            
            for (int i = amount - 1; i > -1; i--)
            {
                listOfPositions.Add((bodySegment.transform.InverseTransformPoint(stretchPositions[i])));
            }

            //listOfPositions.Add(bodySegment.transform.position + (bodySegment.transform.forward * bodyOffset));

            positions = listOfPositions;
            var pathArray = positions.ToArray();


            stretchSequence.Append(headSegment.transform.DOLocalPath(pathArray, stretchRetractTime,
                PathType.CatmullRom, PathMode.Full3D)).SetEase(Ease.OutQuad);


        }

        else
        {

            tailSegment.transform.parent = headSegment.transform;
            bodySegment.transform.parent = headSegment.transform;

            for (int i = 0; i < amount; i++)
            {
                listOfPositions.Add(headSegment.transform.InverseTransformPoint(stretchPositions[i]));
            }


           // listOfPositions.Add(headSegment.transform.position - (headSegment.transform.forward * bodyOffset));


            List<Vector3> tailPositions = listOfPositions;

            for (int i = 0; i < tailPositions.Count; i++)
            {
                var pos = tailPositions[i];
                tailPositions[i] = headSegment.transform.InverseTransformPoint(headSegment.transform.TransformPoint(pos - (tailOffset * bodySegment.transform.forward)));
            }


            stretchSequence.Append(bodySegment.transform.DOLocalPath(listOfPositions.ToArray(), stretchRetractTime,
                PathType.CatmullRom, PathMode.Full3D)).SetEase(Ease.OutQuad); ;

            stretchSequence.Join(tailSegment.transform.DOLocalPath(tailPositions.ToArray(), stretchRetractTime,
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



        stretchSequence
            .Append(headVisual.DOShakePosition(shakeDuration, headShakeStrength,
                shakeVibrationStrength + (int)currentStretchDistance)).SetEase(Ease.OutQuad);
        stretchSequence
            .Join(bodyVisual.DOShakePosition(shakeDuration, bodyShakeStrength,
                shakeVibrationStrength + (int)currentStretchDistance)).SetEase(Ease.OutQuad);
        stretchSequence
            .Join(tailVisual.DOShakePosition(shakeDuration, tailShakeStrength,
                shakeVibrationStrength + (int)currentStretchDistance)).SetEase(Ease.OutQuad);
        

        yield return stretchSequence.WaitForCompletion();
        
        
      


        


      
        //add effect
        
        yield return new WaitForFixedUpdate();


        headSegment.transform.parent = PlayerReferenceManager.instance.transform;
        bodySegment.transform.parent = PlayerReferenceManager.instance.transform;
        tailSegment.transform.parent = PlayerReferenceManager.instance.transform;
        PlayerReferenceManager.instance.headRenderer.material = PlayerReferenceManager.instance.headMaterial;
      
        //PlayerReferenceManager.instance.Honeyfied(false);

        if (stretchState == StretchState.Stuck)
        {
            if (grab.targetObject != null)
            {
                PlayerController.OnReleaseEvent?.Invoke();
            }
           
        }
        
        UpdateStretchState(StretchState.None);
        
        
        
        
        
        PlayerReferenceManager.instance.SetState(PlayerState.Locomotion);
        shouldStretchForward = false;
        hasAlreadyPlayedStickOnce =   false;
        

        //yield return null;

    }

    private bool ForwardCheck()
    {
        return shouldStretchForward || isMetal;
    }


    public void UpdateStretchState(StretchState newState)
    {
        if(stretchState == newState)
            return;
        stretchState = newState;
        onStretchStateChanged?.Invoke(stretchState);
    }

    private void OnDrawGizmosSelected()
    {


        if (!Application.isPlaying)
            return;
        Gizmos.color = Color.aliceBlue;

        //Gizmos.DrawWireSphere(headSegment.position, detectionRadius);
        Gizmos.DrawLine(headSegment.transform.position, headSegment.transform.position + headSegment.transform.forward * maxDetectionDistance);
        
        if (positions.Count == 0)
            return;
        Gizmos.color = Color.cyan;

        for (int i = 0; i < positions.Count; i++)
        {
            Gizmos.DrawWireSphere(positions[i], PlayerReferenceManager.instance.distanceRadius);
        }

    }
    
    
}
