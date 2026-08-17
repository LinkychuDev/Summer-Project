using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class GiantBerryScript : BerryScript
{

    public int berryId;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
          
            GameManager.instance.AddPoints(PointToGive);     
            GameManager.instance.UpdateGiantBerryCount(berryId);
            Destroy(gameObject, 0);
            
        }
    }
}