using System;
using System.Collections;
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
        StartCoroutine(Activate());
       
    }

    IEnumerator Activate()
    {
        yield return new WaitUntil(() =>
            PlayerReferenceManager.instance.playerStretch.stretchState == PlayerStretch.StretchState.None || PlayerReferenceManager.instance.currentState == PlayerState.Locomotion);
        GameManager.instance.StartSpecialCutscene();
        yield return null;
        playableDirector.Play();
    }

    public PlayerActionEvent PlayerActionEvent { get; } = PlayerActionEvent.StretchAction;
}