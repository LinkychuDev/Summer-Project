using System;
using UnityEngine;

public class CheckPointTrigger : CollisionBlock
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(other.TryGetComponent(out PlayerMovement movement))
            {
                GameManager.instance.SavePlayerPosition(PlayerReferenceManager.instance.GetSegmentPositions());
            }
        }
    }
}