using System;
using UnityEngine;

public class ApplePowerup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag( "Player"))
            return;
        if (other.TryGetComponent(out PlayerController player))
        {
            if(PlayerReferenceManager.instance.currentState == PlayerState.Stretching)
                return;
            if(PlayerReferenceManager.instance.canStretch)
                return;
         
        }
    }
}
