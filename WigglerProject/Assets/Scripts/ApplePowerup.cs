using System;
using MoreMountains.Feedbacks;
using UnityEngine;

public class ApplePowerup : MonoBehaviour
{
    public MMF_Player biteFeedback;
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
            biteFeedback.PlayFeedbacks(other.transform.position);
            
        }
    }
}
