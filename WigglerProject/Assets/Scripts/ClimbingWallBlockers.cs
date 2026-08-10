using System;
using UnityEngine;

public class ClimbingWallBlockers : CollisionBlock
{
    private Collider collider;
    MeshRenderer meshRenderer;
    PlayerState playerState;
    
    bool isClimbing;
    void Awake()
    {
        collider = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = GameManager.instance.BoundaryBoxes;
           
    }
    private void OnEnable()
    {
        PlayerController.ClimbEvent += ClimbEvent;
        PlayerReferenceManager.OnStateChange += OnStateChange;
    }

    private void OnStateChange(PlayerState obj)
    {
        playerState = obj;
        collider.enabled = CanActivate(isClimbing);
        
    }

    private void ClimbEvent(bool obj)
    {
        collider.enabled = CanActivate(obj);
        isClimbing = obj;
    }


    bool CanActivate(bool obj)
    {
        bool canActivate = false;

        if (obj)
        {
            if (playerState == PlayerState.Locomotion)
            {
                canActivate = true;
            }
        }
        
        return canActivate;
    }

    private void OnDisable()
    {
        PlayerController.ClimbEvent -= ClimbEvent;
        PlayerReferenceManager.OnStateChange -= OnStateChange;
    }
}   