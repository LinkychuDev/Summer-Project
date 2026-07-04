using System;
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

    public InputActionReference swingInputReference;

    [SerializeField] private float swingForce;
    public Vector2 swingInput;
    
  
    private HingeJoint headJoint, bodyJoint, tailJoint;
    

    [SerializeField] private float gravity = -15f;

    [SerializeField] private float springDamper = 0.2f;

    [SerializeField] private float targetSwingVelocity = 40f;
    
    [SerializeField] private float bodySpring = 100f;
    [SerializeField] private float tailSpring = 4000f;
    
    JointMotor bodyJointMotor;
    
    
    [SerializeField] bool useGravity = true;

    [SerializeField] private InputActionReference swingHeldInputReference;
    private bool isHeldDown;
    void Start()
    {
        controller = GetComponent<PlayerController>();
        headSegment = controller.headSegment;
        bodySegment = controller.bodySegment;
        tailSegment = controller.tailSegment;
    }
    
    
     public void StartSwing(HoneySwingTest hook)
    {
        
        isSwingingSetUp = false;
        
       
        //make this a sequence
        //stretchState = StretchState.Swinging;
        //Vector3 anchorPoint = hook.transform.position - hook.swingAnchor;

        
        hookPoint = hook.swingAnchor;
        hookPoint.transform.localPosition = Vector3.zero;
        hookPoint.rotation = Quaternion.Euler(0, 0, 0);
        headSegment.isKinematic = true;
        headSegment.position = hookPoint.position;

     
        
        
        
        
        
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


            bodySegment.linearDamping = 0;
            bodySegment.angularDamping = 0;
            tailSegment.linearDamping = 0;
            tailSegment.angularDamping = 0;


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
            swingInput = swingInputReference.action.ReadValue<Vector2>();
            isHeldDown = swingHeldInputReference.action.ReadValue<float>() > 0;
        }
    }


    private void FixedUpdate()
    {
        if (isSwingingSetUp)
        {
            SwingEvent();
        }
    }

    void SwingEvent()
    {


        float input = swingInput.y;

        /*

        if (Mathf.Abs(input) > 0)
        {
            currentSpeed = swingSpeed;
        }

        else
        {
            currentSpeed = 0;
        }
        
        float headAngle = (swingLimit * headRatio) * Mathf.Sin(Time.time * currentSpeed);
        float bodyAngle = (swingLimit * bodyRatio) * Mathf.Sin(Time.time * currentSpeed);
        float tailAngle = (swingLimit * tailRatio) * Mathf.Sin(Time.time * currentSpeed);

        

       

        

        float currentHeadAngle = headAngle * input;
        float currentBodyAngle = bodyAngle * input;
        float currentTailAngle = tailAngle * input;


        float newBodyAngle = bodyAngle - currentHeadAngle;
        float newTailAngle = tailAngle - currentBodyAngle;


        float diffHead = headAngle - lastAngleHead;
        float diffBody = bodyAngle - lastAngleBody;
        float diffTail = tailAngle - lastAngleTail;
        
        
        headSegment.transform.rotation = Quaternion.Euler(0, 0, headAngle);
        bodySegment.transform.rotation = Quaternion.Euler(0, 0, newBodyAngle);
        tailSegment.transform.rotation = Quaternion.Euler(0, 0, newTailAngle);
        */
        
       
        
        //bodyJointMotor.force = swingSpeed;

        /*if (Mathf.Abs(input) > 0f)
        {
            bodyJointMotor.targetVelocity = targetSwingVelocity * input;
        }*/

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

    }

    
}
