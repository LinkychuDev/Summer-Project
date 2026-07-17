using System;
using UnityEngine;

public class PlayerGrab : MonoBehaviour
{
    public float grabDetectionRadius;
    public float maxGrabDistance;
    
    Collider[] interactionColliders;
    
    Rigidbody headRigidbody;
    
    EnvironmentObject targetObject;

    private float collisionRadius;
    
    float targetGrabRadius;
    
    [SerializeField] private bool onlyGrabWhenRetracting;

    [SerializeField] private float grabOffset = 0.5f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactionColliders = new Collider[1];
        headRigidbody = PlayerReferenceManager.instance.headSegment;
        PlayerReferenceManager.OnStateChange += OnStateChange;
        collisionRadius = headRigidbody.gameObject.GetComponent<Collider>().bounds.extents.y;
      
        
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
    }

    // Update is called once per frame
    void Update()
    {
        //targetGrabRadius = grabDetectionRadius + collisionRadius;
       
        if (PlayerReferenceManager.instance.CanStick())
        {

            if (targetObject == null)
            {
                DetectGrab();
            }

            else
            {
                GrabObject();
            }

        }

        else
        {
            if (targetObject != null)
            {
                Debug.Log("Called");
                targetObject.ResetGrab();
                targetObject = null;
            }
        }
        
    }

    private void DetectGrab()
    {
        if (Physics.SphereCast(headRigidbody.transform.position, grabDetectionRadius,
                headRigidbody.transform.forward, out RaycastHit hit, maxGrabDistance,
                PlayerReferenceManager.instance.playerCollisionMask))
        {
            if (hit.transform.TryGetComponent(out EnvironmentObject environmentObject))
            {
                if (PlayerReferenceManager.instance.isHoney)
                {
                    environmentObject.SetupGrab(headRigidbody);
                }

                else if (environmentObject.OnHoney)
                {
                    environmentObject.SetupGrab(headRigidbody);
                }


                targetObject = environmentObject;
            }
        }
    }


    void GrabObject()
    {
        targetObject.GrabMove(headRigidbody.position + headRigidbody.transform.forward * grabOffset);
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.coral;
            Gizmos.DrawWireSphere(headRigidbody.position , grabDetectionRadius);
            Gizmos.DrawLine(headRigidbody.position, headRigidbody.position + headRigidbody.transform.forward * maxGrabDistance);
        }


    }
}

public interface IGrabbable
{
    
}