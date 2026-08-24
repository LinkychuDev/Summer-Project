using System;
using DG.Tweening;
using UnityEngine;

public class SwingAnchorResetTrigger : MonoBehaviour
{
    public Transform resetPoint;
    public Transform anchorPoint;
    public Transform swingPoint;
    public float resetTime;
    
    Vector3 swingOriginalPosition;

    private void Start()
    {

        swingOriginalPosition = swingPoint.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(PlayerReferenceManager.instance.currentState == PlayerState.Swinging)
            return;
        
        if (other.TryGetComponent(out PlayerStretch playerMovement))
        {
            if(playerMovement.stretchState == PlayerStretch.StretchState.Retracting)
                return;
            anchorPoint.DOMove(resetPoint.position, resetTime);
            //swingPoint.DOLocalMove(swingOriginalPosition, resetTime);
            swingPoint.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }
}
