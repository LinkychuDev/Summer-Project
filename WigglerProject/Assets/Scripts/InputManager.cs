using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public enum CurrentDevice
{
    
    Keyboard,
    Gamepad
}

public class InputManager : MonoBehaviour
{
    public static InputManager instance {get; private set;}
    public Player_Inputs controls {get; private set;}
    private InputDevice lastInputDevice;

    public bool isStretchHeldDown;
    //private bool isClimbing;

    
    public CurrentDevice currentDevice {get; private set;}
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            controls = new Player_Inputs();
        }

        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        isStretchHeldDown = controls.Gameplay.Stretch.ReadValue<float>() > 0.1f;
    }

    private void OnEnable()
    {
        controls?.Enable();
        InputSystem.onEvent += OnInputDeviceChanged;
        //PlayerController.ClimbEvent += b => isClimbing = b;
    }

    private void OnDisable()
    {
        controls?.Disable();
        InputSystem.onEvent -= OnInputDeviceChanged;
        //PlayerController.ClimbEvent -= b => isClimbing = false;
    }


    private void OnInputDeviceChanged(InputEventPtr eventPtr,  InputDevice device)
    {
        if (device != null)
        {
            if(lastInputDevice == device)
                return;
            lastInputDevice = device;
            if (lastInputDevice is Gamepad)
            {
                currentDevice = CurrentDevice.Gamepad;
            }

            else
            {
                currentDevice = CurrentDevice.Keyboard;
            }
        }
    }
    
    
    public Vector3 GetInputVector(Vector2 input, Vector3 f, Vector3 r, bool isClimbing, bool isOnSlope, Vector3 climbDir, Vector3 slopeNormal )
    {
        
        /*if (isClimbing)
        {
            inputVector = transform.right * input.x + transform.forward * input.y;
            Debug.Log("Climbing Input: " + inputVector);
        }*/

        Vector3 forward = f;
        Vector3 right = r;
        forward.y = 0;
        forward.Normalize();
        right.y = 0;
        right.Normalize();
       
       

        if (isClimbing)
        {
           
            //Debug.Log("Right: " + right);
            
            Debug.Log("Climb Dir:  " + climbDir);
            
            
            Debug.Log("Forward Dir:  " + forward);

            right = Vector3.Cross(climbDir, forward);
            
            Debug.Log("Right Dir:  " + right);

            right = -Vector3.ProjectOnPlane(right, climbDir);
            forward = -Vector3.ProjectOnPlane(forward, climbDir);

        }

       
        var inputVector = right * input.x + forward * input.y;

        
        if (isOnSlope)
        {
            inputVector = Vector3.ProjectOnPlane(inputVector, slopeNormal);
        }

       
        
       
        return inputVector.normalized;
    }

}
