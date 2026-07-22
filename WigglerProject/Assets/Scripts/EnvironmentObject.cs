using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentObject : MonoBehaviour, IGrabbable, IStickable, IBreakable
{
    public bool OnHoney = false;
    private int originalLayer;
   // private FixedJoint joint;
    public GameObject honeyObject;
    protected Rigidbody rb;
    private bool isDynamic;

    public float honeyCooldown;

    protected bool isGrabbed = false;

    protected FixedJoint joint;

    public bool canGrab = true;

    protected const float grabDistance = 0.1f;

    private Transform originalParent;
    
    private Transform grabPointReference;

    public bool isBreakable = false;
    
    //private Transform grabPoint;
    protected virtual void Awake()
    {
        originalLayer = gameObject.layer;
        rb = GetComponent<Rigidbody>();
        isDynamic = rb.isKinematic;

        if (transform.parent != null)
        {
            originalParent = transform.parent;
        }
    }


    public void SetupHoney()
    {
        honeyObject.SetActive(true);
        StartCoroutine(RemoveHoney());
    }

    IEnumerator RemoveHoney()
    {
        Debug.LogWarning("Please replace honey cooldown with a GameManager version");
        yield return new WaitForSeconds(honeyCooldown);
        honeyObject.SetActive(false);
    }
    public virtual void SetupGrab(Transform grabPoint)
    {
        if(!canGrab)
            return;
        if (isGrabbed)
        {
            return;
        }
        
        rb.isKinematic = false;
        grabPointReference = grabPoint;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        transform.parent = grabPointReference;
        rb.MovePosition(grabPoint.position);
        gameObject.layer = grabPoint.gameObject.layer;
        isGrabbed = true;
        
    }
    public virtual void ResetGrab()
    {
       
        
        isGrabbed = false;
        gameObject.layer = originalLayer;
        rb.isKinematic =  isDynamic;
        transform.parent = originalParent;
        grabPointReference = null;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

    }


    protected virtual void Update()
    {
        if (isGrabbed)
        {
            GrabMove();
        }
    }

    public virtual void GrabMove()
    {
        /*if (rb.position != headRigidbodyPosition)
        {
            rb.MovePosition(headRigidbodyPosition);
        }*/

        //if(!canGrab)
        //    return;
       // if(!isGrabbed)
            return;
        //Vector3 rotation = headRigidbodyPosition - rb.position;
        
       // rb.MovePosition(headRigidbodyPosition);
        //rb.Move(Vector3.Lerp(rb.position, headRigidbodyPosition,  grabSpeed * Time.deltaTime), Quaternion.LookRotation(forwardVector, Vector3.up));
        
    }

    public virtual void GrabIdle()
    {
      
    }

    public void Break()
    {
        if (isBreakable)
        {
            Destroy(gameObject);
        }
    }
}