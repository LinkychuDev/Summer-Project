using UnityEngine;
using UnityEngine.Events;

public class SpecialLeverScript : LeverScript
{
    public UnityEvent OnTimeStartEvent;
    public bool hasActivatedOnce;


    
    internal override void RetractedEvent()
    {
     
        var distance = Vector3.Distance(rb.position, leverOrigin.transform.position);
         
    
        if (distance > maxPullDistance)
        {
            if (hasActivatedOnce)
            {
                if (!activated)
                {
                    
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
