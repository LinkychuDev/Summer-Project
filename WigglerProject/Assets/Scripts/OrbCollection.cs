using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class OrbCollection : MonoBehaviour, IBreakable, IPlayerHint
{
    public MMF_Player orbSequence;
    public PlayableDirector playableDirector;
   
    public void Break()
    {
        GameManager.instance.CollectOrb(LevelDefiner.instance.sceneName);
        playableDirector.Play();
    }

    public PlayerActionEvent PlayerActionEvent { get; } = PlayerActionEvent.StretchAction;
}