using System;
using DG.Tweening;
using UnityEngine;

public class OrbFragment : MonoBehaviour
{
    public HubLevelDefiner HubLevelDefiner;
    
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private float timeDelay = 0.2f;

    public int orbId;
    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController controller))
        {
            HubLevelDefiner.CollectOrbPart(orbId);
            Destroy(gameObject);
        }
    }
 
    private void Start()
    {
        //transform.DOMove(transform.position + (transform.up *updateInterval), timeDelay).SetLoops(-1).SetEase(Ease.Linear);
    }
}