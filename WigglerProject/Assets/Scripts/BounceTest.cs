using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BounceTest : MonoBehaviour
{
    public float bounceHeight;
    public bool isActive = true;

    public Renderer bounceRenderer;
    [SerializeField] Material DeadMaterial;
    [SerializeField] Material ActiveMaterial;

    void Start()
    {
        Setup();
    }

    private void OnValidate()
    {
        Setup();
    }

    void Setup()
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
        bounceRenderer.material = ActiveMaterial;
    }

    public void Deactivate()
    {
        bounceRenderer.material = DeadMaterial;   
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