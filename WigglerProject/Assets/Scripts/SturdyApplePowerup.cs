using System;
using UnityEngine;

public class SturdyApplePowerup : MonoBehaviour
{
   private void OnTriggerEnter(Collider other)
   {
      if(!other.CompareTag( "Player"))
         return;
      if (other.TryGetComponent(out PlayerController player))
      {
         player.Sturdy(true);
            
      }
   }
}
