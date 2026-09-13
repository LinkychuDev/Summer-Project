using MoreMountains.Feedbacks;
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
                    base.RetractedEvent();
                }
            }

            else
            {
                OnTimeStartEvent?.Invoke();
                hasActivatedOnce = true;
                tickingTimer.GetFeedbackOfType<MMF_MMSoundManagerSound>().PlaybackDuration =
                    new Vector2(resetTime, resetTime);
                tickingTimer.PlayFeedbacks(transform.position);

            }

        }
        
        
        
        
        
        
        
    }
}
