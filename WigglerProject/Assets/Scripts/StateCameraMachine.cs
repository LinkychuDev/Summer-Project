using System;
using UnityEngine;

public class StateCameraMachine : MonoBehaviour
{
   private static readonly int IsClimbing = Animator.StringToHash("IsClimbing");
   private Animator animator => GetComponent<Animator>();
   private void OnEnable()
   {
      PlayerController.ClimbEvent += ClimbEvent;
   }

   void OnDisable()
   {
      PlayerController.ClimbEvent -= ClimbEvent;
   }

   private void ClimbEvent(bool obj)
   {
      animator.SetBool(IsClimbing, obj);
   }
}