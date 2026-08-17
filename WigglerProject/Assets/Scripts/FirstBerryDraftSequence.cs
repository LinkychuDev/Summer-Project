using System;
using MoreMountains.Feedbacks;
using UnityEngine;

public class FirstBerryDraftSequence : MonoBehaviour
{
   public GameObject berries;
   private MMF_Player berryEvent;
   private void Start()
   {
      berryEvent = GetComponent<MMF_Player>();
      berries.SetActive(false);
   }

   private void OnEnable()
   {
      GameManager.GameEvents.OnEventCompleted += FirstFlowerGrownEvent;
   }

   void OnDisable()
   {
      GameManager.GameEvents.OnEventCompleted -= FirstFlowerGrownEvent;
   }

   private void FirstFlowerGrownEvent(GameFlags gameFlags)
   {
      if (gameFlags == GameFlags.FirstFlowerGrown)
      {
         berryEvent.PlayFeedbacks();
         GameManager.GameEvents.OnEventCompleted -= FirstFlowerGrownEvent;
      }
      
     
     
   }
}
