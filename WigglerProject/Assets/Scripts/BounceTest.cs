using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BounceTest : MonoBehaviour
{
    public float bounceHeight;

    private void OnTriggerEnter(Collider other)
    {
        if(!(other.TryGetComponent(out PlayerReferenceManager player)))
            return;
        Debug.Log("Bounce");
        player.Bounce(bounceHeight);
    }
}