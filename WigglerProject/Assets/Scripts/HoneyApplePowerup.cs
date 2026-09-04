using System;
using DG.Tweening;
using UnityEngine;

public class HoneyApplePowerup : MonoBehaviour
{

    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float endYValue = 1f;

    [SerializeField] private float upDuration = 1f;
    private void Start()
    {
        transform.DOLocalMove(transform.localPosition+transform.up * endYValue, upDuration).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag( "Player"))
            return;
        if (other.TryGetComponent(out PlayerController player))
        {
            player.Honeyfied(true);
            transform.DOShakeScale(shakeDuration);

        }
    }
}
