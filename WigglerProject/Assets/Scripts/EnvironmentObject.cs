using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentObject : StickableObject, IGrabbable, IMetalBreakable
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
    
    public bool isGrounded;
    //private Transform grabPoint;

    public float groundOffset;
    public float groundRadius;
    
    public LayerMask collisionMask;
    public float collisionRadius;
    protected Collider[] _colliders;
    public int maxCollisions = 1;

    private FixedJoint _fixedJoint;
    
    

    public override bool CanStick()
    {
        return OnHoney;


    }

    public interface IGrabEvent
    {
        void GrabEvent(EnvironmentObject sender);
    }
    protected virtual void Awake()
    {
        originalLayer = gameObject.layer;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.freezeRotation = true;

        if (transform.parent != null)
        {
            originalParent = transform.parent;
        }

        _colliders = new Collider[maxCollisions];
        if(honeyObject == null)
            return;
        honeyObject?.SetActive(OnHoney);
    }

   


    public virtual void FixedUpdate()
    {
        if(!isGrabbed)
            return;
        var numCols = Physics.OverlapSphereNonAlloc(rb.position, collisionRadius, _colliders, collisionMask, QueryTriggerInteraction.Collide);
        if ( numCols > 0)
        {
            for (int i = 0; i < numCols; i++)
            {
              
                if (_colliders[i].TryGetComponent(out IGrabEvent grabEvent))
                {
                        grabEvent.GrabEvent(this);
                }
                
               
            }
        }
    }

    void CreateHoneyDecal()
    {
        /*if (honeyDecal == null)
        {
            honeyDecal = Instantiate(GameManager.instance.honeyDecal, transform.position, Quaternion.identity, transform);
        }
        
        else
        {
            honeyDecal.SetActive(true);
        }*/
        
        if(honeyObject == null)
            return;
        honeyObject?.SetActive(OnHoney);
       
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
    public virtual void SetupGrab(Rigidbody grabPoint)
    {
        if(!canGrab)
            return;
        if (isGrabbed)
        {
            return;
        }

        rb.interpolation = RigidbodyInterpolation.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = false;

        int grabLayer = grabPoint.gameObject.layer;
        rb.excludeLayers = 1 << grabLayer;       
        
        
        /*grabPointReference = grabPoint;
                transform.parent = grabPointReference;
                gameObject.layer = grabPoint.gameObject.layer;*/

        _fixedJoint = gameObject.AddComponent<FixedJoint>();
        _fixedJoint.connectedBody = grabPoint;
        _fixedJoint.connectedMassScale = 0.01f;
        _fixedJoint.enableCollision = false;
        _fixedJoint.transform.forward = grabPoint.transform.forward;
        isGrabbed = true;
        
    }
    public virtual void ResetGrab()
    {

        rb.interpolation = RigidbodyInterpolation.None;
        isGrabbed = false;
        /*gameObject.layer = originalLayer;
        transform.parent = originalParent;
        grabPointReference = null;*/
        rb.excludeLayers = 0;
        Destroy(_fixedJoint);
        rb.isKinematic = true;
        isGrabbed = false;
        //Physics.IgnoreLayerCollision(gameObject.layer, PlayerReferenceManager.instance.headSegment.gameObject.layer, false);

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

    public virtual void PreReleaseGrab()
    {
        
    }
}