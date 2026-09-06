using System;
using System.Collections;
using UnityEngine;

public class HubSectionLevelLoader : MonoBehaviour
{
    public GameObject LinkedObject;

    private bool hasActivated;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController controller))
        {
            if (controller.currentState == PlayerState.Locomotion)
            {
                StartCoroutine(WarpPlayer());
            }
        }
      
    }


    IEnumerator WarpPlayer()
    {
        yield return new WaitForFixedUpdate();
        PlayerReferenceManager.instance.SpawnPlayerAtPosition(LinkedObject.transform.position, LinkedObject.transform.forward);
    }

}
