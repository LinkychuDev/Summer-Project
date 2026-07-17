using UnityEngine;

public class HoneyBlob : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] private float speed = 4f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.AddForce(speed * Time.deltaTime * Vector3.down);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController playerController))
        {
            PlayerReferenceManager.instance.Honeyfied(true);
            Debug.Log("Hit Player");
        }

        else if (other.TryGetComponent(out EnvironmentObject environmentObject))
        {
            environmentObject.SetupHoney();
            Debug.Log("Hit EnvironmentObject");
        }

        else
        {
            Debug.Log("Hit Ground");
        }
        Destroy(gameObject);
    }
}
