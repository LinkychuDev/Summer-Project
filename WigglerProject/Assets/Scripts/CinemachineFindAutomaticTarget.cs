using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CinemachineFindAutomaticTarget : MonoBehaviour
{
   public CinemachineCamera virtualCamera;
   [SerializeField] private Transform targetFollow;
   public string targetName;


   private void OnValidate()
   {
      if(targetFollow == null)
         return;
      targetName = targetFollow?.name;
   }

   private void Start()
   {
      virtualCamera = gameObject.GetComponent<CinemachineCamera>();
      if (virtualCamera.Target.LookAtTarget == null)
      {
         Transform target = GameObject.Find(targetName).transform;
         virtualCamera.Target.TrackingTarget = target;
      }
   }
}