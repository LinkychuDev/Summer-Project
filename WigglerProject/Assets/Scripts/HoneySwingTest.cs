using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoneySwingTest : MonoBehaviour
{
    public Transform swingAnchor;

    public void DisableCollisions()
    {
        gameObject.GetComponent<Collider>().enabled = false;
    }

    public void EnableCollisions()
    {
        gameObject.GetComponent<Collider>().enabled = true;
    }
   
}