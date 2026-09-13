using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class BounceTest : MonoBehaviour
{
    public float bounceHeight;
    public bool isActive = true;

    public Renderer bounceRenderer;
    [SerializeField] Material[] DeadMaterial;
    [SerializeField] Material[] ActiveMaterial;
    public Renderer grassRenderer;
    [SerializeField] Material deadGrassMaterial;
    [SerializeField] private Material aliveGrassMaterial;

    
    
    public MMF_Player bouncePlayer;
    void Start()
    {
        Setup();
    }

    private void OnValidate()
    {
        Setup();
    }

    protected virtual void Setup()
    {
        if (!isActive)
        {
            Deactivate();
        }

        else
        {
            Activate();
        }
    }
    public void Activate()
    {
        if (bounceRenderer != null) bounceRenderer.materials = ActiveMaterial;

        if (grassRenderer != null) grassRenderer.material = aliveGrassMaterial;
    }

    public void Deactivate()
    {
        if (bounceRenderer != null) bounceRenderer.materials = DeadMaterial;

        if (grassRenderer != null) grassRenderer.material = deadGrassMaterial;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(!isActive)
            return;
        if(!(other.TryGetComponent(out PlayerMovement player)))
            return;
      
        
       // if(!PlayerMovement.isGrounded)
         //   return;
         
         bouncePlayer.PlayFeedbacks();
        player.Bounce(bounceHeight);
    }


    

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        
    }
}