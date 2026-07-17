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

    private Rigidbody _rigidbody;

    private Vector2 input;

    private Vector3 moveDir;

    private Transform playerCam;

    private Vector3 moveVelocity;

    private float verticalVelocity;



    [SerializeField] private float groundMultiplier = 1f;
    [SerializeField] private float airMultiplier = 0.2f;
    private float movementMultiplier;

    public bool isGrounded;


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

   


    public bool isOnCoyoteTime;

    float timeSinceLastGrounded;

    /*Vector3 targetBodyPosition;
    Vector3 targetTailPosition;*/
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


        segments.Add(PlayerReferenceManager.instance.segments[0]);
        segments.Add(PlayerReferenceManager.instance.segments[1]);
        segments.Add(PlayerReferenceManager.instance.segments[2]);

        _rigidbody = segments[0].rb;
        playerCollider = _rigidbody.GetComponent<SphereCollider>();
        sphereRadius = playerCollider.radius;
        playerCam = Camera.main.transform;


        //collisionDetection = _rigidbody.transform.GetComponent<CollisionDetection>();
    }

    private void Update()
    {

        if (PlayerReferenceManager.instance.currentState != PlayerState.Locomotion)
            return;
        HandleInput();



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

    private void FixedUpdate()
    {
        //isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        GroundCheck();

        if (PlayerReferenceManager.instance.currentState != PlayerState.Locomotion)
            return;
        HandleGravity();
        HandleRotation();
        HandleDrag();
        Movement();


    }


    void HandleGravity()
    {
        if (isGrounded)
        {
            if (isOnSlope)
            {
                // Keep player pinned to slope
                _rigidbody.AddForce(Vector3.down * slopeDownForce, ForceMode.Acceleration);
            }
            else
            {
                // Normal gravity
                _rigidbody.AddForce(PlayerReferenceManager.instance.gravity * Vector3.up, ForceMode.Acceleration);
            }
        }
        else
        {
            // Full gravity in air
            _rigidbody.AddForce(PlayerReferenceManager.instance.gravity * Vector3.up, ForceMode.Acceleration);
        }


        if (PlayerReferenceManager.instance.launched)
        {
            PlayerReferenceManager.instance.launched = false;
        }
    }

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, PlayerReferenceManager.instance.groundMask);
        PlayerReferenceManager.instance.isGrounded = isGrounded;


        if (PlayerReferenceManager.instance.currentState == PlayerState.Locomotion)
        {
            if (isGrounded == false)
            {
                if (timeSinceLastGrounded > coyoteTime)
                {
                    //stop coyote time
                    isOnCoyoteTime = false;
                    timeSinceLastGrounded = 0;
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

            
            PlayerReferenceManager.instance.isOnCoyoteTime = isOnCoyoteTime;

            isOnSlope = IsOnSlope(moveDir, ref slopeDir);
        }


        

    }

    void HandleDrag()
    {
        if (isGrounded)
        {
            _rigidbody.linearDamping = groundDrag;

            if (isOnSlope)
            {
                movementMultiplier = slopeMultiplier;
            }

            else
            {
                movementMultiplier = groundMultiplier;
            }

        }

        else
        {
            _rigidbody.linearDamping = airDrag;
            movementMultiplier = airMultiplier;
        }
    }

    // Update is called once per frame
    void Movement()
    {





        //movement

        //Vector3 moveVel = moveDir * (speed * Time.fixedDeltaTime) + (verticalVelocity * Time.fixedDeltaTime * Vector3.up);

        //Vector3 finalVel = collisionDetection.CollideAndSlide(moveVel, _rigidbody.position, 0, true, moveVel);


        // _rigidbody.MovePosition(_rigidbody.position + finalVel);



        Vector3 currentVelocity = _rigidbody.linearVelocity;
        currentVelocity.y = 0;

        Vector3 targetDirection = moveDir.normalized;





        Vector3 targetVelocity = targetDirection * (speed * movementMultiplier * input.magnitude);
        Vector3 velocityChange = targetVelocity - currentVelocity;



        velocityChange = Vector3.ClampMagnitude(velocityChange, maxSpeed);

        _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);


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

            if (isOnSlope)
            {
                direction = slopeDir;
            }

            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(_rigidbody.transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            /*body.rotation =  Quaternion.Slerp(body.rotation, transform.rotation, bodyReactTime * Time.deltaTime);
            tail.rotation =  Quaternion.Slerp(tail.rotation, transform.rotation, tailReactTime * Time.deltaTime);*/
        }




    }



    void UpdateSegments()
    {
        if (PlayerReferenceManager.instance.currentState != PlayerState.Locomotion)
            return;
        if(PlayerReferenceManager.instance.launched)
            return;
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
            segments[i].rb.linearVelocity = velocity;




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


                segments[i].t.transform.rotation = (Quaternion.Slerp(segments[i].t.rotation, targetRotation, turnSpeed * movementMultiplier * Time.fixedDeltaTime));

            }

        }
    }

    public bool IsOnSlope(Vector3 inputDir, ref Vector3 slopeDir)
    {
        Physics.Raycast(_rigidbody.position, Vector3.down, out RaycastHit hit, sphereRadius * 0.5f + slopeGroundDistance + slopeOffset,
            PlayerReferenceManager.instance.groundMask);

        if (hit.normal != Vector3.up)
        {
            var angle = Vector3.Angle(hit.normal, Vector3.up);

            if (angle < GroundAngleLimit && angle != 0)
            {
                slopeDir = Vector3.ProjectOnPlane(inputDir, hit.normal).normalized;

                Debug.Log("Slope Point: " + hit.point);
                Debug.Log("Slope Point Local: " + _rigidbody.transform.InverseTransformPoint(hit.point));

                // transform.forward = slopeDir;
                Debug.Log("Is On Slope");
                return true;
            }


        }



        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}