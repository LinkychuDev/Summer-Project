using System;
using UnityEngine;


[System.Serializable]
public class PlayerControlsHUD
{
    public GameObject hudCanvasGameplay;
    public GameObject hudCanvasKeyboard;
}
public class PlayerHUDManager : MonoBehaviour
{
    public PlayerControlsHUD locomotionHUD, stretchHUD, swingHUD;
    public PlayerState currentPlayerState;
    private CurrentDevice currentDevice;

    private void OnEnable()
    {
        PlayerReferenceManager.OnStateChange += OnStateChange;
        InputManager.OnInputChanged += OnInputChanged;
    }

    private void OnInputChanged(CurrentDevice obj)
    {
        currentDevice  = obj;
        UpdatePlayerHUDCanvas();
    }

    void OnDisable()
    {
        PlayerReferenceManager.OnStateChange -= OnStateChange;
        InputManager.OnInputChanged -= OnInputChanged;
    }

    private void OnStateChange(PlayerState obj)
    {
       
        currentPlayerState = obj;
        UpdatePlayerHUDCanvas();
    }

    void UpdatePlayerHUDCanvas()
    {
        HideHUD();
        
        switch (currentPlayerState)
        {
            case PlayerState.Default:
                ShowHUD(locomotionHUD, currentDevice);
                break;
            case PlayerState.Locomotion:
                ShowHUD(locomotionHUD, currentDevice);
                break;
            case PlayerState.Stretching:
                ShowHUD(stretchHUD, currentDevice);
                break;
            case PlayerState.Stuck:
                ShowHUD(stretchHUD, currentDevice);
                break;
            case PlayerState.Swinging:
                ShowHUD(swingHUD, currentDevice);
                break;
            case PlayerState.Launching:
                ShowHUD(locomotionHUD, currentDevice);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void ShowHUD(PlayerControlsHUD hud, CurrentDevice obj)
    {
        if (obj == CurrentDevice.Gamepad)
        {
            hud.hudCanvasGameplay.SetActive(true);
            hud.hudCanvasKeyboard.SetActive(false);
        }

        else
        {
            hud.hudCanvasGameplay.SetActive(false);
            hud.hudCanvasKeyboard.SetActive(true);
        }
    }
    

    void HideHUD()
    {
        locomotionHUD.hudCanvasGameplay.SetActive(false);
        locomotionHUD.hudCanvasKeyboard.SetActive(false);
        stretchHUD.hudCanvasKeyboard.SetActive(false);
        stretchHUD.hudCanvasGameplay.SetActive(false);
        swingHUD.hudCanvasKeyboard.SetActive(false);
        swingHUD.hudCanvasGameplay.SetActive(false);
        
        
            
    }
}