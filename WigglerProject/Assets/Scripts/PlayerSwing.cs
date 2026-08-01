using System;
using DG.Tweening;
using UnityEngine;

public class PlayerSwing : MonoBehaviour
{
    private CharacterController headSegment;
    private CharacterController bodySegment;
    private CharacterController tailSegment;
    
    
    private HoneySwingTest hookReference;
    private bool isSwinging;
    private Transform hookPoint;
    [SerializeField] private float swingSetUpDuration;
    [SerializeField] private float swingLength = 3;
    [SerializeField] private float maxAngle = 120f;
    private float currentAngle;
    private float timer;
    [SerializeField] private float swingSpeed;
    
    [SerializeField] HoneySwingTest hook;

    private Vector3 swingVelocity;
    private void Start()
    {
      //  headSegment = PlayerReferenceManager.instance.headSegment;
       // bodySegment = PlayerReferenceManager.instance.bodySegment;
        //tailSegment = PlayerReferenceManager.instance.tailSegment;
        
        StartSwing();
    }


    public void StartSwing()
    {
        if (hook == null)
            return;


        isSwinging = false;
        
        hookReference = hook;
        hookPoint = hookReference.swingAnchor;
        hookPoint.transform.localPosition = Vector3.zero;
        hookPoint.transform.localRotation = Quaternion.Euler(Vector3.zero);
        currentAngle = 0;
       
        headSegment.transform.rotation = hookPoint.rotation;
        headSegment.transform.position = hookPoint.position;
        

        var hookSequence = DOTween.Sequence();

        


        hookSequence.Append(headSegment.transform.DOMove(hookPoint.transform.position, swingSetUpDuration));

        //hookSequence.Join(headSegment.transform.DORotate(headSegment.rotation.eulerAngles + headTargetRotation, swingSetUpDuration));
        //headSegment.transform.SetParent(hookPoint, true);


        
        var targetBodyPosition =
            hookPoint.position + swingLength * (Vector3.down * PlayerReferenceManager.instance.bodyOffset);

        var targetTailPosition = targetBodyPosition + Vector3.down * PlayerReferenceManager.instance.tailOffset;
        hookSequence.Join(bodySegment.transform.DOMove(targetBodyPosition, swingSetUpDuration)).SetEase(Ease.OutBounce);


        hookSequence.Join(tailSegment.transform.DOMove(targetTailPosition, swingSetUpDuration)).SetEase(Ease.OutBounce);


        hookSequence.OnComplete(() =>
        {
            bodySegment.transform.position = new Vector3(headSegment.transform.position.x,
                bodySegment.transform.position.y, headSegment.transform.position.z);
            bodySegment.transform.rotation = hookPoint.rotation;


            tailSegment.transform.position = new Vector3(headSegment.transform.position.x,
                tailSegment.transform.position.y, headSegment.transform.position.z);
            tailSegment.transform.rotation = hookPoint.rotation;


            swingVelocity = Vector3.zero;
            //bodySegment.AddForce(swingSpeed * , ForceMode.VelocityChange);


            headSegment.transform.parent = hookPoint;
            bodySegment.transform.parent = hookPoint;
            tailSegment.transform.parent = hookPoint;
            isSwinging = true;
        });


        //swingLength = Mathf.Abs(swingLength);
       
       // isSwinging = true;
    }


    private void Update()
    {
        //player should move onto position
        
        //player should then swing with input
        
        //if no input keep pendulum until it moves into the center
        
        //upon letting go, player launches

       // ApplyGrappleForces();

       if (isSwinging)
       {
           bool isHeldDown = InputManager.instance.controls.Gameplay.Stretch.ReadValue<float>() > 0;

           if (isHeldDown)
           {
               SwingEvent();
           }
       }

    }

    void SwingEvent()
    {
        float yInput = InputManager.instance.controls.Gameplay.Move.ReadValue<Vector2>().y;
       
        timer += Time.deltaTime * swingSpeed * yInput;
        float angle = maxAngle * Mathf.Sin(timer);
       
        currentAngle = angle;

        hookPoint.localRotation = Quaternion.Euler(0, currentAngle, 0);

        if (timer >= float.MaxValue)
        {
            timer = 0;
        }
    }


    void ApplyGrappleForces()
    {
        /*Vector3 displacement = hookPoint.position - tailSegment.characterController.transform.position;
        float theta = Vector3.Angle(displacement, Vector3.up);
        theta = ClampAngle(theta, -maxAngle, maxAngle);
        theta = theta * Mathf.Deg2Rad;
        
        float centripetalAcceleration = swingVelocity.sqrMagnitude/displacement.sqrMagnitude;
        
        Vector3 tension = tailSegment.mass * (centripetalAcceleration + PlayerReferenceManager.instance.gravity * Mathf.Cos(theta)) * displacement.normalized;
        
        tailSegment.characterController.Move(tension * Time.deltaTime);*/
        
    }
    
    
    
    
    public float ClampAngle(float angle, float min, float max) {
        float start = (min + max) * 0.5f - 180;
        float floor = Mathf.FloorToInt((angle - start) / 360) * 360;
        return Mathf.Clamp(angle, min + floor, max + floor);
    }
}

