using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrbCollection : MonoBehaviour, IBreakable
{
    public MMF_Player orbSequence;
   
    public void Break()
    {
        orbSequence.PlayFeedbacks();
    }
}