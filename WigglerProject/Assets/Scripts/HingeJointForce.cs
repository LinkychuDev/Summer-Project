using UnityEngine;

public class HingeJointForce : MonoBehaviour
{
    Rigidbody rb;
    private HingeJoint joint;
    [SerializeField] private float swingForce;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<HingeJoint>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
     
        rb.AddForce(joint.axis * swingForce, ForceMode.VelocityChange);
    }
}
