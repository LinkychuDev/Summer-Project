using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

public class BackgroundMusicPlayer : MonoBehaviour
{
    public MMF_Player player;
    private MMF_MMSoundManagerAllSoundsControl soundManager;
    private void Start()
    {
        soundManager = player.GetFeedbackOfType<MMF_MMSoundManagerAllSoundsControl>();
        soundManager.ControlMode = MMSoundManagerAllSoundsControlEventTypes.Play;
        player.PlayFeedbacks();
        // PlayMusic();
    }

    public void PlayMusic()
    {
        soundManager.ControlMode = MMSoundManagerAllSoundsControlEventTypes.Play;
        player.PlayFeedbacks();
    }

    public void StopMusic()
    {
       
        soundManager.ControlMode = MMSoundManagerAllSoundsControlEventTypes.Pause;
        player.PlayFeedbacks();

    }
}
