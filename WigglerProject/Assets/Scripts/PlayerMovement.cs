using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float turnSpeed;
    
    private CollisionDetection collisionDetection;
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
    public Transform groundCheck;


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
        
        collisionDetection = _rigidbody.transform.GetComponent<CollisionDetection>();
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


        verticalVelocity += gravity * Time.fixedDeltaTime;

        if (isGrounded)
        {
            verticalVelocity = 0;
        }
       
       // _rigidbody.AddForce(gravity * movementMultiplier * Time.fixedDeltaTime  * Vector3.up, ForceMode.Acceleration);
       
        
        
        Vector3 moveVel = moveDir * (speed * Time.fixedDeltaTime);


        Vector3 finalVel = collisionDetection.CollideAndSlide(moveVel, _rigidbody.position, 0, true, moveVel);
        _rigidbody.MovePosition(_rigidbody.position + finalVel);
        
        
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
            _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
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
            
            var spacing = Mathf.Abs(segments[i].spacingToNextSegment);
            
            Vector3 targetPos = prevPos - (segments[i - 1].t.forward) * spacing;
            Vector3 direction = (targetPos - pos).normalized;
            direction.y = 0;
            //var scaledDirection = Vector3.Scale();
            
            
            Vector3 finalVelocity = segments[i].collisionDetection.CollideAndSlide(targetPos, pos, 0, true, targetPos);
            if (finalVelocity == Vector3.zero)
            {
                finalVelocity = pos;
            }
            segments[i].rb.MovePosition(finalVelocity);
          
            //rotation
            if (direction.magnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                
                
                segments[i].rb.MoveRotation(Quaternion.Slerp(segments[i].rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
                
            }

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}


