using System;
using DG.Tweening;
using UnityEngine;

public class HookSwingTriggerPoint : MonoBehaviour
{
    public Transform swingPoint;

    public float offset;

    public float swingDropTime;
    
    Vector3 startPos;

    public bool canMove = true;
    
    Sequence swingSequence;

    private void Start()
    {
        startPos = swingPoint.position;
    }

    
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.transform.TryGetComponent(out PlayerMovement playerMovement))
        { 
            if(PlayerReferenceManager.instance.currentState == PlayerState.Swinging)
                return;
            if(!playerMovement.isGrounded)
                return;
            Debug.Log("Entering");
            
            if(!canMove)
                return;
            swingSequence.Append(swingPoint.transform.DOMoveY(PlayerReferenceManager.instance.headSegment.transform.position.y + offset, swingDropTime));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(PlayerReferenceManager.instance.currentState == PlayerState.Swinging)
            return;
        if (other.transform.TryGetComponent(out PlayerMovement playerMovement))
        {
           Debug.Log("Exiting"); 
           Deactivate(canMove);
        }
    }

    public void Deactivate(bool move)
    {
        canMove = move;
        if (!canMove)
        {
            if (swingSequence != null)
            {
                if (swingSequence.IsPlaying())
                {
                    swingSequence.Kill();
                }
            }
        }

        swingSequence.Append(swingPoint.transform.DOLocalMove(new Vector3(0, swingPoint.localPosition.y, 0), swingDropTime));
    }
}
