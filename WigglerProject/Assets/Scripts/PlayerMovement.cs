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
    public float turnSpeed;
    
    
    private List<Segment> segments = new List<Segment>();

    private Rigidbody _rigidbody;

    private Vector2 input;

    private Vector3 moveDir;

    private Transform camera;

    private Vector3 moveVelocity;

    private float verticalVelocity;
   
    public float gravity = -9.81f;

    [SerializeField] private float movementMultiplier = 50f;
    public bool isGrounded;
    /*
    public float bodyHeadSpacing = -1.5f;
    public float tailBodySpacing = -1f;
    public float tailReactTime = 0.5f;
    public float bodyReactTime = 0.25f;*/
    public InputActionReference moveInput;
    
    
    public LayerMask groundMask;
    public float groundDistance = 0.4f;
    public float groundRadius = 0.2f;
    public Transform groundCheck;

    public float GroundAngleLimit = 45;

    public float airDrag = 0.3f;

    public float groundDrag = 6f;

 
    /*Vector3 targetBodyPosition;
    Vector3 targetTailPosition;*/
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        
        segments.Add(PlayerStateReference.instance.segments[0]);
        segments.Add(PlayerStateReference.instance.segments[1]);
        segments.Add(PlayerStateReference.instance.segments[2]);

        _rigidbody = segments[0].rb;
        camera = Camera.main.transform;
        
        //collisionDetection = _rigidbody.transform.GetComponent<CollisionDetection>();
    }

    private void Update()
    {
        
        if(PlayerStateReference.instance.state == PlayerState.Stretching)
            return;
        input = moveInput.action.ReadValue<Vector2>();
      
       
    }

    private void FixedUpdate()
    {
        //isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        GroundCheck();
        HandleDrag(); 
        if(PlayerStateReference.instance.state == PlayerState.Stretching)
            return;
        Movement();
    }


    void GroundCheck()
    {
        if (Physics.SphereCast(groundCheck.position, groundRadius, Vector3.down, out RaycastHit hit, groundDistance,
                groundMask))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) < GroundAngleLimit)
            {
                float distance = _rigidbody.position.y - hit.point.y;
                
                //_rigidbody.MovePosition(new Vector3(_rigidbody.position.x, _rigidbody.position.y + distance, _rigidbody.position.z));
                isGrounded = true;
            }

            else
            {
                isGrounded = false;
            }
            
        }

        else
        {
            isGrounded = false;
        }
    }

    void HandleDrag()
    {
        if (isGrounded)
        {
            _rigidbody.linearDamping = groundDrag;
        }

        else
        {
            _rigidbody.linearDamping = airDrag;
        }
    }

    // Update is called once per frame
    void Movement()
    {
        HandleRotation();


       

        /*if (isGrounded)
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }
           
            
        }
        
        else
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;
        }*/
        
        //gravity
        _rigidbody.AddForce(gravity  * Vector3.up, ForceMode.Acceleration);
       
        
        
        
        //movement
        
        //Vector3 moveVel = moveDir * (speed * Time.fixedDeltaTime) + (verticalVelocity * Time.fixedDeltaTime * Vector3.up);
        
        //Vector3 finalVel = collisionDetection.CollideAndSlide(moveVel, _rigidbody.position, 0, true, moveVel);
        
        
       // _rigidbody.MovePosition(_rigidbody.position + finalVel);
        
       Vector3 _moveDir = moveDir * speed;
       Vector3 _velocity = new Vector3(_moveDir.x, _rigidbody.linearVelocity.y, _moveDir.z) - _rigidbody.linearVelocity;
        
       
       _rigidbody.AddForce(_velocity, ForceMode.VelocityChange);
        UpdateSegments();
        /*body.position = Vector3.Lerp(body.position, transform.position - (bodyHeadSpacing * transform.forward), bodyReactTime * Time.deltaTime);
        tail.position = Vector3.Lerp(tail.position, body.position - (tailBodySpacing * transform.forward), tailReactTime * Time.deltaTime);*/

        //rotation

    }

    private void HandleRotation()
    {
        if(PlayerStateReference.instance.state == PlayerState.Stretching)
            return;
        Vector3 forward = camera.transform.forward;
        Vector3 right = camera.transform.right;
        forward.y = 0;
        forward.Normalize();
        right.y = 0;
        right.Normalize();

          
        moveDir = input.x * right + input.y * forward;
        moveDir.y = 0;
        
        if (input != Vector2.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
            transform.rotation = (Quaternion.Slerp(_rigidbody.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
            /*body.rotation =  Quaternion.Slerp(body.rotation, transform.rotation, bodyReactTime * Time.deltaTime);
            tail.rotation =  Quaternion.Slerp(tail.rotation, transform.rotation, tailReactTime * Time.deltaTime);*/
        }

       
    }
    

    void UpdateSegments()
    {
        for (int i = 1; i < segments.Count; i++)
        {
            Vector3 pos = segments[i].rb.position;
            Vector3 prevPos = segments[i - 1].rb.position;
            //Vector3 forward = segments[i - 1].t.forward;
            
            
            var spacing = Mathf.Abs(segments[i].spacingToNextSegment);
            
            Vector3 currentDir = prevPos - pos;
            
            
            
            Vector3 desiredPos = prevPos + (-currentDir.normalized * spacing);
           


            Vector3 targetPos = desiredPos - pos;
            
           
            
            Vector3 direction = targetPos.normalized;
            direction.y = 0;
            
            
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
            
            
          
            //rotation
            if (direction.magnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                
                
                segments[i].rb.transform.rotation = (Quaternion.Slerp(segments[i].rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
                
            }

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}