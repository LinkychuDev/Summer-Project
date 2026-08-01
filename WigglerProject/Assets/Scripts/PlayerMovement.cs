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

    //private CharacterController _characterController;

    private Vector2 input;

    [SerializeField]private Vector3 moveDir;

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

    private float currentDrag;

    private SphereCollider playerCollider;
    private Vector3 slopeDir;
    private RaycastHit slopeHit;
   // private RaycastHit groundHit;

    //private bool isOnSlope;
    [SerializeField] private float slopeGroundDistance = 0.5f;


    [SerializeField] private float slopeMultiplier = 1.5f;

    private float sphereRadius;
    public bool isOnSlope;

    [SerializeField] private float slopeOffset = 0.1f;

    [SerializeField]private float slopeDownForce = 150f;

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
    private Vector3 climbUp;
    private Vector3 climbRight;
    public float slopeAngle;

     private Vector3 climbDir;
     RaycastHit climbHit;
    /*Vector3 targetBodyPosition;
    Vector3 targetTailPosition;*/
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public Transform parent;
    private Rigidbody rb;
   
    [SerializeField] private float minGroundAngle;
    
    //public Vector3 gravityDirection = Vector3.down;
    [SerializeField] private Vector3 groundNormal;

    private void OnEnable()
    {
        PlayerController.isOnSturdyEvent += OnSturdyEvent;
        LaunchVelocity += OnSwingLaunch;
        PlayerController.ClimbEvent += b => isClimbing = b;
    }

    private void OnDisable()
    {
        PlayerController.isOnSturdyEvent -= OnSturdyEvent;
        LaunchVelocity -= OnSwingLaunch;
        PlayerController.ClimbEvent -= b => isClimbing = b;
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

        rb = segments[0].rb;
        playerCollider = rb.GetComponent<SphereCollider>();
        sphereRadius = playerCollider.radius;
        playerCam = Camera.main.transform;
        
        characterHeight = playerCollider.radius;
        
        
        //collisionDetection = _rigidbody.transform.GetComponent<CollisionDetection>();
    }


    void Update()
    {
        if (PlayerReferenceManager.instance.currentState != PlayerState.Locomotion)
            return;
        HandleInput();
    }
    private void FixedUpdate()
    {
        
        GroundCheck();
        
        
        //CollisionDetection();
        HandleGravity();

        
        
        
        if (PlayerReferenceManager.instance.currentState != PlayerState.Locomotion)
            return;
        HandleRotation();
        HandleDrag();
       // AlignToSurface();
        Movement();
      

    }

    void HandleInput()
    {

        input = InputManager.instance.controls.Gameplay.Move.ReadValue<Vector2>();


        moveDir = GetInputVector();

        //
        //moveDir.y = 0;



    }

   

    void HandleGravity()
    {
        if (isGrounded)
        {
            PlayerReferenceManager.instance.launched = false;

            /*if (verticalVelocity < 0)
            {
                verticalVelocity = (groundVelocity);
            }


            if (PlayerReferenceManager.instance.launched)
            {
                launchVelocityHead = Vector3.zero;
                launchVelocityBody = Vector3.zero;
                launchVelocityTail = Vector3.zero;
                PlayerReferenceManager.instance.launched = false;
            }*/
            
            
            if (isOnSlope  && (input.sqrMagnitude > 0.001f))
            {
                rb.AddForce(transform.up * -slopeDownForce, ForceMode.Acceleration);
            }
        }


        else
        {
            if (PlayerReferenceManager.instance.useGravity)
            {
                Debug.Log("Applying gravity");
                rb.AddForce(transform.up * (PlayerReferenceManager.instance.gravity), ForceMode.Acceleration);
            }
        }


    }

    //separate visuals and character
    private bool IsGrounded()
    {
        
       
       
        if (Physics.CheckSphere(groundCheck.position, groundRadius, PlayerReferenceManager.instance.groundMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Ground");
            
            if (Physics.Raycast(groundCheck.position, -transform.up, out slopeHit,  groundDistance, PlayerReferenceManager.instance.groundMask))
            {
                slopeAngle = Vector3.Angle(slopeHit.normal, transform.up);

                groundNormal = slopeHit.normal;
                
                if(groundNormal != transform.up)
                {
                
                    if (slopeAngle > 0)
                    {
                        if (slopeAngle < GroundAngleLimit && slopeAngle > minGroundAngle)
                        {
                            isOnSlope = true;
                            return true;
                        }

                        else if(slopeAngle < GroundAngleLimit && slopeAngle < minGroundAngle)
                        {
                            isOnSlope = false;
                            return true;
                        }

                        else
                        {
                            isOnSlope = false;
                            return false;
                        }
                    
                    }
                    
                }
                
                else
                {
                    //even flat terrain
                    isOnSlope = false;
                
                    return true;
                }

            }
            
           
           
        }

        isOnSlope = false;
        return false;
    }

    Vector3 GetInputVector()
    {
        /*if (isClimbing)
        {
            inputVector = transform.right * input.x + transform.forward * input.y;
            Debug.Log("Climbing Input: " + inputVector);
        }*/
        
        Vector3 forward = playerCam.transform.forward;
        Vector3 right = playerCam.transform.right;
        forward.y = 0;
        forward.Normalize();
        right.y = 0;
        right.Normalize();
        Vector3 up = playerCam.transform.up;
       

        if (isClimbing)
        {
           
            //Debug.Log("Right: " + right);
            
            Debug.Log("Climb Dir:  " + climbDir);
            
            Debug.Log("Player Cam Forward:" + playerCam.transform.forward);
            Debug.Log("Player Cam Right:" + playerCam.transform.right);
            forward = playerCam.transform.forward;
            
            Debug.Log("Forward Dir:  " + forward);

            right = Vector3.Cross(climbDir, forward);
            
            Debug.Log("Right Dir:  " + right);

            right = -Vector3.ProjectOnPlane(right, climbDir);
            forward = -Vector3.ProjectOnPlane(forward, climbDir);

        }

       
        var inputVector = right * input.x + forward * input.y;

        
        if (isOnSlope)
        {
            inputVector = Vector3.ProjectOnPlane(inputVector, slopeHit.normal);
        }

       
        
       
        return inputVector.normalized;
    }


  

    public void GroundCheck()
    {
        //isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, PlayerReferenceManager.instance.groundMask);
        
       //== PlayerReferenceManager.instance.isGrounded = isGrounded;
       
       isGrounded = groundedCheck = IsGrounded();
        if (isBounced)
        {
            if (rb.linearVelocity.y < 0 && isGrounded)
            {
                isBounced = false;
            }
        }
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
        
    }

    // Update is called once per frame
    void Movement()
    {

        

        Vector3 currentVelocity = rb.linearVelocity;
        
        //currentVelocity
        Vector3 targetVelocity = moveDir * (movementMultiplier * input.sqrMagnitude * speed);


        //targetVelocity = Vector3.ClampMagnitude(targetVelocity , maxSpeed);
        //Vector3 velocityChange = AlignToSurface(targetVelocity - currentVelocity);


        Vector3 velocityChange = targetVelocity - currentVelocity;

        velocityChange = Vector3.ClampMagnitude(velocityChange, maxSpeed);

        Debug.Log("VelocityChange: " + velocityChange);

        //velocityChange.y = currentVelocity.y;
        //Vector3 currentVelocity =  rb.linearVelocity + ( * targetVelocity) ;



        //targetVelocity = Vector3.ClampMagnitude(targetVelocity, maxSpeed);


        //Vector3 velocityChange = targetVelocity - currentVelocity;

        moveVelocity = velocityChange * (1 - currentDrag);
        //SlopeCheck(targetVelocity)
        rb.AddForce(moveVelocity, ForceMode.VelocityChange);

        /*var velocity = rb.linearVelocity;

        if (isClimbing)
        {
            velocity.x *= (1 - currentDrag);
            velocity.y *= (1 - currentDrag);
            velocity.z *= (1 - currentDrag);
        }

        else
        {
            velocity.x *= (1 - currentDrag);
            velocity.z *= (1 - currentDrag);
        }*/

          
        

        //rb.linearVelocity = velocity;
            
        //Debug.Log("Velocity: " + velocity);



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
        
        
        if (input != Vector2.zero)
        {
            Vector3 direction = moveDir;
            var targetRotation =
                //direction.y = 0;
                Quaternion.LookRotation(direction, transform.up);
          
           transform.rotation = Quaternion.Slerp(rb.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            
           

         
            /*body.rotation =  Quaternion.Slerp(body.rotation, transform.rotation, bodyReactTime * Time.deltaTime);
            tail.rotation =  Quaternion.Slerp(tail.rotation, transform.rotation, tailReactTime * Time.deltaTime);*/
        }
        




    }

    public void Bounce(float height)
    {
        isBounced = true;
        //verticalVelocity = 0;
       
        float bounceHeight = rb.mass * (Mathf.Sqrt(-2 * PlayerReferenceManager.instance.gravity * height) - (Time.fixedDeltaTime * PlayerReferenceManager.instance.gravity) / 2);
       // Debug.Log("Bounce height: " + bounceHeight);
        
        //Debug.Log(rb.linearDamping);
        
        Debug.Log(height);
        rb.AddForce(transform.up * bounceHeight , ForceMode.VelocityChange);
    }


    


    
    

    void AlignToSurface()
    {
        Debug.Log("Aligning Surface");
       Quaternion gravityRotation = Quaternion.FromToRotation(transform.up, -transform.up) * rb.transform.rotation;
       Quaternion surfaceRotation = Quaternion.FromToRotation(transform.up, groundNormal) * rb.transform.rotation;
       
       Quaternion finalRotationDelta = Quaternion.Slerp(gravityRotation, surfaceRotation, turnSpeed);
       
       transform.rotation = Quaternion.Slerp(transform.rotation, finalRotationDelta, turnSpeed * Time.smoothDeltaTime);

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        var tempPosition = transform.position + (Vector3.up * characterHeight) + (Vector3.up * slopeOffset);
        
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

                 segments[i].isGrounded = Physics.CheckSphere(segments[i].groundCheck.position, groundRadius, PlayerReferenceManager.instance.groundMask);
                 if (!segments[i].isGrounded)
                 {
                     if (isGrounded)
                     {
                         segments[i].rb.AddForce( transform.up * PlayerReferenceManager.instance.gravity, ForceMode.Acceleration);
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
    
    

    public void ChangeGravity(Vector3 newDir)
    {
        
        Quaternion rotationDifference = Quaternion.FromToRotation(transform.up, -newDir);
			
        rb.transform.rotation = rotationDifference * rb.transform.rotation;
        //rb.constraints = RigidbodyConstraints.FreezeRotation

        climbDir = -newDir;
        
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
}