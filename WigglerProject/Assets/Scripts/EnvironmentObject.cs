using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentObject : MonoBehaviour, IGrabbable, IStickable, IMetalBreakable
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

    private GameObject honeyDecal;

    public bool useGravity = true; 
    
    private const float gravity = 9.81f;
    
    bool isGrounded;
    //private Transform grabPoint;
    protected virtual void Awake()
    {
        originalLayer = gameObject.layer;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        if (transform.parent != null)
        {
            originalParent = transform.parent;
        }


        if (OnHoney)
        {
            CreateHoneyDecal();
        }
    }

    private void OnValidate()
    {
        //CreateHoneyDecal();
        //honeyDecal.SetActive(OnHoney));
        
    }


    private void FixedUpdate()
    {
        isGrounded = Physics.SphereCast(transform.position, 0.2f, Vector3.down, out RaycastHit hit, 0.3f,
            PlayerReferenceManager.instance.groundMask);

        if (!isGrounded && useGravity)
        {
            Vector3 gravityVector = Vector3.up * (gravity * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + (gravityVector* Time.fixedDeltaTime));
        }
        
        
    }

    void CreateHoneyDecal()
    {
        if (honeyDecal == null)
        {
            honeyDecal = Instantiate(GameManager.instance.honeyDecal, transform.position, Quaternion.identity, transform);
        }
        
        else
        {
            honeyDecal.SetActive(true);
        }
       
    }
    public void SetupHoney()
    {
       // honeyObject.SetActive(true);
       CreateHoneyDecal();
        StartCoroutine(RemoveHoney());
    }

    IEnumerator RemoveHoney()
    {
        Debug.LogWarning("Please replace honey cooldown with a GameManager version");
        yield return new WaitForSeconds(honeyCooldown);
        //honeyObject.SetActive(false);
        if (honeyDecal != null)
        {
            honeyDecal.SetActive(false);
        }
    }
    public virtual void SetupGrab(Transform grabPoint)
    {
        if(!canGrab)
            return;
        if (isGrabbed)
        {
            return;
        }
        
        rb.isKinematic = true;
        grabPointReference = grabPoint;
        transform.parent = grabPointReference;
        gameObject.layer = grabPoint.gameObject.layer;
        isGrabbed = true;
        
    }
    public virtual void ResetGrab()
    {
       
        
        isGrabbed = false;
        gameObject.layer = originalLayer;
        transform.parent = originalParent;
        grabPointReference = null;
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

    public void MetalBreak()
    {
        if (isBreakable)
        {
            Destroy(gameObject);
        }
    }
}