using System;
using UnityEngine;

public class WallCollision : CollisionBlock
{
    private Collider collider;

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }

    public void OnEnable()
    {
        PlayerReferenceManager.OnStateChange += OnStateChange;
    }

    void OnDisable()
    {
        PlayerReferenceManager.OnStateChange -= OnStateChange;
    }

    private void OnStateChange(PlayerState obj)
    {
        if(obj == PlayerState.Stretching)
        {
            collider.enabled = false;
        }
        
        else
        {
            collider.enabled = true;
        }
    }


    private void OnDrawGizmos()
    {
        /*Gizmos.color = Color.yellow;


        switch (collider)
        {
            case BoxCollider boxCollider:
                Gizmos.matrix = boxCollider.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
                break;
            case SphereCollider sphereCollider:
                Gizmos.matrix = sphereCollider.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
                break;
            default:
                // fallback: draw bounds
                Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
                break;
            
        }*/
    }
}