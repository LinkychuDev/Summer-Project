using System;
using UnityEngine;
using UnityEngine.Events;

public class SwitchScript : MonoBehaviour
{
    public UnityEvent onActivatedEvent, onDeactivatedEvent;
    [SerializeField] private bool isRepeating;
    public bool isActivated;

    public Renderer _renderer;
    
    public Material activatedMaterial;
    public Material deactivatedMaterial;
    //public EnvironmentObject obj;

    private Collider triggerCollider;
    
    BoxCollider boxCollider;
   


    private Vector3 halfExtents, center;

    public int maxColliders = 10;
    private Collider[] colliders;

    public LayerMask interactionMask;
    void Start()
    {
        //onDeactivatedEvent?.Invoke();
        boxCollider = GetComponent<BoxCollider>();
        colliders = new Collider[maxColliders];
        center = transform.TransformPoint(boxCollider.center);
        halfExtents = Vector3.Scale(boxCollider.size, transform.lossyScale) * 0.5f;

    }


    public void Activate()
    {
        isActivated = true;
        _renderer.material = activatedMaterial;
        onActivatedEvent.Invoke();
    }

    public void Deactivate()
    {
        isActivated = false;
        _renderer.material = deactivatedMaterial;
        onDeactivatedEvent.Invoke();
    }
    private void FixedUpdate()
    {
        int numberOfColliders =
            Physics.OverlapBoxNonAlloc(center, halfExtents, colliders, transform.rotation, interactionMask);
        if (numberOfColliders > 0)
        {
            if(isActivated)
                return;
            for (int i = 0; i < numberOfColliders; i++)
            {
                if (colliders[i].TryGetComponent(out EnvironmentObject rb))
                {
           
                    Activate();

                    if (!isRepeating)
                    {
                        rb.canGrab = false;
                    }
                }

        
                else if(colliders[i].TryGetComponent(out PlayerStretch stretch))
                {
                    if(stretch.stretchState != PlayerStretch.StretchState.None)
                        return;
                    Activate();
                }
            }
            
        }

        else
        {
            if(!isActivated)
                return;
            if(!isRepeating)
                return;
            
            Deactivate();
            
        }
        
        
        
    }
}
