using UnityEngine;

public class PlayerGrab : MonoBehaviour
{
    public float grabDetectionRadius;
    
    
    Collider[] interactionColliders;
    
    Rigidbody headRigidbody;
    
    EnvironmentObject targetObject;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactionColliders = new Collider[1];
        headRigidbody = PlayerReferenceManager.instance.headSegment;
        PlayerReferenceManager.OnStateChange += OnStateChange;
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
        if(targetObject != null)
            return;
        if (PlayerReferenceManager.instance.currentState == PlayerState.Stretching)
        {
            var size = Physics.OverlapSphereNonAlloc(headRigidbody.position + headRigidbody.transform.forward, grabDetectionRadius, interactionColliders, PlayerReferenceManager.instance.playerCollisionMask);
            if (size > 0)
            {
                
                if (interactionColliders[0].TryGetComponent(out EnvironmentObject environmentObject))
                {
                    if (PlayerReferenceManager.instance.isHoney)
                    {
                        environmentObject.transform.SetParent(headRigidbody.transform);
                    }

                    else if(environmentObject.OnHoney)
                    {
                        environmentObject.transform.SetParent(headRigidbody.transform);
                    }

                    targetObject = environmentObject;
                    targetObject.gameObject.layer = headRigidbody.gameObject.layer;
                }
            }
        }
        
    }
}

public interface IGrabbable
{
    
}