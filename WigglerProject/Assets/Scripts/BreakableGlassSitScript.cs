using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BreakableGlassSitScript : MonoBehaviour
{
   public Dictionary<int, bool> BreakableGlassParts = new Dictionary<int, bool>();
   
   public UnityEvent breakEvent;

   public GameObject[] breakableGlassParts;
   private void Start()
   {
      for (int i = 0; i < breakableGlassParts.Length; i++)
      {
         BreakableGlassParts.Add(i, false);
      }
   }

   public void BreakGlass(int id)
   {
      BreakableGlassParts[id] = true;

      for (int i = 0; i < BreakableGlassParts.Keys.Count; i++)
      {
         if (!BreakableGlassParts[i])
            return;
      }

      SuccessfulBreak();
   }

   void SuccessfulBreak()
   {
      breakEvent?.Invoke();
   }
}
