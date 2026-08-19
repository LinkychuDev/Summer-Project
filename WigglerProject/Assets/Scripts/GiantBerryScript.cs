using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class GiantBerryScript : BerryScript
{

    public int berryId;
    public bool isCollected;
  
    void Start()
    {
       
    }
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            CollectEvent();
            GameManager.instance.AddPoints(PointToGive);     
            LevelDefiner.instance.UpdateGiantBerryCount(berryId);
        }
    }

    public void CollectEvent()
    {
        isCollected = true;
        gameObject.SetActive(false);
    }
}