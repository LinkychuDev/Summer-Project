using System;
using System.Collections;
using System.Collections.Generic;
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
    
    Rigidbody headSegment;
    Rigidbody bodySegment;
    Rigidbody tailSegment;

    //public InputActionReference swingInputReference;

    [SerializeField] private float swingForce;
    public Vector2 swingInput;
    
  
    private HingeJoint headJoint, bodyJoint, tailJoint;
    

    

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


    [SerializeField] private Transform bodyOpen;
    [SerializeField] private Transform headClose;
    [SerializeField] private Transform bodyClose;
    [SerializeField] private Transform tailOpen;
    Vector3 headClosePos;
    Vector3 bodyOpenPos;
    Vector3 bodyClosePos;
    Vector3 tailOpenPos;
    [SerializeField] private float swingDamp;
    
    [SerializeField] Vector3 headTargetRotation = new Vector3(-90f, 0f, 0f);





    [Header("Parabola")] 
    [SerializeField] private float maxHeight;
    [SerializeField] private float minHeight;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private int parabolaPoints;
    [SerializeField] private LineRenderer trajectoryLine;
    private float parabolaHeight;
    [SerializeField] private float curveLength;
    List<Vector3> pathPoints = new List<Vector3>();
    Vector3 TargetSwingPosition;
    [SerializeField] private float launchRatio;

    void Start()
    {
   
        headSegment = PlayerReferenceManager.instance.headSegment;
        bodySegment = PlayerReferenceManager.instance.bodySegment;
        tailSegment = PlayerReferenceManager.instance.tailSegment;
    }
    
    
     public void StartSwing(HoneySwingTest hook)
    {
        if(hook ==  null)
            return;
        
        
        isSwingingSetUp = false;
        trajectoryLine.positionCount = parabolaPoints;
        trajectoryLine.enabled = true;
       
        //make this a sequence
        //stretchState = StretchState.Swinging;
        //Vector3 anchorPoint = hook.transform.position - hook.swingAnchor;
      

        hookReference = hook;
        hookPoint = hookReference.swingAnchor;
        hookPoint.transform.localPosition = Vector3.zero;
        hookPoint.rotation = Quaternion.Euler(0, 0, 0);
        headSegment.isKinematic = true;
        headSegment.rotation = Quaternion.Euler(0, 90f, 0);
        
        headCollider = headSegment.GetComponent<SphereCollider>();
        bodyCollider = bodySegment.GetComponent<SphereCollider>();
        tailCollider = tailSegment.GetComponent<SphereCollider>();
     
        
        
        
        
        
        Sequence hookSequence = DOTween.Sequence();
        
        
        headClosePos = headClose.localPosition;
        bodyOpenPos = bodyOpen.localPosition;
        bodyClosePos = bodyClose.localPosition;
        tailOpenPos = tailOpen.localPosition;

        pathPoints.Clear();
        

        hookSequence.Append(headSegment.transform.DOMove(hookPoint.transform.position, swingSetUpDuration));
       
        //hookSequence.Join(headSegment.transform.DORotate(headSegment.rotation.eulerAngles + headTargetRotation, swingSetUpDuration));
        //headSegment.transform.SetParent(hookPoint, true);

        headSegment.isKinematic = true;

        
        Vector3 targetBodyPosition =
            hookPoint.position + (swingLength * (Vector3.down * (PlayerReferenceManager.instance.bodyOffset)));
        
        Vector3 targetTailPosition = targetBodyPosition + (Vector3.down * (PlayerReferenceManager.instance.tailOffset));
        hookSequence.Join(bodySegment.DOMove(targetBodyPosition, swingSetUpDuration)).SetEase(Ease.OutBounce);
        
        
        hookSequence.Join(tailSegment.DOMove(targetTailPosition, swingSetUpDuration)).SetEase(Ease.OutBounce);


        hookSequence.OnComplete(() =>
        {

            
            headClose.localPosition = new Vector3(headClosePos.x, headClosePos.z, headClosePos.y);
            bodyOpen.localPosition = new Vector3(bodyOpenPos.x, bodyOpenPos.z, bodyOpenPos.y);
            bodyClose.localPosition = new Vector3(bodyClosePos.x, bodyClosePos.z, bodyClosePos.y);
            tailOpen.localPosition = new Vector3(tailOpenPos.x, tailOpenPos.z, tailOpenPos.y);
            
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

            
            bodyJoint.autoConfigureConnectedAnchor = false;
            tailJoint.autoConfigureConnectedAnchor = false;
            
            
            bodyJoint.anchor = new Vector3(0, bodySegment.transform.InverseTransformPoint(headSegment.position).y, 0);
            tailJoint.anchor = new Vector3(0, tailSegment.transform.InverseTransformPoint(bodySegment.position).y, 0);
            
            
       
            
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
            bodySegment.AddForce(PlayerReferenceManager.instance.gravity * Vector3.up, ForceMode.Acceleration);
            tailSegment.AddForce(PlayerReferenceManager.instance.gravity * Vector3.up, ForceMode.Acceleration);
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
        accumulatedVelocity = tailSegment.linearVelocity;
        Debug.Log("2Accumulated Velocity: " +accumulatedAngularVelocity);
        Debug.Log("Rigidbody Velocity: " + accumulatedVelocity);

        
        
        TargetSwingPosition = new Vector3(tailSegment.position.x + input * (minDistance + (accumulatedVelocity.magnitude * launchRatio))  , tailSegment.position.y, tailSegment.position.z);
        

       
            
        //accumulatedVelocity = bodySegment.transform.forward * bodyJoint.velocity;
        //is at max swing
        CalculateSwingTrajectory(tailSegment.position, TargetSwingPosition);

    }


    void CalculateSwingTrajectory(Vector3 startPosition, Vector3 endPosition)
    {
        pathPoints.Clear();
        for (int i = 0; i < parabolaPoints; i++)
        {
            float t = (float)i / (parabolaPoints -1);
            
            Vector3 point = ParabolaCalculator(startPosition, endPosition, t);
            pathPoints.Add(point);
            
        }
        
        
        if (Physics.Raycast(pathPoints[^1], Vector3.down, out RaycastHit hit, Mathf.Infinity, PlayerReferenceManager.instance.groundMask))
        {
            pathPoints.Add(hit.point);
            Debug.Log("Ground Detected: " + hit.point);
        }

        else
        {
            Debug.Log("No Ground Detected");
        }
        trajectoryLine.SetPositions(pathPoints.ToArray());
        
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
        
       
        
       
        
        
        //PlayerReference.instance.SetState(PlayerState.Locomotion);
        //hookReference = null;
        
       
    }

    public Vector3 ParabolaCalculator(Vector3 start, Vector3 end, float timeStep)
    {
        Vector3 linePoint = Vector3.Lerp(start, end, timeStep);
        
        float parabola = -4f * minHeight * (timeStep * timeStep - timeStep);
        
        linePoint.y += parabola;
        
        return linePoint;
    }
    void LaunchEvent()
    {
        //tailSegment.AddForce(tailSegment.transform.forward * springSpeed, ForceMode.VelocityChange);





        headSegment.isKinematic = true;
        bodySegment.isKinematic = true;
        tailSegment.isKinematic = true;
        
        isSwingingSetUp = false;
        
       
        ResetSwing();


        /*
        Vector3 upwardsForce = upwardForce * Vector3.up;

        headSegment.AddForce((upwardsForce) * (springSpeed * headLaunchRatio), ForceMode.VelocityChange);
        bodySegment.AddForce((upwardsForce) * (springSpeed * bodyLaunchRatio), ForceMode.VelocityChange);
        tailSegment.AddForce(( upwardsForce) * (springSpeed * tailLaunchRatio), ForceMode.VelocityChange);
        */






    }

    void ResetSwing()
    {
        headClose.localPosition = new Vector3(headClosePos.x, headClosePos.y, headClosePos.z);
        bodyOpen.localPosition = new Vector3(bodyOpenPos.x, bodyOpenPos.y, bodyOpenPos.z);
        bodyClose.localPosition = new Vector3(bodyClosePos.x, bodyClosePos.y, bodyClosePos.z);
        tailOpen.localPosition = new Vector3(tailOpenPos.x, tailOpenPos.y, tailOpenPos.z);
        
        
        bodySegment.transform.forward = Vector3.right;
        tailSegment.transform.forward = Vector3.right;

        
        
        
        
        var positions = pathPoints.ToArray();
        Sequence sequence = DOTween.Sequence();

        sequence.Append(tailSegment.DOPath(positions, downTime)).OnUpdate(() =>
        {
            bodySegment.position = tailSegment.position - (tailSegment.transform.right * PlayerReferenceManager.instance.tailOffset);
            headSegment.position = bodySegment.position - (bodySegment.transform.right * PlayerReferenceManager.instance.bodyOffset);
        });
        
        
        
        sequence.OnComplete(() =>
        {
            headSegment.isKinematic = false;
            bodySegment.isKinematic = false;
            tailSegment.isKinematic = false;
            
        });
        
        //headSegment.isKinematic = true;
       



    }


   
      
        
    

    
   

    bool HasHitSomething(Collider segmentCollider)
    {
        if(Physics.CheckSphere(segmentCollider.attachedRigidbody.position, segmentCollider.bounds.extents.x + ( collisionDistance), PlayerReferenceManager.instance.playerCollisionMask))
        {
            return true;
        }
        
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (isSwingingSetUp)
        {
            
            for (int i = 0; i < pathPoints.Count; i++)
            {
                if (i != pathPoints.Count - 1)
                {
                    Gizmos.color = Color.chartreuse;
                    Gizmos.DrawWireSphere(pathPoints[i], 0.2f);
                }

                else
                {
                    Gizmos.color = Color.crimson;
                    Gizmos.DrawWireSphere(pathPoints[i], 0.2f);
                }
            }


            Gizmos.color = Color.slateBlue;
            Gizmos.DrawLine(pathPoints[^1], pathPoints[^1] + (Vector3.down  * 10000));
        }
    }
}
