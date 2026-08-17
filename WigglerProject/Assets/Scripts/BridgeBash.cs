using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Playables;

public class BridgeBash : MonoBehaviour, IStretchBashable, IPlayerHint
{
    public float fallSpeed = 5f;
    public bool isRepeating = false;
    public bool hasBashed;

    public Ease ease = Ease.InCubic;
    public Vector3 targetRotation = new Vector3(0f, 0f, 262.5f);
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject[] bridgeColliders;
    
    public PlayableDirector playableDirector;



    public void BashEvent()
    {
        //bashPlayer.PlayFeedbacks();
        playableDirector.Play();
        transform.DORotate(targetRotation, fallSpeed, RotateMode.Fast).SetEase(ease).OnComplete(() =>
        {
            foreach (var collider in bridgeColliders)
            {
                collider.SetActive(true);
            }
        });
    }
    public void StretchBash()
    {/*
        if (hasBashed)
        {
            if (isRepeating)
            {
                hasBashed = false;
            }
        }


        else
        {
            //bashPlayer.PlayFeedbacks();
            playableDirector.Play();
            transform.DORotate(targetRotation, fallSpeed, RotateMode.Fast).SetEase(ease).OnComplete(() =>
            {
                foreach (var collider in bridgeColliders)
                {
                    collider.SetActive(true);
                }
            });
        }*/
    }

    public PlayerActionEvent PlayerActionEvent { get; } = PlayerActionEvent.StretchAction;
}
