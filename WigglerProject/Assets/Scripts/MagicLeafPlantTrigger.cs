using System;
using System.Collections.Generic;
using UnityEngine;

public class MagicLeafPlantTrigger : MonoBehaviour
{
    public event Action OnPlatformStepped;
    public event Action OnPlatformReleased;
    public LayerMask platformLayerMask;
    public HashSet<Rigidbody> overlappingRigidbodies = new HashSet<Rigidbody>();

   
    
    private void Start()
    {
      
    }

  

    private void OnTriggerEnter(Collider other)
    {
        if(!other.transform.CompareTag("Player"))
            return;
        if (other.TryGetComponent(out PlayerStretch playerController))
        {
            if(!CanMoveWithPlatform(playerController))
                return;
            OnPlatformStepped?.Invoke();
            playerController.rb.interpolation = RigidbodyInterpolation.None;
        }
        if(!overlappingRigidbodies.Add(other.attachedRigidbody))
            return;
        
     
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.CompareTag("Player"))
            return;
        if (other.TryGetComponent(out PlayerStretch playerController))
        {
            if(!CanMoveWithPlatform(playerController))
                return;
            playerController.rb.interpolation = RigidbodyInterpolation.Interpolate;
            OnPlatformReleased?.Invoke();
        }
        overlappingRigidbodies.Remove(other.attachedRigidbody);
        
    }
   


 

    private bool CanMoveWithPlatform(PlayerStretch playerController)
    {
        return playerController.stretchState == PlayerStretch.StretchState.None || !playerController.shouldStretchForward;
    }
}