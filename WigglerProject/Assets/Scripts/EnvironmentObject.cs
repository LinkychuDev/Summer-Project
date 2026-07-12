using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentObject : MonoBehaviour
{
    public bool OnHoney = false;
    private Transform originalParent;
    private int originalLayer;


    private void Awake()
    {
        originalParent = transform.parent;
        originalLayer = gameObject.layer;
    }

    public void ResetGrab()
    {
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
        }

        else
        {
            transform.parent = null;
        }


        gameObject.layer = originalLayer;
    }
    
}