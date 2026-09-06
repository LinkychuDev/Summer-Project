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

        yield return new WaitForFixedUpdate();
        PlayerReferenceManager.instance.SpawnPlayerAtPosition(warpHubPosition.position);
        PlayerReferenceManager.instance.headSegment.isKinematic = true;
        yield return new WaitForSeconds(resetWaitTime);
        PlayerReferenceManager.instance.headSegment.isKinematic = false;
        if (orbsNeeded >= orbsPartsNeeded)
        {
            OnOrbPartsCollected?.Invoke();
        }
    }
}