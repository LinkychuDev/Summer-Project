using UnityEngine;
using UnityEngine.Events;

public class SpecialLeverScript : LeverScript
{
    public UnityEvent OnTimeStartEvent;
    public bool hasActivatedOnce;


    
    public override void GrabMove()
    {
        //rb.transform.forward = forwardVector;
        //base.GrabMove(headRigidbodyPosition, forwardVector, grabSpeed);
       
        var distance = Vector3.Distance(rb.position, leverOrigin.transform.position);
         
        Debug.Log($"Lever Distance: " + distance);
       
        if (distance > maxPullDistance)
        {
            if (hasActivatedOnce)
            {
                if (!activated)
                {
                    Debug.Log("Triggered");
                    OnLeverPulledEvent?.Invoke();
                    activated = true;
                }
            }

            else
            {
                OnTimeStartEvent?.Invoke();
                hasActivatedOnce = true;
                activated = true;
               
            }

        }
        
        
        
        
        
        
        
    }
}
