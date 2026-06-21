using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BounceTest : MonoBehaviour, IStretchInteractable
{
    public float bounceHeight;
    public void OnStretchEvent(Rigidbody segment)
    {
        Debug.Log("OnStretchEvent");
        segment.AddForce(Vector3.up * bounceHeight, ForceMode.Impulse);
    }
    
}