using System;
using UnityEngine;
using UnityEngine.Events;

public class SwitchScript : MonoBehaviour
{
    public UnityEvent onActivatedEvent, onDeactivatedEvent;
    [SerializeField] private bool isRepeating;
    public bool isActivated;

    public EnvironmentObject obj;

    private Collider triggerCollider;
    public int stayCount;
    void Start()
    {
        //onDeactivatedEvent?.Invoke();
    }

    

    private void OnTriggerStay(Collider other)
    {
        if (isActivated)
            return;
        if (other.TryGetComponent(out EnvironmentObject rb))
        {
           
            isActivated = true;
            obj = rb;
            onActivatedEvent.Invoke();

            if (!isRepeating)
            {
                rb.canGrab = false;
            }
        }

        
        else if(other.TryGetComponent(out PlayerStretch stretch))
        {
            if(obj != null)
                return;
            if(stretch.stretchState != PlayerStretch.StretchState.None)
                return;
            isActivated = true;
            onActivatedEvent.Invoke();
        }
        
        
    }

    private void OnTriggerExit(Collider other)
    {
        if(!isActivated)
            return;
        if(!isRepeating)
            return;
        if (other.TryGetComponent(out EnvironmentObject rb))
        {
            obj = null;
            isActivated = false;
            onDeactivatedEvent.Invoke();
        }

        else if(other.TryGetComponent(out PlayerStretch stretch))
        {
            if(obj != null)
                return;
            if(stretch.stretchState != PlayerStretch.StretchState.None)
                return;
            isActivated = false;
            onDeactivatedEvent.Invoke();
        }
       
    }
}
