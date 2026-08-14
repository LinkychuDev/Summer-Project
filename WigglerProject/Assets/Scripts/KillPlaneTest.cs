using System;
using UnityEngine;

public class KillPlaneTest: CollisionBlock
{
     public void OnTriggerEnter(Collider other)
     {
          if (other.TryGetComponent(out PlayerController playerController))
          {
               GameManager.instance.ResetPlayerPosition();
          }
     }
}