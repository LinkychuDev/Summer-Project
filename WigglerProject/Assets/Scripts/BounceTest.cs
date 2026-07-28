using System;
using System.Collections.Generic;
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
        Debug.Log("Bounce");
        
        if(!PlayerMovement.isGrounded)
            return;
        player.Bounce(bounceHeight);
    }


    

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        
    }
}