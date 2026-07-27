using System;
using UnityEngine;

public class FirstBerryDraftSequence : MonoBehaviour
{
   public GameObject berries;

   private void Start()
   {
      berries.SetActive(false);
   }

   private void OnEnable()
   {
      GameManager.instance.FirstFlowerGrownEvent += FirstFlowerGrownEvent;
   }

   void OnDisable()
   {
      GameManager.instance.FirstFlowerGrownEvent -= FirstFlowerGrownEvent;
   }

   private void FirstFlowerGrownEvent()
   {
      berries.SetActive(true);
   }
}
