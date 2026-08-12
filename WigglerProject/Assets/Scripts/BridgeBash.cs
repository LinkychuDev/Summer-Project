using DG.Tweening;
using UnityEngine;

public class BridgeBash : MonoBehaviour, IStretchBashable
{
    public float fallSpeed = 5f;
    public bool isRepeating = false;
    public bool hasBashed;

    public Ease ease = Ease.InCubic;
    public Vector3 targetRotation = new Vector3(0f, 0f, 262.5f);
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject[] bridgeColliders;
    public void StretchBash()
    {
        if (hasBashed)
        {
            if (isRepeating)
            {
                hasBashed = false;
            }
        }


        else
        {
            transform.DORotate(targetRotation, fallSpeed, RotateMode.Fast).SetEase(ease).OnComplete(() =>
            {
                foreach (var collider in bridgeColliders)
                {
                    collider.SetActive(true);
                }
            });
        }
    }
}
