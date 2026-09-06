using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class MovingPlatformTrigger : MonoBehaviour
{
   public Transform movingPlatformParent;
   private Transform cachedParent;
   
   
   
   public LayerMask platformLayerMask;
   public HashSet<Rigidbody> overlappingRigidbodies = new HashSet<Rigidbody>();

   BoxCollider boxCollider;

   private Vector3 center, halfExtents;
   private Quaternion rotation;
   
   Collider[] colliders = new Collider[10];
   private void Start()
   {
      
   }

  

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
      return playerController.stretchState == PlayerStretch.StretchState.None || !playerController.shouldStretchForward;
   }
}
