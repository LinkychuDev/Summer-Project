using System;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;

public class LeverScript : EnvironmentObject
{
    public UnityEvent OnLeverPulledEvent, OnLeverRetracted;
    public bool isRepeating;
    internal float maxPullDistance;
    public float pullDistance;
    public Transform leverOrigin;
    Vector3 originalPosition;
    public bool activated;

    Vector3 lastPosition;
    public float resetTime;

    private Tween pullTween;
    
    public LineRenderer pullLineRenderer;

    private float originalOffset;
    [SerializeField] private float pullResetTime = 3f;
    
    
    [SerializeField] private Material deactiveMaterial;
    [SerializeField] private Material activeMaterial;
    [SerializeField] Renderer rend;
    [SerializeField] Renderer leverOriginRenderer;



    public MMF_Player tickingTimer;
    //only in editor
    [SerializeField] private bool deactivate;
    
    protected override void Awake()
    {
        base.Awake();
        originalPosition = transform.position;
        resetTime = pullResetTime;
        rb.useGravity = false;

        if (deactivate)
        {
            DeactivateLever();
        }
        
        

    }

    public override void SetupGrab(Rigidbody grabPoint)
    {
        base.SetupGrab(grabPoint);
        if (isRepeating)
        {
            if (pullTween != null)
            {
                if (pullTween.IsPlaying())
                {
                    pullTween.Kill();
                }
            }
        }
    }


    public void ActivateLever()
    {
        leverOriginRenderer.material = activeMaterial;
        rend.material = activeMaterial;
        canGrab = true;
        
    }

    public void DeactivateLever()
    {
        leverOriginRenderer.material = deactiveMaterial;
        rend.material = deactiveMaterial;
        canGrab = false;
        if (activated)
        {
            activated = false;
            OnLeverRetracted?.Invoke();
           
        }
    }
    void Start()
    {
        //StartupEvent?.Invoke();
        originalOffset = Vector3.Distance(rb.position, leverOrigin.position);
        maxPullDistance = pullDistance + originalOffset;
        
    }

    

    protected override void Update()
    {
        base.Update();
        pullLineRenderer.SetPosition(0, pullLineRenderer.transform.InverseTransformPoint(leverOrigin.transform.position));
        pullLineRenderer.SetPosition(1, pullLineRenderer.transform.InverseTransformPoint(rb.position));
    }

    public override void GrabMove()
    {
        //rb.transform.forward = forwardVector;
         //base.GrabMove(headRigidbodyPosition, forwardVector, grabSpeed);
       
         if(!canGrab)
             return;
       
        
        
        
        
        
        
        
    }


    public override void ResetGrab()
    {
        base.ResetGrab();
        
        RetractedEvent();
        GrabIdle();
    }

    internal virtual void RetractedEvent()
    {
        var distance = Vector3.Distance(rb.position, leverOrigin.transform.position);
         
          
        if (distance > maxPullDistance)
        {
            if (!activated)
            {
                OnLeverPulledEvent?.Invoke();
                activated = true;
                resetTime = CalculatePullResetTime();

                if (isRepeating)
                {
                    tickingTimer.GetFeedbackOfType<MMF_MMSoundManagerSound>().PlaybackDuration =
                        new Vector2(resetTime, resetTime);
                    //tickingTimer.GetFeedbackOfType<MMF_MMSoundManagerSound>().SetFeedbackDuration(resetTime);
                    tickingTimer.PlayFeedbacks(transform.position);
                }

            }
            
        }
    }

    public override void GrabIdle()
    {
        if (isRepeating)
        {
            rb.useGravity = false;
            
            pullTween = rb.transform.DOMove(originalPosition, resetTime).OnComplete(() =>
            {
                rb.useGravity = true;
                activated = false;
              OnLeverRetracted?.Invoke();
                resetTime = pullResetTime;
                tickingTimer.StopFeedbacks();
            });

          
        }
      
    }


    int CalculatePullResetTime()
    {
        //if at max position pullreset time is standard
        
        float distance = Vector3.Distance(rb.position, leverOrigin.transform.position);


        float t = Mathf.Clamp01(distance / maxPullDistance);
        float x = Mathf.Lerp(0, pullResetTime, t);
        
        return (int)x;


    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
}
