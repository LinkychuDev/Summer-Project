using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class MovementBase : MonoBehaviour
{
    public float speed;
    public float maxSpeed;
    public float turnSpeed;
    internal List<Segment> segments = new List<Segment>();
    
    internal Vector2 input;

    internal Vector3 moveDir;

    internal Transform playerCam;

    public Vector3 moveVelocity;
    
    internal float currentDrag;
    
    internal Vector3 slopeDir;
    internal RaycastHit slopeHit;

 

    internal float sphereRadius;
    public bool isOnSlope;

    protected float slopeDownForce = 80;

    public float coyoteTime = 0.3f;


    public float airTime;
    public float airThreshold = 2f;


    public bool isOnCoyoteTime;

    float timeSinceLastGrounded;


    internal bool isOnSturdy;

    internal float sturdyRatio;


    public bool isBounced;  
    Vector3 launchVelocityHead, launchVelocityBody, launchVelocityTail;


    internal float characterHeight;

    internal Vector3 climbDir;
    /*Vector3 targetBodyPosition;
    Vector3 targetTailPosition;*/
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    internal Rigidbody rb;
    //public Vector3 gravityDirection = Vector3.down;
    public Vector3 groundNormal;
    public bool isGrounded;
    public Transform groundCheck;
    public float groundRadius;
    public float groundDistance;
    
    internal float slopeAngle;
    internal float minGroundAngle = 0f;
    internal float GroundAngleLimit = 60f;
    public bool isClimbing;
    internal SphereCollider playerCollider;
    internal bool canResetBounce;
    public Vector3 vertVelocity;
    [SerializeField] private float groundVelocity = -2f;


    protected virtual void OnEnable()
    {
        PlayerController.isOnSturdyEvent += OnSturdyEvent;
       
      //  PlayerController.ClimbEvent += ClimbEvent;
    }

    protected virtual void OnDisable()
    {
        PlayerController.isOnSturdyEvent -= OnSturdyEvent;
       // PlayerController.ClimbEvent -= b => isClimbing = b;
    }
    
    
    private void OnSturdyEvent(float arg1, bool arg2)
    {
        sturdyRatio = arg1;
        isOnSturdy = arg2;
    }

    protected virtual void Start()
    {
        segments.Add(PlayerReferenceManager.instance.segments[0]);
        segments.Add(PlayerReferenceManager.instance.segments[1]);
        segments.Add(PlayerReferenceManager.instance.segments[2]);

        rb = segments[0].rb;
        playerCollider = rb.GetComponent<SphereCollider>();
        sphereRadius = playerCollider.radius;
        playerCam = Camera.main.transform;
        
        characterHeight = playerCollider.radius;
       
    }

    protected bool IsGrounded()
    {
        
       
       
        if (Physics.CheckSphere(groundCheck.position, groundRadius, PlayerReferenceManager.instance.groundMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Ground");
            
            if (Physics.Raycast(groundCheck.position, -transform.up, out slopeHit,  groundDistance, PlayerReferenceManager.instance.groundMask, QueryTriggerInteraction.Ignore))
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
                            Debug.Log("Ground Check Fail, not min ground angle");
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

        Debug.Log("Ground Check Fail");
        isOnSlope = false;
        return false;
    }

    internal virtual Vector3 GetInputVector()
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
           
            forward = Vector3.ProjectOnPlane(playerCam.transform.up, climbDir);
            
            Debug.Log("Forward Dir:  " + forward);

            right = Vector3.ProjectOnPlane(playerCam.transform.right, climbDir);
            
            Debug.Log("Right Dir:  " + right);
            

        }

       
        var inputVector = right * input.x + forward * input.y;

        
        if (isOnSlope)
        {
            inputVector = Vector3.ProjectOnPlane(inputVector, slopeHit.normal);
        }

       
        
        Debug.Log("Input Vector:  " + inputVector);
        return inputVector.normalized;
    }


  

    public void GroundCheck()
    {
        //isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, PlayerReferenceManager.instance.groundMask);
        
       //== PlayerReferenceManager.instance.isGrounded = isGrounded;
       
       
       
        isGrounded = IsGrounded();

        if (canResetBounce)
        {
            if (isGrounded)
            {
                canResetBounce = false;
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
    
    
    protected void HandleGravity()
    {
        if (isGrounded)
        {
            PlayerReferenceManager.instance.launched = false;
            
            Vector3 gravityVelocity = Vector3.Project(rb.linearVelocity, transform.up);

            if (isBounced)
            {
                isBounced = false;
            }
            
            float signedValue = Vector3.Dot(gravityVelocity, transform.up);
            Debug.Log("signed Value: " + signedValue);
            
            if (!isOnSlope)
            {
                if (signedValue < 0)
                {
                   //vertVelocity = transform.up * groundVelocity;
                }
           
                //rb.AddForce(-PlayerReferenceManager.instanc   e.playerGravityDir * slopeDownForce, ForceMode.VelocityChange);
            }
            

            else
            {
                Debug.Log("0'd out movement");
                if (signedValue < 0)
                {
                    //vertVelocity = slopeHit.normal * groundVelocity;
                }
                
            }
          
            


        }

        
        else if (PlayerReferenceManager.instance.useGravity && !isClimbing)
        {

            
          rb.AddForce(PlayerReferenceManager.instance.gravity * PlayerReferenceManager.instance.playerGravityDir, ForceMode.Acceleration);
            
        }

    }

    protected virtual void Movement()
    {

    }

    protected virtual void HandleRotation()
    {
        if (PlayerReferenceManager.instance.currentState == PlayerState.Stretching)
            return;
        
        
        if (input != Vector2.zero)
        {
            Vector3 direction = moveDir;

            if (!isClimbing)
            {
                direction.y = 0;
            }

            var targetRotation = 
                Quaternion.LookRotation(direction, PlayerReferenceManager.instance.playerGravityDir);
          
            transform.rotation = Quaternion.Slerp(rb.transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            
           

         
            /*body.rotation =  Quaternion.Slerp(body.rotation, transform.rotation, bodyReactTime * Time.deltaTime);
            tail.rotation =  Quaternion.Slerp(tail.rotation, transform.rotation, tailReactTime * Time.deltaTime);*/
        }

    }

}