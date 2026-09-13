using System;
using System.Collections;
using RotaryHeart.Lib.PhysicsExtension;
using UnityEngine;

public class SeedHoleScript : MonoBehaviour, EnvironmentObject.IGrabEvent
{
    public MagicLeafPlant magicPlant;
    public float overlapRadius = 3;
    public LayerMask detectionLayer;
    private BoxCollider boxCollider;

    private Vector3 center, halfExtents;
    
    Collider[] colliders = new Collider[1];
    
    bool hasSpawned = false;

    private void Awake()
    {
        
    }
    
    public void SpawnPlant()
    {
        magicPlant.gameObject.SetActive(true);
        hasSpawned = true;
    }

    public void GrabEvent(EnvironmentObject sender)
    {
        //sender.ResetGrab();
        SeedScript seed = sender as SeedScript;
        
        
        PlayerController.OnReleaseEvent?.Invoke();
        StartCoroutine(ResetThings(seed));
    }

    private IEnumerator ResetThings(SeedScript sender)
    {
        yield return new WaitForFixedUpdate();
        sender.transform.position = transform.position;

        yield return new WaitUntil(() => sender.isWet);
        SpawnPlant();
    }
}
