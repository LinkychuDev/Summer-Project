using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BounceTest : MonoBehaviour
{
    public float bounceHeight;

    private void OnTriggerEnter(Collider other)
    {
        if(!(other.TryGetComponent(out PlayerMovement player)))
            return;
        Debug.Log("Bounce");
        
        if(!player.isGrounded)
            return;
        player.Bounce(bounceHeight);
    }
    
}