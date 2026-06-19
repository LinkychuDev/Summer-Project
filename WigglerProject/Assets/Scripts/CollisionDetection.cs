using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollisionDetection : MonoBehaviour
{
     public int maxBounces = 5;
     public float skinWidth = 0.05f;
     
     //collider bounding Box
     private Collider _collider;
     private Bounds bounds;
     
    // public LayerMask collisionMask;

    public LayerMask collisionMask;

     private float maxSlopeAngle;
     

     private void Awake()
     {
          _collider = GetComponent<Collider>();
          collisionMask = GetMask(gameObject.layer);
     }

     private void FixedUpdate()
     {
          //recursion
          bounds = _collider.bounds;
          bounds.Expand(-2 * skinWidth);
     }

     public Vector3 CollideAndSlide(Vector3 vel, Vector3 pos, int depth, bool gravityPass, Vector3 initialVelocity)
     {
          
          if (depth >= maxBounces)
          {
               
               return Vector3.zero;
          }

          //distance from point to collision needs skinwidth
         float distance = vel.magnitude + skinWidth;
         
         RaycastHit hit;
         if (Physics.SphereCast(pos, bounds.extents.x, vel.normalized, out hit, distance, collisionMask))
         {
              Debug.Log("Collision detected");
              Vector3 snapToSurface = vel.normalized * (hit.distance - skinWidth) ;
              
              //leftover velocity that would have been, if not for the collision
              Vector3 leftOver = vel - snapToSurface;

              float angle = Vector3.Angle(vel, hit.normal);
              
              //makes sure that we have enough room for collision check to work
              if (snapToSurface.magnitude <= skinWidth)
              {
                   Debug.Log("Failed Collision Check");
                   snapToSurface = Vector3.zero;
              }

              //normal slopes/flat surfaces
              if (angle <= maxSlopeAngle)
              {
                   if (gravityPass)
                        return snapToSurface; 
                   leftOver = ProjectAndScale(leftOver, hit.normal);
              }

              //wall
              else
              {
                   Vector3 hitNormalDir = hit.normal;
                   Vector3 initialVelDir = initialVelocity;

                   hitNormalDir.y = 0;
                   initialVelDir.y = 0;
                   float scale = 1- Vector3.Dot(hitNormalDir.normalized, -initialVelDir.normalized);
                   leftOver = ProjectAndScale(leftOver, hit.normal) * scale;
              }

              
              
              return snapToSurface + CollideAndSlide(leftOver, pos + snapToSurface, depth + 1, gravityPass,  initialVelocity);
              
         }
         
          Debug.Log("No Collisions");
          return vel;
     }

     private  Vector3 ProjectAndScale(Vector3 vector, Vector3 normal)
     {
          float magnitude = vector.magnitude;
          vector = Vector3.ProjectOnPlane(vector, normal).normalized;
          vector *= magnitude;
          return vector;
     }
     
     
     LayerMask GetMask ( int againstLayer ) {
          LayerMask result = new LayerMask();
          for ( int i = 0; i < 32; i++ ) {
               result = result ^ ( ( Physics.GetIgnoreLayerCollision( i, againstLayer ) ? 0 : 1 ) << i );
          }
          return result;
     }
}