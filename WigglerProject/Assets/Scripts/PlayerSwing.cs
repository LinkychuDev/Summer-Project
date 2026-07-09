using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerSwing : MonoBehaviour
{
    
    public Transform hookPoint;


    
    [SerializeField] private float swingLimit = 95;

    [SerializeField] private float swingSetUpDuration = 0.2f;

    [SerializeField] private float swingLength = 3f;
    
    
    private bool isSwingingSetUp;

    private float currentSwingAngle;
    
    private PlayerController controller;
    Rigidbody headSegment;
    Rigidbody bodySegment;
    Rigidbody tailSegment;

    //public InputActionReference swingInputReference;

    [SerializeField] private float swingForce;
    public Vector2 swingInput;
    
  
    private HingeJoint headJoint, bodyJoint, tailJoint;
    

    [SerializeField] private float gravity = -15f;

    [SerializeField] private float springDamper = 0.2f;

    [SerializeField] private float targetSwingVelocity = 40f;
    
    [SerializeField] private float bodySpring = 100f;
    [SerializeField] private float tailSpring = 4000f;

    [SerializeField] private float springSpeed = 30f;
    JointMotor bodyJointMotor;

    float lastSwingAngle;
    [SerializeField] bool useGravity = true;

    //[SerializeField] private InputActionReference swingHeldInputReference;

    private HoneySwingTest hookReference;
    
     Vector3 accumulatedVelocity;
     Vector3 accumulatedAngularVelocity;
     
     [SerializeField] private float headLaunchRatio,  bodyLaunchRatio, tailLaunchRatio;
     
     float cachedDampingBody, cachedDampingBodyAngular, cachedDampingTail,  cachedDampingTailAngular;
    private bool isHeldDown;
    [SerializeField] private float upwardForce;
    [SerializeField] private float downTime;

   // private bool enterSwingRecoveryMode;

    
    [Header("Collision Detection")]
    //public float groundCheckDistance;
    public float collisionDistance = 0.01f;
    Collider tailCollider;
    Collider headCollider;
    Collider bodyCollider;
    
    [SerializeField] private float swingDamp;
    void Start()
    {
        controller = GetComponent<PlayerController>();
        headSegment = controller.headSegment;
        bodySegment = controller.bodySegment;
        tailSegment = controller.tailSegment;
    }
    
    
     public void StartSwing(HoneySwingTest hook)
    {
        if(hook ==  null)
            return;
        
        isSwingingSetUp = false;
        
       
        //make this a sequence
        //stretchState = StretchState.Swinging;
        //Vector3 anchorPoint = hook.transform.position - hook.swingAnchor;

        hookReference = hook;
        hookPoint = hookReference.swingAnchor;
        hookPoint.transform.localPosition = Vector3.zero;
        hookPoint.rotation = Quaternion.Euler(0, 0, 0);
        headSegment.isKinematic = true;
        headSegment.position = hookPoint.position;

        headCollider = headSegment.GetComponent<SphereCollider>();
        bodyCollider = bodySegment.GetComponent<SphereCollider>();
        tailCollider = tailSegment.GetComponent<SphereCollider>();
     
        
        
        
        
        
        Sequence hookSequence = DOTween.Sequence();
        
        
        
       


        hookSequence.Append(headSegment.transform.DOMove(hookPoint.transform.position, swingSetUpDuration));
       
        //headSegment.transform.SetParent(hookPoint, true);

        headSegment.isKinematic = true;

        headSegment.transform.rotation = Quaternion.Euler(0, 90f, 0);

        Vector3 targetBodyPosition =
            hookPoint.position + (swingLength * (Vector3.down * (controller.bodyOffset)));
        
        Vector3 targetTailPosition = targetBodyPosition + (Vector3.down * (controller.tailOffset));
        hookSequence.Join(bodySegment.DOMove(targetBodyPosition, swingSetUpDuration)).SetEase(Ease.OutBounce);
        
        
        hookSequence.Join(tailSegment.DOMove(targetTailPosition, swingSetUpDuration)).SetEase(Ease.OutBounce);


        hookSequence.OnComplete(() =>
        {

            bodySegment.position = new Vector3(headSegment.position.x, bodySegment.position.y, headSegment.position.z);
            bodySegment.transform.rotation = headSegment.transform.rotation;
            
            
            tailSegment.position = new Vector3(headSegment.position.x, tailSegment.position.y, headSegment.position.z);
            tailSegment.transform.rotation = headSegment.transform.rotation;
            
            bodySegment.isKinematic = false;
            tailSegment.isKinematic = false;


            bodySegment.constraints = RigidbodyConstraints.None;
            tailSegment.constraints = RigidbodyConstraints.None;
            
            bodySegment.useGravity = false;
            tailSegment.useGravity = false;

            bodyJoint = bodySegment.gameObject.AddComponent<HingeJoint>();
            tailJoint = tailSegment.gameObject.AddComponent<HingeJoint>();
            
            bodyJoint.useAcceleration = true;
            tailJoint.useAcceleration = true;

            cachedDampingBody = bodySegment.linearDamping;
            cachedDampingTail = tailSegment.linearDamping;

            cachedDampingBodyAngular = bodySegment.angularDamping;
            cachedDampingTailAngular = tailSegment.angularDamping;
            bodySegment.linearDamping = swingDamp;
            //bodySegment.angularDamping = 0;
            tailSegment.linearDamping = swingDamp;
            //tailSegment.angularDamping = 0.05f;


            bodyJoint.connectedBody = headSegment;
            tailJoint.connectedBody = bodySegment;

            bodyJoint.anchor = new Vector3(0, bodySegment.transform.InverseTransformPoint(headSegment.position).y, 0);
            tailJoint.anchor = new Vector3(0, tailSegment.transform.InverseTransformPoint(bodySegment.position).y, 0);
            
            
            bodyJoint.autoConfigureConnectedAnchor = false;
            tailJoint.autoConfigureConnectedAnchor = false;
            
            bodyJoint.connectedAnchor = Vector3.zero;
            tailJoint.connectedAnchor = Vector3.zero;
            
            
            bodyJoint.connectedMassScale = 0.0001f;
            tailJoint.connectedMassScale = 0.0001f;
            
            
            bodyJoint.spring = new JointSpring
            {
                spring = bodySpring,
                damper = springDamper
            };
            
            
            bodyJoint.useSpring = true;
            
            /*bodyJoint.motor = new JointMotor
            {
                targetVelocity = targetSwingVelocity,
                force = swingSpeed
            };


            */
            bodyJointMotor = bodyJoint.motor;
            //bodyJoint.useMotor = true;

            bodyJoint.limits = new JointLimits()
            {
                min = -swingLimit,
                max = swingLimit
            };
            
            bodyJoint.useLimits = true;
            
            tailJoint.spring = new JointSpring
            {
                spring = tailSpring,
                damper = springDamper
            };
            
            
            tailJoint.useSpring = true;
            
            
            /*
            bodySegment.constraints = RigidbodyConstraints.FreezeRotation;
            tailSegment.constraints = RigidbodyConstraints.FreezeRotation;

            bodySegment.constraints = RigidbodyConstraints.FreezePositionY;
            tailSegment.constraints = RigidbodyConstraints.FreezePositionY;
            */
            

            
            




            
            //bodySegment.AddForce(swingSpeed * , ForceMode.VelocityChange);
            isSwingingSetUp = true;
            
            

        });

        //swingLength = Mathf.Abs(swingLength);


    }


    private void Update()
    {
        if (isSwingingSetUp)
        {
            swingInput = InputManager.instance.controls.Gameplay.Move.ReadValue<Vector2>();
            
        }
        
        isHeldDown = InputManager.instance.controls.Gameplay.Stretch.ReadValue<float>() > 0;
    }


    private void FixedUpdate()
    {
        if (isSwingingSetUp)
        {
            if (isHeldDown)
            {
                SwingEvent();
            }

            else
            {
                ReleaseEvent();
                LaunchEvent();
            }
        }
        
       
    }

    void SwingEvent()
    {


        float input = swingInput.y;
        

        if (useGravity)
        {
            bodySegment.AddForce(gravity * Vector3.up, ForceMode.Acceleration);
            tailSegment.AddForce(gravity * Vector3.up, ForceMode.Acceleration);
        }


        if (Mathf.Abs(input) > 0.01f)
        {
            bodyJoint.useMotor = true;
            bodyJointMotor.force = swingForce * Mathf.Abs(input);
            bodyJointMotor.targetVelocity = targetSwingVelocity * -input;
            bodyJoint.motor = bodyJointMotor;
        }

        else
        {
            bodyJoint.useMotor = false;
        }

        var currentRotation = bodySegment.transform.localRotation;
        var targetRotationMin = bodyJoint.axis * -swingLimit;
        var targetRotationMax = bodyJoint.axis * swingLimit;



        lastSwingAngle = tailJoint.angle;
        accumulatedAngularVelocity = tailSegment.angularVelocity;
        Debug.Log("2Accumulated Velocity: " +accumulatedAngularVelocity);
        Debug.Log("2Joint Velocity: " + tailJoint.velocity);
        Debug.Log("2Joint Body Velocity: " + bodyJoint.velocity);

        //accumulatedVelocity = bodySegment.transform.forward * bodyJoint.velocity;

        //is at max swing


    }

    void ReleaseEvent()
    {
        if(hookReference == null)
            return;
        Debug.Log("Triggered");
        headSegment.isKinematic = false;
        bodySegment.isKinematic = false;
        tailSegment.isKinematic = false;
        
        
        
        bodySegment.constraints = RigidbodyConstraints.FreezeRotation;
        tailSegment.constraints = RigidbodyConstraints.FreezeRotation;
        
        
        //Destroy(headJoint);
        
       // bodyJoint.connectedBody = null;
       // tailJoint.connectedBody = null;
       

       Destroy(bodyJoint);
       Destroy(tailJoint);
        
       
       
        //hookReference.EnableCollisions();
        
       
        
       
        
        
        //controller.SetState(PlayerState.Locomotion);
        //hookReference = null;
        
       
    }

    void LaunchEvent()
    {
        //tailSegment.AddForce(tailSegment.transform.forward * springSpeed, ForceMode.VelocityChange);
       
        
        
        
        
 
        bodySegment.constraints = RigidbodyConstraints.FreezePositionZ;
        tailSegment.constraints = RigidbodyConstraints.FreezePositionZ;
        bodySegment.linearDamping = 0.3f;
        tailSegment.linearDamping = 0.3f;
        
        isSwingingSetUp = false;
        StartCoroutine(ResetSwing());


        /*
        Vector3 upwardsForce = upwardForce * Vector3.up;

        headSegment.AddForce((upwardsForce) * (springSpeed * headLaunchRatio), ForceMode.VelocityChange);
        bodySegment.AddForce((upwardsForce) * (springSpeed * bodyLaunchRatio), ForceMode.VelocityChange);
        tailSegment.AddForce(( upwardsForce) * (springSpeed * tailLaunchRatio), ForceMode.VelocityChange);
        */






    }

    IEnumerator ResetSwing()
    {
        /*bodySegment.linearDamping = cachedDampingBody;
        bodySegment.angularDamping = cachedDampingBodyAngular;
        tailSegment.linearDamping = cachedDampingTail;
        tailSegment.angularDamping = cachedDampingTailAngular;*/

        //apply gravity
        
        
        //launch velocity
        float x = Mathf.Sin(lastSwingAngle * Mathf.Deg2Rad);
        float y = Mathf.Cos(lastSwingAngle * Mathf.Deg2Rad);
        
        

        Vector3 launchVector = new Vector3(x, y);
        
        launchVector.Normalize();

        
        Debug.Log("Angular Velocity Cached: " + accumulatedAngularVelocity);
        launchVector += accumulatedAngularVelocity;
        Debug.Log("Launch Vector: " + launchVector);

        
        //float timer = 0;
        bodySegment.AddForce(launchVector  * bodyLaunchRatio, ForceMode.VelocityChange);
        tailSegment.AddForce( launchVector  * tailLaunchRatio, ForceMode.VelocityChange);

        while (!HasHitSomething(tailCollider) || !(HasHitSomething(bodyCollider)))
        {
            
           
            bodySegment.AddForce(gravity * Vector3.up, ForceMode.Acceleration);
            tailSegment.AddForce(gravity  * Vector3.up, ForceMode.Acceleration);
            yield return null;
        }
        
        
        bodySegment.transform.forward = Vector3.right;
        tailSegment.transform.forward = Vector3.right;
        headSegment.DOMove(bodySegment.position - (bodySegment.transform.right * controller.bodyOffset), downTime).OnComplete(() =>
        {
            
            hookReference.EnableCollisions();
            hookReference = null;
            hookPoint = null;
            controller.SetState(PlayerState.Locomotion);
        });
        
        //wait for grounded callback
        








    }


    bool HasHitSomething(Collider segmentCollider)
    {
        if(Physics.CheckSphere(segmentCollider.attachedRigidbody.position, segmentCollider.bounds.extents.x + ( collisionDistance), controller.playerCollisionMask))
        {
            return true;
        }
        
        return false;
    }
    
}
