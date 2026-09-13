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

   public bool HasPlayer;
   private void Start()
   {
      boxCollider = GetComponent<BoxCollider>();
      center = transform.TransformPoint(boxCollider.center);
      halfExtents = Vector3.Scale(boxCollider.size, transform.lossyScale) * 0.5f;
   }


   private void FixedUpdate()
   {
      int count = Physics.OverlapBoxNonAlloc(transform.position, halfExtents, colliders, transform.rotation, platformLayerMask);
      HasPlayer = false;

      if (overlappingRigidbodies.Count > 0)
      {
         foreach (var other in overlappingRigidbodies)
         {
            other.interpolation = RigidbodyInterpolation.Interpolate;
         }

         overlappingRigidbodies.Clear();
      }

      for (int i = 0; i < count; i++)
      {
         if (colliders[i].TryGetComponent(out Rigidbody rb))
         {
            rb.interpolation = RigidbodyInterpolation.None;


            if (rb.gameObject.CompareTag("Player"))
            {
               HasPlayer = true;
            }
            overlappingRigidbodies.Add(rb);
           
           
         }
      }
   }

 


 

   private bool CanMoveWithPlatform(PlayerStretch playerController)
   {
      bool canMove =  playerController.stretchState == PlayerStretch.StretchState.None || !playerController.shouldStretchForward || playerController.stretchState != PlayerStretch.StretchState.Stuck;

      Debug.Log("CanMoveWithPlatform:  " + canMove);
      
      return canMove;
   }
}
