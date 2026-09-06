using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using RotaryHeart.Lib.PhysicsExtension;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityUtils;
using Physics = UnityEngine.Physics;


/*public class MovementStep
{
    public Vector3 position;
    public Vector3 direction;
    public float distance;
}*/
public class PlayerMovement : MovementBase
{
  

    //private CharacterController _characterController;

    [SerializeField] private Animator bubblyAnimatorHead, bubblyAnimatorBody, bubblyAnimatorTail;
    [SerializeField] private float groundMultiplier = 1f;
    [SerializeField] private float airMultiplier = 0.2f;
    private float movementMultiplier;

    public bool groundedCheck;

    public float bounceTime;
    // public InputActionReference moveInput;
    
    public float airDrag = 0.3f;

    public float groundDrag = 6f;

    public float climbDetectionDistance;
    
   // private RaycastHit groundHit;

    //private bool isOnSlope;
    [SerializeField] private float slopeGroundDistance = 0.5f;

    public float climbForce = 2;



    public float groundLimit = 100f;
    [Header("Silk")]
    //public bool isOnSilk;
    public float detectionRange = 1f;
    public float maxClimbAngle;
    public LayerMask silkMask;
     RaycastHit climbHit;
     public float climbTime = 0.4f;

     private bool isClimbingGrounded;

     [SerializeField] public float climbOffset = 0.2f;

     [SerializeField] private float climbSpeed = 4f;
    
    /*Vector3 targetBodyPosition;
    Vector3 targetTailPosition;*/
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    private Vector3 gravityDir;


    public Transform climbCheckOffset;
    
    //public Vector3 gravityDirection = Vector3.down;
    
   
    [SerializeField] private float climbDetectionRadius = 0.6f;
    [SerializeField] private float climbAngle;
    [SerializeField] private float climbDetectionCRadius;
    [SerializeField] private float checkClimbCornerOffset;
    [SerializeField] private float climbDetectionCDistance;
    public float climbTriggerOffset = 0.5f;


    bool isWalking;
    public float calculatedDrag;
    public MMF_Player walkSound;
    public MMF_Player honeySound;

    protected override void OnEnable()
    {
        
        PlayerStretch.onStretchStateChanged += OnStretchStateChanged;
        PlayerController.ClimbEvent += ClimbEvent;
        PlayerController.OnWaterEvent += OnWaterEvent;
    }

    private void OnWaterEvent()
    {
        
        if (isClimbing)
        {
            ChangeGravity(Vector3.down, false);
        }
        
        StartCoroutine(SoakPlayer());
    }
    
    
    public IEnumerator SoakPlayer()
    {
        PlayerReferenceManager.instance.isSoaked = true;
        yield return new WaitForSeconds(PlayerReferenceManager.instance.soakedDuration);
        yield return new WaitUntil(() => IsGrounded());
        PlayerReferenceManager.instance.isSoaked = false;
    }

    private void OnStretchStateChanged(PlayerStretch.StretchState obj)
    {
        if (isClimbing)
        {
            if (obj == PlayerStretch.StretchState.Stuck)
            {
                ChangeGravity(Vector3.down, false);
            }
        }
    }

    private void ClimbEvent(bool b)
    {
        isClimbing = b;
    }

    protected override void OnDisable()
    {
       
        PlayerStretch.onStretchStateChanged -= OnStretchStateChanged;
        PlayerController.ClimbEvent -= ClimbEvent;
        PlayerController.OnWaterEvent -= OnWaterEvent;
    }

    private void OnSturdyEvent(float arg1, bool arg2)
    {
        sturdyRatio = arg1;
        isOnSturdy = arg2;
    }
    
   
    protected override void Start()
    {

        
        segments.Add(PlayerReferenceManager.instance.segments[0]);
        segments.Add(PlayerReferenceManager.instance.segments[1]);
        segments.Add(PlayerReferenceManager.instance.segments[2]);

        rb = segments[0].rb;
        playerCollider = rb.GetComponent<SphereCollider>();
        sphereRadius = playerCollider.radius;
       
        
        characterHeight = playerCollider.radius;
       

        //climbDetectionDistance = (Mathf.Abs(transform.localPosition.y - groundCheck.localPosition.y)) + groundDistance;

        //collisionDetection = _rigidbody.transform.GetComponent<CollisionDetection>();
    }


