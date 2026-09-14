using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class HubLevelDefiner : LevelDefiner
{
    public UnityEvent OnOrbPartsCollected;
    public int orbsPartsNeeded = 3;
    public Transform warpHubPosition;
    private int orbsNeeded = 0;
    public float resetWaitTime = 1f;
    public OrbPartHud orbPartHud;
    
    public void CollectOrbPart(int id)
    {
        orbsNeeded++;
        orbPartHud.ActivateOrb(id);
        StartCoroutine(OrbPartSequence());
    }

    IEnumerator OrbPartSequence()
    {

        if (orbsNeeded >= orbsPartsNeeded)
        {
            OnOrbPartsCollected?.Invoke();
        }
        
        
        yield return new WaitForFixedUpdate();
       
        PlayerReferenceManager.instance.headSegment.isKinematic = true;
        PlayerReferenceManager.instance.bodySegment.isKinematic = true;
        PlayerReferenceManager.instance.tailSegment.isKinematic = true;
        yield return new WaitForFixedUpdate();
        PlayerReferenceManager.instance.SpawnPlayerAtPosition(warpHubPosition.position);
        
        yield return new WaitForSeconds(resetWaitTime);
        PlayerReferenceManager.instance.headSegment.isKinematic = false;
        PlayerReferenceManager.instance.bodySegment.isKinematic = false;
        PlayerReferenceManager.instance.tailSegment.isKinematic = false;
       
    }
}