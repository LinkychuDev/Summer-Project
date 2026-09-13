using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class WindBlow : MonoBehaviour
{
    public bool isStatic = true;
    public float windForce = 100;

    public float restTime = 0.5f;
    public float blowDuration = 5f;
    
    private BoxCollider boxCollider;
    public GameObject particles;

    public Vector2 offsetRange = new Vector2(0, 2);

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    public void TurnOn()
    {
        boxCollider.enabled = true;
        particles.SetActive(true);
    }

    public void TurnOff()
    {
        boxCollider.enabled = false;
        particles.SetActive(false);
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(Random.Range(offsetRange.x, offsetRange.y));
        while (!isStatic)
        {
            TurnOn();
            yield return new WaitForSeconds(blowDuration);
            TurnOff();
            yield return new WaitForSeconds(restTime);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Rigidbody rb))
        {
            if (!rb.isKinematic)
            {
                if (rb.TryGetComponent(out PlayerController stretch))
                {
                    if(stretch.currentState == PlayerState.Stretching)
                        return;
                }
                
                
                rb.AddForce(windForce * transform.forward, ForceMode.VelocityChange);
            }
        }
    }
}
