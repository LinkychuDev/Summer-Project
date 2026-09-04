using DG.Tweening;
using UnityEngine;

public class DoorEnvironmentScript : EnvironmentObject
{
    Vector3 originalPosition;

    public float anglePulled;
    public float stretchAmount;
    public float maxAngle;
    private float stretchTime;
    public float maxStretchTime;
    public float maxStretchDistance;


    public float pullMaxAngle;
    public float minStretchTime;

    public bool hasReachedMaxAngle;


    private Collider doorPullCollider;
    public Collider doorLeft, doorRight;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.position;
        maxStretchDistance = PlayerReferenceManager.instance.playerStretch.stretchDistanceHead;
        hasReachedMaxAngle = false;
        doorPullCollider = GetComponent<Collider>();
        doorLeft.enabled = false;
        doorRight.enabled = false;
    }

    public override void SetupGrab(Transform grabPoint)
    {
        if(!canGrab)
            return;
        if (isGrabbed)
        {
            return;
        }

        
        if(hasReachedMaxAngle)
            return;
        
        
        rb.interpolation = RigidbodyInterpolation.None;
        rb.isKinematic = true;
        
        isGrabbed = true;
        stretchAmount = PlayerReferenceManager.instance.playerStretch.currentStretchDistance;


        anglePulled += Mathf.Lerp(0, pullMaxAngle, stretchAmount / maxStretchDistance);
        
       

        anglePulled = Mathf.Clamp(anglePulled, maxAngle, 0);
        
        stretchTime = Mathf.Lerp(minStretchTime, maxStretchTime, stretchAmount/maxStretchDistance);

        if (anglePulled <= maxAngle)
        {
            hasReachedMaxAngle = true;
        }
    }


    public override void PreReleaseGrab()
    {
        doorRight.transform.DOLocalRotate(transform.up *anglePulled, stretchTime, RotateMode.Fast);
        doorLeft.transform.DOLocalRotate(transform.up * -anglePulled, stretchTime, RotateMode.Fast);
        doorPullCollider.enabled = false;
        doorLeft.enabled = true;
        doorRight.enabled = true;

    }
    public override void ResetGrab()
    {
       
        base.ResetGrab();
       
    }
}
