using System;
using UnityEngine;

public class PlayerGrab : MonoBehaviour
{
    public float grabDetectionRadius;
    public float maxGrabDistance;
    
    Collider[] interactionColliders;
    
    public Rigidbody headSegment;
    
    public EnvironmentObject targetObject;

    private float collisionRadius;
    
    
    public float grabSpeed = 5;
    float targetGrabRadius;
    
    [SerializeField] private bool onlyGrabWhenRetracting;

    [SerializeField] private float grabOffset = 0.5f;
    
    PlayerState currentState;
    
    public bool isOnHoney;

    public LayerMask interactableMask;
    
    [SerializeField] private Transform grabPoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    private void OnEnable()
    {
        PlayerController.isOnHoneyEvent += OnHoneyStateChanged;
        PlayerReferenceManager.OnStateChange += OnStateChange;
        PlayerController.OnGrabEvent += OnGrabEvent;
        PlayerController.OnReleaseEvent += OnReleaseEvent;
        PlayerController.OnPreReleaseEvent += OnPreReleaseEvent;
    }

    private void OnPreReleaseEvent()
    {
        
        targetObject?.PreReleaseGrab();
    }

    private void OnReleaseEvent()
    {
       
        targetObject.ResetGrab();
        targetObject = null;
    }

    private void OnGrabEvent(EnvironmentObject obj)
    {
        targetObject = obj;
        targetObject.SetupGrab(headSegment);
    }

    private void OnHoneyStateChanged(bool obj)
    {
        isOnHoney = obj;
    }

    void Start()
    {
        interactionColliders = new Collider[1];
        headSegment = PlayerReferenceManager.instance.headSegment;
      
        collisionRadius = headSegment.gameObject.GetComponent<Collider>().bounds.extents.y;
        

    }


    private void OnDisable()
    {
        PlayerReferenceManager.OnStateChange -= OnStateChange;
        PlayerController.isOnHoneyEvent -= OnHoneyStateChanged;
        PlayerController.OnGrabEvent -= OnGrabEvent;
        PlayerController.OnReleaseEvent -= OnReleaseEvent;
        PlayerController.OnPreReleaseEvent -= OnPreReleaseEvent;
    }

    private void OnStateChange(PlayerState state)
    {
        if (state != PlayerState.Stretching)
        {
            if (targetObject != null)
            {
                targetObject.ResetGrab();
                targetObject = null;
            }
            
            
        }
        
        currentState = state;
    }


   
}

public interface IGrabbable
{
    
}