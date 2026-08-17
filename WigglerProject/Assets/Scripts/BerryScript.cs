using System;
using UnityEngine;

public class BerryScript : MonoBehaviour
{
    public int PointToGive = 1;
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
           GameManager.instance.AddPoints(PointToGive);     
           Destroy(gameObject, 0);
        }
    }
}
