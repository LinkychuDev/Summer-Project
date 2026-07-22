using UnityEngine;

public class HoneyApplePowerup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag( "Player"))
            return;
        if (other.TryGetComponent(out PlayerController player))
        {
            player.Honeyfied(true);
            if(PlayerReferenceManager.instance.currentState == PlayerState.Stretching)
                return;
            if(PlayerReferenceManager.instance.canStretch)
                return;
          
            
        }
    }
}
