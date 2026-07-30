using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


/*public class MovementStep
{
    public Vector3 position;
    public Vector3 direction;
    public float distance;
}*/
public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float maxSpeed;
    public float turnSpeed;



    private List<Segment> segments = new List<Segment>();

    private CharacterController _characterController;

    private Vector2 input;

    private Vector3 moveDir;

    private Transform playerCam;

    private Vector3 moveVelocity;

    private float verticalVelocity;
    
    


    [SerializeField] private float groundMultiplier = 1f;
    [SerializeField] private float airMultiplier = 0.2f;
    private float movementMultiplier;

    public static bool isGrounded;
    public bool groundedCheck;

    [SerializeField] private float acceleration = 10;
    [SerializeField] private float deceleration = 10;

    // public InputActionReference moveInput;



    public float groundDistance = 0.4f;
    public float groundRadius = 0.2f;
    public Transform groundCheck;

    public float GroundAngleLimit = 45;

    public float airDrag = 0.3f;

    public float groundDrag = 6f;


    private SphereCollider playerCollider;
    private Vector3 slopeDir;
    private RaycastHit slopeHit;
    private RaycastHit groundHit;

    //private bool isOnSlope;
    [SerializeField] private float slopeGroundDistance = 0.5f;


    [SerializeField] private float slopeMultiplier = 1.5f;

    private float sphereRadius;
    public bool isOnSlope;

    [SerializeField] private float slopeOffset = 0.1f;

    private const float slopeDownForce = 80f;

    public float coyoteTime = 0.3f;


    private float airTime;
    [SerializeField] private float airThreshold = 2f;


    public static bool isOnCoyoteTime;

    float timeSinceLastGrounded;
    [SerializeField] private float groundVelocity = -2f;
    

    private bool isOnSturdy;

    private float sturdyRatio;



    public static Action<Vector3, Vector3, Vector3> LaunchVelocity;


    private bool isBounced; 
    Vector3 launchVelocityHead, launchVelocityBody, launchVelocityTail;


    private float characterHeight;
    [Header("Silk")]
    //public bool isOnSilk;
    public bool isClimbing;
    public float detectionRange = 1f;
    public float maxClimbAngle;
    public LayerMask silkMask;

    public float slopeAngle;

     private Vector3 climbDir;
     RaycastHit climbHit;
    /*Vector3 targetBodyPosition;
    Vector3 targetTailPosition;*/
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        PlayerController.isOnSturdyEvent += OnSturdyEvent;
        LaunchVelocity += OnSwingLaunch;
    }

    private void OnDisable()
    {
        PlayerController.isOnSturdyEvent -= OnSturdyEvent;
        LaunchVelocity -= OnSwingLaunch;
    }

    private void OnSturdyEvent(float arg1, bool arg2)
    {
        sturdyRatio = arg1;
        isOnSturdy = arg2;
    }
    
    private void OnSwingLaunch(Vector3 arg1, Vector3 arg2, Vector3 arg3)
    {
        launchVelocityHead = arg1;
        launchVelocityBody = arg2;
        launchVelocityTail = arg3;
    }

    void Start()
    {


        segments.Add(PlayerReferenceManager.instance.segments[0]);
        segments.Add(PlayerReferenceManager.instance.segments[1]);
        segments.Add(PlayerReferenceManager.instance.segments[2]);

        _characterController = segments[0].characterController;
        playerCollider = _characterController.GetComponent<SphereCollider>();
        sphereRadius = playerCollider.radius;
        playerCam = Camera.main.transform;

        _characterController.slopeLimit = GroundAngleLimit;
        characterHeight = _characterController.height;
        //collisionDetection = _rigidbody.transform.GetComponent<CollisionDetection>();
    }

    private void Update()
    {

        GroundCheck();
        
        if (PlayerReferenceManager.instance.currentState != PlayerState.Locomotion)
            return;
        HandleInput();
        CollisionDetection();
        HandleGravity();

        HandleRotation();
        HandleDrag();
        Movement();


    }

    void HandleInput()
    {

        input = InputManager.instance.controls.Gameplay.Move.ReadValue<Vector2>();
        Vector3 forward = playerCam.transform.forward;
        Vector3 right = playerCam.transform.right;
        forward.y = 0;
        forward.Normalize();
        right.y = 0;
        right.Normalize();


        moveDir = input.x * right + input.y * forward;
        moveDir.y = 0;
        
    }

   

    void HandleGravity()
    {
        if (isGrounded)
        {
            
            
            if (verticalVelocity < 0)
            {
                verticalVelocity = (groundVelocity);
            }
            
            
            if (PlayerReferenceManager.instance.launched)
            {
                launchVelocityHead = Vector3.zero;
                launchVelocityBody = Vector3.zero;
                launchVelocityTail = Vector3.zero;
                PlayerReferenceManager.instance.launched = false;
            }
        }
        else
        {
            // Full gravity in air
            verticalVelocity += PlayerReferenceManager.instance.gravity * (1/sturdyRatio) * Time.deltaTime;
        }


       
    }

  

    public void GroundCheck()
    {
        //isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, PlayerReferenceManager.instance.groundMask);
        
       isGrounded = Physics.SphereCast(transform.position, groundRadius, -transform.up, out  groundHit, characterHeight * 0.5f + 0.2f,
            PlayerReferenceManager.instance.groundMask);
       //== PlayerReferenceManager.instance.isGrounded = isGrounded;
       
       groundedCheck = isGrounded;

        if (isGrounded)
        {
            isBounced = false;
        }
        isOnSlope = IsOnSlope();
        PlayerReferenceManager.instance.isOnSlope = isOnSlope;

      
        if (PlayerReferenceManager.instance.currentState == PlayerState.Locomotion)
        {
            timeSinceLastGrounded = Mathf.Clamp(timeSinceLastGrounded, 0, coyoteTime);
            if (!isGrounded && !isBounced)
            {
                if (timeSinceLastGrounded >= coyoteTime)
                {
                    //stop coyote time
                    isOnCoyoteTime = false;
                }

                else
                {
                    isOnCoyoteTime = true;
                    timeSinceLastGrounded += Time.deltaTime;
                }

            }

            else
            {
                isOnCoyoteTime = false;
                timeSinceLastGrounded = 0;
            }

            
            //PlayerReferenceManager.instance.isOnCoyoteTime = isOnCoyoteTime;
            
        }


        

    }

    void HandleDrag()
    {
        if (isGrounded)
        {
            movementMultiplier = groundMultiplier;
            airTime = 0;

        }

        else
        {
            airTime += Time.deltaTime;

            if (airTime >= airThreshold)
            {
                movementMultiplier = airMultiplier;
            }
        }
        
        airTime = Mathf.Clamp(airTime, 0, airThreshold);
    }

    // Update is called once per frame
    void Movement()
    {




        Vector3 currentVelocity = moveVelocity;
        //movement

        //Vector3 moveVel = moveDir * (speed * Time.fixedDeltaTime) + (verticalVelocity * Time.fixedDeltaTime * Vector3.up);

        //Vector3 finalVel = collisionDetection.CollideAndSlide(moveVel, _rigidbody.position, 0, true, moveVel);


        // _rigidbody.MovePosition(_rigidbody.position + finalVel);


        currentVelocity.y = 0;
        //Vector3 currentVelocity = _rigidbody.linearVelocity;


        

        var currAccel = input.magnitude > 0.01f ? acceleration * sturdyRatio : deceleration;
        
        Vector3 targetVelocity = Vector3.MoveTowards(currentVelocity, GetInputVector() * (speed * sturdyRatio * movementMultiplier), currAccel);


        //SlopeCheck(targetVelocity);
        
        
        moveVelocity = targetVelocity + (transform.up * (verticalVelocity));


        if (PlayerReferenceManager.instance.launched)
        {
            moveVelocity += (launchVelocityHead * Time.deltaTime);
        }
        //_rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

        _characterController.Move(moveVelocity  * Time.deltaTime);

        

        /*body.position = Vector3.Lerp(body.position, transform.position - (bodyHeadSpacing * transform.forward), bodyReactTime * Time.deltaTime);
        tail.position = Vector3.Lerp(tail.position, body.position - (tailBodySpacing * transform.forward), tailReactTime * Time.deltaTime);*/

        //PlayerReferenceManager.instance.UpdateCachedHeadPositions(_rigidbody.position, transform.rotation);

        UpdateSegments();
        //rotation

    }

    private void HandleRotation()
    {
        if (PlayerReferenceManager.instance.currentState == PlayerState.Stretching)
            return;


        if(isClimbing)
            return;
        if (input != Vector2.zero)
        {
            Vector3 direction = GetInputVector().normalized;
            //direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);
            transform.rotation = Quaternion.Slerp(_characterController.transform.rotation, targetRotation, turnSpeed * Time.smoothDeltaTime);

            
           

         
            /*body.rotation =  Quaternion.Slerp(body.rotation, transform.rotation, bodyReactTime * Time.deltaTime);
            tail.rotation =  Quaternion.Slerp(tail.rotation, transform.rotation, tailReactTime * Time.deltaTime);*/
        }
        




    }

    public void Bounce(float height)
    {
        isBounced = true;
        verticalVelocity = 0;
        verticalVelocity += Mathf.Sqrt(-2 * PlayerReferenceManager.instance.gravity * height);
    }


    


    void UpdateSegments()
    {
        if (PlayerReferenceManager.instance.currentState != PlayerState.Locomotion)
            return;
        if (PlayerReferenceManager.instance.launched)
        {
            Vector3 targetVelocityBody = (transform.up * verticalVelocity) + launchVelocityBody;
            Vector3 targetVelocityTail = (transform.up * verticalVelocity) + launchVelocityTail;
            
            PlayerReferenceManager.instance.bodySegment.Move(targetVelocityBody * Time.deltaTime);
            PlayerReferenceManager.instance.tailSegment.Move(targetVelocityTail * Time.deltaTime);
        }

        else
        {
             for (int i = 1; i < segments.Count; i++)
             {
                 Vector3 pos = segments[i].characterController.transform.position;
                 Vector3 prevPos = segments[i - 1].characterController.transform.position;
                 //Vector3 forward = segments[i - 1].t.forward;


                 var spacing = Mathf.Abs(segments[i].spacingToNextSegment);

                 Vector3 currentDir = pos - prevPos;



                 Vector3 desiredPos = prevPos + (currentDir.normalized * spacing);






                 //segments[i].rb.AddForce(direction * Mathf.Lerp() , ForceMode.Acceleration);


                 //check if overshooting
                 Vector3 velocity = segments[i].characterController.velocity;
                 segments[i].springConnector.UpdateSpringVector(Time.deltaTime, ref pos, ref velocity, desiredPos);

                 // segments[i].rb.MovePosition(pos);
                // 

                 segments[i].isGrounded = (Physics.CheckSphere(segments[i].groundCheck.position, groundRadius,
                     PlayerReferenceManager.instance.groundMask));

                 if (!segments[i].isGrounded)
                 {
                     if (isGrounded)
                     {
                         velocity += transform.up * PlayerReferenceManager.instance.gravity * Time.deltaTime;
                     }
                 }
                 
                 segments[i].characterController.Move(velocity * Time.deltaTime);
            
            
                
                



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



                 Vector3 direction = targetPos.normalized;
                 direction.y = 0;

                 //rotation
                 if (moveDir.magnitude > 0.001f)
                 {
                     Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);


                     segments[i].t.transform.rotation = (Quaternion.Slerp(segments[i].t.rotation, targetRotation, turnSpeed * movementMultiplier * Time.deltaTime));

                 }

             }

             if (segments[1].isGrounded)
             {
                 launchVelocityBody = Vector3.zero;
             }

             if (segments[2].isGrounded)
             {
                 launchVelocityTail = Vector3.zero;
             }
        }
       
    }

    void CollisionDetection()
    {
        if (!isClimbing && moveDir.magnitude > 0.001f)
        {
            if (Physics.Raycast(transform.position, transform.forward, out climbHit, detectionRange, silkMask))
            {
                // _characterController.Move(hitInfo.point - transform.position);

                if (Vector3.Dot(transform.forward, climbHit.normal) < Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad))
                {
                    isClimbing = true;
                    _characterController.Move(climbHit.point - transform.position);
                    Quaternion rot  = Quaternion.FromToRotation(transform.up, climbHit.normal) * transform.rotation;
                    //transform.up = climbHit.normal;
                }
               
            }
        }

        else if(isClimbing)
        {
            if (!isGrounded)
            {
                isClimbing = false;
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }

            else
            {
                transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                climbHit = groundHit;
                
            }
        }
        
        
       
    }

    private bool IsOnSlope()
    {
        if (Physics.Raycast(transform.position, -transform.up, out slopeHit,
                characterHeight * 0.5f + 0.2f, PlayerReferenceManager.instance.groundMask))
        {
            if(slopeHit.normal != transform.up)
            {
                if (isClimbing)
                {
                    slopeAngle = Vector3.Angle(transform.up, slopeHit.normal);
                }
               
                slopeAngle = Vector3.Angle(slopeHit.normal, Vector3.up);

                if (slopeAngle > 0)
                {
                    if (slopeAngle < GroundAngleLimit)
                    {
                        return true;
                    }
                }
               

                return false;
            }
        }

        return false;
    }

    Vector3 GetInputVector()
    {
        Vector3 inputVector = moveDir;
        if (IsOnSlope())
        {
            inputVector = Vector3.ProjectOnPlane(moveDir, slopeHit.normal);
        }

        if (isClimbing)
        {
            var inputDir = Vector3.ProjectOnPlane(inputVector, climbHit.normal);
            inputVector = inputDir;
        }
        return inputVector;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
    
    
    
}