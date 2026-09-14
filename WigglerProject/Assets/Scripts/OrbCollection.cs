using System;
using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class OrbCollection : MonoBehaviour
{
    public MMF_Player orbSequence;
    public PlayableDirector playableDirector;
    public MMF_Player teleportTrack;
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerMovement playerMovement))
        {
            GameManager.instance.CollectOrb(LevelDefiner.instance.sceneName);
            StartCoroutine(Activate());
        }

    }

    IEnumerator Activate()
    {
        yield return new WaitUntil(() =>
            PlayerReferenceManager.instance.playerStretch.stretchState == PlayerStretch.StretchState.None || PlayerReferenceManager.instance.currentState == PlayerState.Locomotion);
        
        FindFirstObjectByType<BackgroundMusicPlayer>().StopMusic();
        yield return null;
        teleportTrack.PlayFeedbacks();
        GameManager.instance.StartSpecialCutscene();
        yield return null;
        playableDirector.Play();
    }

    public PlayerActionEvent PlayerActionEvent { get; } = PlayerActionEvent.StretchAction;
}