    void Update()
    {
        if (PlayerReferenceManager.instance.currentState == PlayerState.Locomotion)
        {
            HandleInput();
        }

        HandleAnimations();
    }
    
    
    private void FixedUpdate()
    {
        Debug.Log("Current State: " + PlayerReferenceManager.instance.currentState);
        if (PlayerReferenceManager.instance.currentState != PlayerState.Locomotion)
            return;
        GroundCheck();
        //CollisionDetection();
        //AlignToSurface();
       
       
        HandleRotation();
        HandleDrag();
        AlignToSurface(); 
        HandleGravity();
        Movement(); 
        UpdateSegments();
      
        
       
       //DetermineMovement();
       
       
      
        

    }
    

    void HandleInput()
    {

        input = InputManager.instance.controls.Gameplay.Move.ReadValue<Vector2>();

        Debug.Log("Input: " + input);
        moveDir = GetInputVector();

        Debug.Log("MoveDir: " + moveDir);
        //
        //moveDir.y = 0;



    }

    
    //separate visuals and character
    

    void HandleDrag()
    {
        if (isGrounded || isClimbing)
        {
            movementMultiplier = groundMultiplier;
            currentDrag = groundDrag;
            airTime = 0;
        }

        else
        {
            airTime += Time.deltaTime;

            if (airTime >= airThreshold || isBounced)
            {
                movementMultiplier = airMultiplier;
                currentDrag = airDrag;
            }
        }
        
        airTime = Mathf.Clamp(airTime, 0, airThreshold);
        rb.linearDamping = currentDrag;
        //calculatedDrag = 1 - (Time.deltaTime * currentDrag);
    }

    // Update is called once per frame

    void HandleAnimations()
    {
        if (PlayerReferenceManager.instance.currentState != PlayerState.Locomotion)
        {
            isWalking = false;
        }

        else
        {
            if (isGrounded)
            {
                if (moveDir.magnitude > 0)
                {
                    isWalking = true;
                }

                else
                {
                    isWalking = false;
                }
            }

            else
            {
                isWalking = false;
            }
        }



        if (isWalking)
        {
            if (isClimbing)
            {
                honeySound.PlayFeedbacks();
            }

            else
            {
                walkSound.PlayFeedbacks();
            }
            
        }
        bubblyAnimatorHead.SetBool("isWalking", isWalking);
        bubblyAnimatorBody.SetBool("isWalking", isWalking);
        bubblyAnimatorHead.SetBool("isWalking", isWalking);
    }

    protected override void Movement()
    {
        
        Vector3 currentVelocity = rb.linearVelocity;

        

       
        var gravityVel = Vector3.Project(currentVelocity, -groundNormal);

        if (isOnSlope)
        {
            gravityVel = Vector3.zero;
        }

        if (!isGrounded)
        {
            gravityVel = Vector3.Project(currentVelocity, PlayerReferenceManager.instance.playerGravityDir);
        }


        var hz = currentVelocity - gravityVel;
        Debug.Log("RB Velocity: " + currentVelocity);
        Debug.Log("Gravity Velocity: " + gravityVel);
        Debug.Log(" HZ Velocity: " + hz);

        var _speed = isClimbing ? climbSpeed : speed;
        var targetVelocity = (moveDir) * (movementMultiplier * _speed);

        
        //currentVelocity
        //targetVelocity = Vector3.ClampMagnitude(targetVelocity , maxSpeed);
        //Vector3 velocityChange = AlignToSurface(targetVelocity - currentVelocity);
        
        
        
        Vector3 velocityChange = targetVelocity - hz;
        
        
        Debug.Log("VelocityChange: " + velocityChange);

        velocityChange = Vector3.ClampMagnitude(velocityChange, maxSpeed);

        Debug.Log("VelocityChangeClamped: " + velocityChange);

        

       

      


        moveVelocity = velocityChange;

      
        
        rb.AddForce(moveVelocity, ForceMode.VelocityChange);
        
        //dustParticle.PlayFeedbacks();

    }

    
    public void Bounce(float height)
    {
       
        //verticalVelocity = 0;
       
        float bounceHeight = rb.mass * (Mathf.Sqrt(-2 * PlayerReferenceManager.instance.gravity * height) - (Time.deltaTime * PlayerReferenceManager.instance.gravity) / 2);
       // Debug.Log("Bounce height: " + bounceHeight);
        
        //Debug.Log(rb.linearDamping);
        
        Debug.Log(height);
        //rb.AddForce(gravityDir * bounceHeight , ForceMode.VelocityChange);

        canResetBounce = false;
        rb.linearVelocity = Vector3.zero;
        //isClimbing = false;
        //ChangeGravity(Vector3.down, false);
        
        
        Debug.Log("bouncing!");
        rb.AddForce( PlayerReferenceManager.instance.playerGravityDir * bounceHeight, ForceMode.Impulse);
        isBounced = true;
        
        //rb.DOMove(rb.position + (Vector3.up * height), bounceTime).OnComplete(() => canResetBounce = true);
    }








