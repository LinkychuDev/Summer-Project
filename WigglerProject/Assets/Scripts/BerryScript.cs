using System;
using MoreMountains.Feedbacks;
using UnityEngine;

public class BerryScript : MonoBehaviour
{
    public int PointToGive = 1;
    public MMF_Player berrySFX;
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            berrySFX.PlayFeedbacks();
           GameManager.instance.AddPoints(PointToGive);     
           Destroy(gameObject, 0);
        }
    }
}
