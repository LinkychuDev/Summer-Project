using System;
using UnityEngine;

public class BerryScript : MonoBehaviour
{
    public int PointToGive = 1;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
           GameManager.instance.AddPoints(PointToGive);     
           Destroy(gameObject);
        }
    }
}
