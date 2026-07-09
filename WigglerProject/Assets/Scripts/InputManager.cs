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

    private void OnEnable()
    {
        controls?.Enable();
        InputSystem.onEvent += OnInputDeviceChanged;
    }

    private void OnDisable()
    {
        controls?.Disable();
        InputSystem.onEvent -= OnInputDeviceChanged;
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
}
