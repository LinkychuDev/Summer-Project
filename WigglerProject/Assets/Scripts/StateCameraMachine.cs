using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class StateCameraMachine : MonoBehaviour
{
   public CinemachineTargetGroup targetGroup;
   private static readonly int IsClimbing = Animator.StringToHash("IsClimbing");

   //public List<Transform> cinemaMachineTargets;

   [SerializeField] private CinemachineCamera CinemachineClimbCamera;
   
   private CinemachineFollow cinemachineFollow;
   private Animator animator => GetComponent<Animator>();

   [SerializeField] private float offset = -10f;

   private Vector3 followOffset;
   
   void Start()
   {
      cinemachineFollow = CinemachineClimbCamera.GetComponent<CinemachineFollow>();
      followOffset = cinemachineFollow.FollowOffset;
   }
   private void OnEnable()
   {
      PlayerController.ClimbEvent += ClimbEvent;
      GameManager.CameraClimbSwitch += OnCameraSwitch;
   }

  

   void OnDisable()
   {
      PlayerController.ClimbEvent -= ClimbEvent;
      GameManager.CameraClimbSwitch -= OnCameraSwitch;
   }

   
   private void OnCameraSwitch(Vector3 arg3)
   {
      cinemachineFollow.FollowOffset = arg3 * offset;
     // CinemachineGroupCamera.transform.rotation = Quaternion.FromToRotation(transform.up, arg3);   
      
   }
   private void ClimbEvent(bool isClimbing)
   {
      animator.SetBool(IsClimbing, isClimbing);
      
      
   }


   private void OnApplicationQuit()
   {
      if (Application.isEditor)
      {
        // cinemachineFollow.FollowOffset = followOffset;
      }
   }
}