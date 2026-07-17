using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentObject : MonoBehaviour, IGrabbable, IStickable
{
    public bool OnHoney = false;
    private int originalLayer;
   // private FixedJoint joint;
    public GameObject honeyObject;
    protected Rigidbody rb;
    public bool isDynamic;
    
    bool isGrabbed = false;
    private void Awake()
    {
        originalLayer = gameObject.layer;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = !isDynamic;
    }


    public void SetupHoney()
    {
        honeyObject.SetActive(true);
        StartCoroutine(RemoveHoney());
    }

    IEnumerator RemoveHoney()
    {
        Debug.LogWarning("Please replace honey cooldown with a GameManager version");
        yield return new WaitForSeconds(PlayerReferenceManager.instance.honeyCooldown);
        honeyObject.SetActive(false);
    }
    public void SetupGrab(Rigidbody connectedBody)
    {
        if (isGrabbed)
        {
            return;
        }
        rb.isKinematic = false;
        gameObject.layer = connectedBody.gameObject.layer;
        //joint = gameObject.AddComponent<FixedJoint>();
       // joint.connectedBody = connectedBody;
        //joint.connectedMassScale = 0.0001f;
    }
    public void ResetGrab()
    {
        //Destroy(joint);
        //joint = null;
        isGrabbed = false;
        gameObject.layer = originalLayer;
        rb.isKinematic =  !isDynamic;
    }

    public virtual void GrabMove(Vector3 headRigidbodyPosition)
    {
        rb.MovePosition(headRigidbodyPosition);
    }
}