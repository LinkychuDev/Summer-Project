using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BounceTest : MonoBehaviour
{
    public float bounceHeight;

    private void OnTriggerEnter(Collider other)
    {
        if(!(other.TryGetComponent(out PlayerController player)))
            return;
        Debug.Log("Bounce");
        Bounce();
    }
    
    public void Bounce()
    {
        var jumpVelocity = PlayerReferenceManager.instance.headSegment.mass * Mathf.Sqrt(bounceHeight * -2 * PlayerReferenceManager.instance.gravity) - (Time.fixedDeltaTime * PlayerReferenceManager.instance.gravity / 2);
        PlayerReferenceManager.instance.headSegment.AddForce(Vector3.up * jumpVelocity, ForceMode.Impulse);
    }
}