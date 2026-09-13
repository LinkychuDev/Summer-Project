using System;
using UnityEngine;

public class SeedScript : EnvironmentObject, IWettable
{
    public bool isWet;

    public Material wetMaterial;
    public float currentWaterAmount;
    public float waterThreshold = 1;
    private Renderer rend;

    private bool planted;

    private Collider _collider;
    protected override void Awake()
    {
        base.Awake();
        rend = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
    }
    
    
    public override void FixedUpdate()
    {
        if(!isGrabbed)
            return;
        if(planted)
            return;
        var numCols = Physics.OverlapSphereNonAlloc(rb.position, collisionRadius, _colliders, collisionMask, QueryTriggerInteraction.Collide);
        if ( numCols > 0)
        {
            for (int i = 0; i < numCols; i++)
            {
              
                if (_colliders[i].TryGetComponent(out IGrabEvent grabEvent))
                {
                    planted = true;
                    _collider.enabled = false;
                    grabEvent.GrabEvent(this);
                }
                
               
            }
            
           
        }
    }

    public void OnWet(float wetAmount)
    {
        if(isWet)
            return;
        if(rend == null)
            rend = GetComponent<Renderer>();
        currentWaterAmount += wetAmount;

        if (currentWaterAmount >= waterThreshold)
        {
            rend.material = wetMaterial;
            isWet = true;
        }
    

    }
}
