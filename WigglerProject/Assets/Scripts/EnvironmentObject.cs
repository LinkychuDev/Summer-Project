using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentObject : MonoBehaviour, IGrabbable
{
    public bool OnHoney = false;
    private int originalLayer;
    private FixedJoint joint;


    private void Awake()
    {
        originalLayer = gameObject.layer;
    }


    public void SetupGrab(Rigidbody connectedBody)
    {
        if (joint != null)
        {
            ResetGrab();
        }
        
        gameObject.layer = connectedBody.gameObject.layer;
        joint = gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = connectedBody;
        joint.connectedMassScale = 0.0001f;
    }
    public void ResetGrab()
    {
        Destroy(joint);
        joint = null;
        gameObject.layer = originalLayer;
    }
    
}