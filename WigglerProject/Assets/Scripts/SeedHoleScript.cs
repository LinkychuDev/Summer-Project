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
      
        PlayerController.OnReleaseEvent?.Invoke();
        StartCoroutine(ResetThings(sender.transform));
    }

    private IEnumerator ResetThings(Transform sender)
    {
        yield return new WaitForFixedUpdate();
        sender.position = transform.position;
        SpawnPlant();
    }
}
