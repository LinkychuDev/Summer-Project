using System;
using UnityEngine;

public class OrbHudActivator : MonoBehaviour
{
    private bool hasActivated;
    public OrbPartHud orbPartHud;
    private void OnTriggerEnter(Collider other)
    {
        if(hasActivated)
            return;
        if (other.TryGetComponent(out PlayerController player))
        {
            if (PlayerReferenceManager.instance.currentState == PlayerState.Locomotion)
            {
                orbPartHud.gameObject.SetActive(true);
                hasActivated = true;
            }
        }
    }
}
