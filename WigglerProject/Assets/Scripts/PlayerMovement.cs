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
    private RaycastHit slopeHit;

    private bool isOnSturdy;

    private float sturdyRatio;



    public static Action<Vector3, Vector3, Vector3> LaunchVelocity;


    private bool isBounced; 
    Vector3 launchVelocityHead, launchVelocityBody, launchVelocityTail;
    
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
        //collisionDetection = _rigidbody.transform.GetComponent<CollisionDetection>();
    }

    private void Update()
    {

        GroundCheck();
        if (PlayerReferenceManager.instance.currentState != PlayerState.Locomotion)
            return;
        HandleInput();
       
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

    private bool IsOnSlope()
    {
        Vector3 origin = _characterController.transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(origin, Vector3.down, out slopeHit, _characterController.height * 0.5f + 0.2f, PlayerReferenceManager.instance.groundMask))
        {
            float angle = Vector3.Angle(slopeHit.normal, Vector3.up);
            isOnSlope = angle > 0.1f && angle <= GroundAngleLimit;
        }
        else
        {
            isOnSlope = false;
        }

        return isOnSlope;
    }   

    public void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, PlayerReferenceManager.instance.groundMask);
       // PlayerReferenceManager.instance.isGrounded = isGrounded;

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
        Vector3 targetVelocity = Vector3.MoveTowards(currentVelocity, moveDir * (speed * sturdyRatio * movementMultiplier), currAccel);


        //SlopeCheck(targetVelocity);
        
        moveVelocity = targetVelocity + (Vector3.up * (verticalVelocity));


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


        if (input != Vector2.zero)
        {
            Vector3 direction = moveDir.normalized;

            

            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(_characterController.transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
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
            Vector3 targetVelocityBody = (Vector3.up * verticalVelocity) + launchVelocityBody;
            Vector3 targetVelocityTail = (Vector3.up * verticalVelocity) + launchVelocityTail;
            
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
                         velocity += Vector3.up * PlayerReferenceManager.instance.gravity * Time.deltaTime;
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
                     Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);


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

   

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}