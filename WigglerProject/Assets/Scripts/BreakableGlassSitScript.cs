using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class BreakableGlassSitScript : MonoBehaviour
{
   public Dictionary<int, bool> BreakableGlassParts = new Dictionary<int, bool>();



   public PlayableDirector glassBreakTrack;
   public UnityEvent breakEvent;

   public MMF_Player glassBreak;
   public GameObject[] breakableGlassParts;


   private void OnEnable()
   {
      glassBreakTrack.played += GlassBreakTrackOnPlayed;
      glassBreakTrack.stopped += GlassBreakTrackOnStopped;
   }

   private void GlassBreakTrackOnPlayed(PlayableDirector obj)
   {
      if (obj == glassBreakTrack)
      {
         glassBreakTrack.Play();
      }
   }

   private void OnDisable()
   {
      glassBreakTrack.played -= GlassBreakTrackOnPlayed;
      glassBreakTrack.stopped -= GlassBreakTrackOnStopped;
   }

   private void GlassBreakTrackOnStopped(PlayableDirector obj)
   {
      for (int i = 0; i < BreakableGlassParts.Keys.Count; i++)
      {
         if (!BreakableGlassParts[i])
            return;
      }
      
      if (obj == glassBreakTrack)
      {
         SuccessfulBreak();
      }
   }

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

      
      glassBreakTrack.Play();
      
     
      //SuccessfulBreak();
   }

   void SuccessfulBreak()
   {
      breakEvent?.Invoke();
   }
}
