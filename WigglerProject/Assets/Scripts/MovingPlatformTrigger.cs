using System;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformTrigger : MonoBehaviour
{
   public Transform movingPlatformParent;
   private Transform cachedParent;
   
   public HashSet<Rigidbody> overlappingRigidbodies = new HashSet<Rigidbody>();
   private void OnTriggerEnter(Collider other)
   {
      if (other.TryGetComponent(out PlayerStretch playerController))
      {
         if(!CanMoveWithPlatform(playerController))
            return;
         playerController.rb.interpolation = RigidbodyInterpolation.None;
      }
      if(!overlappingRigidbodies.Add(other.attachedRigidbody))
         return;
   }

   private void OnTriggerExit(Collider other)
   {
      if (other.TryGetComponent(out PlayerStretch playerController))
      {
         if(!CanMoveWithPlatform(playerController))
            return;
         playerController.rb.interpolation = RigidbodyInterpolation.Interpolate;
      }
      overlappingRigidbodies.Remove(other.attachedRigidbody);
   }


   private bool CanMoveWithPlatform(PlayerStretch playerController)
   {
      return playerController.stretchState == PlayerStretch.StretchState.None || playerController.shouldStretchForward;
   }
}