    void  AlignToSurface()
    {
        //Vector3 velocity = targetVelocity;
        /*Debug.Log("Aligning Surface");
       Quaternion gravityRotation = Quaternion.FromToRotation(transform.up, -transform.up) * rb.transform.rotation;
       Quaternion surfaceRotation = Quaternion.FromToRotation(transform.up, groundNormal) * rb.transform.rotation;
       
       Quaternion finalRotationDelta = Quaternion.Slerp(gravityRotation, surfaceRotation, turnSpeed);
       
       transform.rotation = Quaternion.Slerp(transform.rotation, finalRotationDelta, turnSpeed * Time.smoothDeltaTime);*/


        if (isClimbing && !PlayerReferenceManager.instance.isSoaked)
        {

            /*if (RotaryHeart.Lib.PhysicsExtension.Physics.Raycast(transform.position, -transform.up, out climbHit,
                    climbDetectionDistance, silkMask, QueryTriggerInteraction.Ignore, PreviewCondition.Both, 0,
                    Color.aliceBlue, Color.black))
            {


                Vector3 pos = transform.position + -transform.up;
                Ray downRay = new Ray(pos, -moveDir);
                if (RotaryHeart.Lib.PhysicsExtension.Physics.Raycast(downRay, out climbHit, climbDetectionCDistance, silkMask,
                        QueryTriggerInteraction.Ignore, PreviewCondition.Both, 0, Color.chartreuse, Color.crimson))
                {
                    /*climbAngle = Vector3.Angle(climbHit.normal,  transform.up);
                    Debug.Log("Climb Hit Normal: " + climbHit.normal);#1#
                    
                    //Debug.Log("OffsetNormalised: " + offSet.normalized);
                    
                    ChangeGravity(-climbHit.normal, true, true, false, Vector3.zero, climbHit.transform);

                    //DebugExtensions.DebugSphereCast(climbCheckU.position, transform.forward, climbDetectionDistance, Color.darkOrange, climbDetectionRadius, 1, CastDrawType.Complete, PreviewCondition.Both, true );
                }
                
                /*else
                {
                    Debug.Log("detected nothing");
                    ChangeGravity(Vector3.down, false, false);
                
                }#1#
                
            }
            */

          
            
            
            //disable climb
           
            
            if (Physics.Raycast(rb.position, transform.forward,
                    climbDetectionCDistance, PlayerReferenceManager.instance.groundMask, QueryTriggerInteraction.Ignore))
            {
                Debug.Log("Climbing Ground Detected");
                ChangeGravity(Vector3.down, false);
            }

            else if (transform.up == gravityDir)
            {
                if(!Physics.Raycast(rb.position, -gravityDir, climbDetectionCDistance, silkMask, QueryTriggerInteraction.Ignore))
                {
                    ChangeGravity(Vector3.down, false);
                }
            }




            /*else if (!isGrounded)
            {
                ChangeGravity(Vector3.down, false);
            }*/
           
           

        }
        
        

       
    }
    
    
    public void ChangeGravity(Vector3 newDir, bool isTrigger, bool isOnSurface = false, bool setPos = false, Vector3 pos = new Vector3(), Transform wallObject = null)
    {
        Vector3 rotateDir = -newDir;

        

        Quaternion rotationDifference = Quaternion.FromToRotation(transform.up, rotateDir);

        rb.transform.rotation = rotationDifference * rb.transform.rotation;
        //Debug.Log("Hit Normal:" + hit.normal);


        climbDir = rotateDir;
        PlayerReferenceManager.instance.playerGravityDir = rotateDir;
        gravityDir = PlayerReferenceManager.instance.playerGravityDir;
        
        

        //rb.constraints = RigidbodyConstraints.FreezeRotation
        
        

        moveDir = GetInputVector();


        //PlayerReferenceManager.instance.useGravity = false;

        if (setPos)
        {
            Debug.Log("Moving Object");
            rb.DOMove(pos, climbTime).SetEase(Ease.InQuad);
        }


      
        PlayerController.ClimbEvent?.Invoke(isTrigger);
       


       
        Debug.Log("Gravity Dir: " + rotateDir);

        // Debug.Log(climbHit.normal);
        // gravityDirection = newDir;
        //AlignToSurface();

        /*
         * If (you are not grounded and there is a wall detected by the raycast) {

            reduce gravity/turn it off;
            Vector3 projectonplane = Vector3.ProjectOnPlane(rigidbody.velocity, planenormal);
            addforce to the rigidbody using the what projectonplane spit out; ///use forcemode impulse or VelocityChange

            }
         */
    }


    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Water"))
        {
            if(PlayerReferenceManager.instance.currentState != PlayerState.Locomotion)
                return;
            PlayerController.OnWaterEvent?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
       
        
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);

        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(segments[1].groundCheck.position, groundRadius);
            Gizmos.DrawWireSphere(segments[2].groundCheck.position, groundRadius);
        }
    }
    
    
    
    void UpdateSegments()
    {
        if (PlayerReferenceManager.instance.currentState != PlayerState.Locomotion)
            return;
        /*if (PlayerReferenceManager.instance.launched)
        {
            Vector3 targetVelocityBody = (transform.up * verticalVelocity) + launchVelocityBody;
            Vector3 targetVelocityTail = (transform.up * verticalVelocity) + launchVelocityTail;
            
            PlayerReferenceManager.instance.bodySegment.Move(targetVelocityBody * Time.deltaTime);
            PlayerReferenceManager.instance.tailSegment.Move(targetVelocityTail * Time.deltaTime);
        }*/

        else
        {
             for (int i = 1; i < segments.Count; i++)
             {
                 Vector3 pos = segments[i].rb.position;
                 Vector3 prevPos = segments[i - 1].rb.position;
                 //Vector3 forward = segments[i - 1].t.forward;


                 var spacing = Mathf.Abs(segments[i].spacingToNextSegment);

                 Vector3 currentDir = pos - prevPos;



                 Vector3 desiredPos = prevPos + (currentDir.normalized * spacing);






                 //segments[i].rb.AddForce(direction * Mathf.Lerp() , ForceMode.Acceleration);


                 //check if overshooting
                 Vector3 velocity = segments[i].rb.linearVelocity;
                 segments[i].springConnector.UpdateSpringVector(Time.fixedDeltaTime, ref pos, ref velocity, desiredPos);

                 // segments[i].rb.MovePosition(pos);
                // 
                segments[i].rb.linearVelocity = velocity;

                 segments[i].isGrounded = Physics.CheckSphere(segments[i].groundCheck.position, groundDistance, PlayerReferenceManager.instance.groundMask);
                 if (!segments[i].isGrounded)
                 {
                     if (isGrounded)
                     {
                         segments[i].rb.AddForce( PlayerReferenceManager.instance.gravity * PlayerReferenceManager.instance.playerGravityDir, ForceMode.Acceleration);
                     }
                 }
                 
                
            
            
                
                



                 //

                 //segments[i].rb.MovePosition(smoothedPosition);
                 //var scaledDirection = Vector3.Scale();


                 //Vector3 finalVelocity = segments[i].collisionDetection.CollideAndSlide(targetPos, pos, 0, true, targetPos);
                 //if (finalVelocity == Vector3.zero)
                 //  {
                 //      finalVelocity = pos;
                 //  }
                 //   segments[i].rb.MovePosition(finalVelocity);





                 //calculate spring physics

                 Vector3 targetPos = desiredPos - pos;



                 Vector3 direction = VectorMath.RemoveDotVector(targetPos.normalized,
                     PlayerReferenceManager.instance.playerGravityDir);

                 //rotation
                 if (moveDir.magnitude > 0.001f)
                 {
                     Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);


                     segments[i].t.transform.rotation = (Quaternion.Slerp(segments[i].t.rotation, targetRotation, turnSpeed * Time.deltaTime));

                 }

             }

            
        }
       
    }
}