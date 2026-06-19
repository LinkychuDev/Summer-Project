using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float turnSpeed;
    

    private List<Segment> segments = new List<Segment>();
    
    private CharacterController _characterController;

    private Vector2 input;

    private Vector3 moveDir;

    private Transform camera;

    private Vector3 moveVelocity;

    
   
    public float gravity = -9.81f;

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
    
    /*Vector3 targetBodyPosition;
    Vector3 targetTailPosition;*/
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        segments.Add(PlayerStateReference.instance.segments[0]);
        segments.Add(PlayerStateReference.instance.segments[1]);
        segments.Add(PlayerStateReference.instance.segments[2]);
        camera = Camera.main.transform;
    }

    private void Update()
    {
        
        if(PlayerStateReference.instance.state == PlayerState.Stretching)
            return;
        input = moveInput.action.ReadValue<Vector2>();
        Movement();
    }

    // Update is called once per frame
    void Movement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        HandleRotation();


        if (isGrounded)
        {
            if (moveVelocity.y < -2f)
            {
                moveVelocity.y = -2f;
            }
        }
        

        moveVelocity.y += gravity * Time.deltaTime;
        
        Vector3 finalVelocity = moveDir * speed + moveVelocity.y * Vector3.up;
        _characterController.Move(finalVelocity * Time.deltaTime);
        
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            /*body.rotation =  Quaternion.Slerp(body.rotation, transform.rotation, bodyReactTime * Time.deltaTime);
            tail.rotation =  Quaternion.Slerp(tail.rotation, transform.rotation, tailReactTime * Time.deltaTime);*/
        }

       
    }

    private void LateUpdate()
    {
        if(PlayerStateReference.instance.state == PlayerState.Stretching)
            return;
        UpdateSegments();
    }

    void UpdateSegments()
    {
        for (int i = 1; i < segments.Count; i++)
        {
            Vector3 velocity = Vector3.zero;
            
            
            Vector3 pos = segments[i].t.position;
            pos.y = 0;
            Vector3 prevPos = segments[i - 1].t.position;
            prevPos.y = 0;
            
            var maxDistance = Mathf.Abs(segments[i].spacingToNextSegment);

            var direction = (prevPos - pos);
            direction.y = 0;
            
            Debug.Log("direction: " + direction);
            var distance = direction.magnitude;
            Debug.Log("distance: " + distance);    
            
            if (distance > maxDistance)
            {
                //desired position
                var targetPosition = prevPos + direction.normalized * maxDistance;
                Debug.Log("targetPosition: " + targetPosition);
                var targetDirection = targetPosition - pos;
                Debug.Log("targetDirection: " + targetDirection);
                velocity = targetDirection;
                Debug.Log("velocity: " + velocity);
                //segments[i].rb.MovePosition(prevPos - (maxDistance * direction.normalized));


            }
            
            segments[i].characterController.Move(velocity * speed * Time.deltaTime + moveVelocity.y * Vector3.up);
        }
    }

   
}
