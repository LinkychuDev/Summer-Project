using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrbCollection : MonoBehaviour, IBreakable, IPlayerHint
{
    public MMF_Player orbSequence;
   
    public void Break()
    {
        GameManager.instance.CollectOrb(LevelDefiner.instance.sceneName);
        SceneManager.LoadScene("HubWorld");
    }

    public PlayerActionEvent PlayerActionEvent { get; } = PlayerActionEvent.StretchAction;
}