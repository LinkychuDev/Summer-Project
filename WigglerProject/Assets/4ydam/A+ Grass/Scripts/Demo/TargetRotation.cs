using UnityEngine;
using UnityEngine.InputSystem;

public class TargetRotation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 720f;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (targetCamera == null)
            return;

        if (Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = targetCamera.ScreenPointToRay(mousePosition);

        Plane plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        if (!plane.Raycast(ray, out float enter))
            return;

        Vector3 hitPoint = ray.GetPoint(enter);
        Vector3 lookDirection = hitPoint - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
