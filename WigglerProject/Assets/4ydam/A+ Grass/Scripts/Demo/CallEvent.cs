using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CallEvent : MonoBehaviour
{
    [SerializeField] private UnityEvent OnLeftClickEvent;
    [SerializeField] private UnityEvent OnRightClickEvent;
    private Mouse mouse;

    private void Awake()
    {
        mouse = Mouse.current;
    }
    private void Update()
    {
        if (mouse == null)
            return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            OnLeftClickEvent?.Invoke();
        }
        else if (mouse.rightButton.wasPressedThisFrame)
        {
            OnRightClickEvent?.Invoke();
        }
    }
}
