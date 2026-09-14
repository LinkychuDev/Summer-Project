using System;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[System.Serializable]
public class ControlsImageDisplay
{
   public GameObject diplayImage;
  
}



public class PauseMenuScript : MenuScript
{
   public float hoverTime;
   public float hoverScale = 1.25f;

   public GameObject options, normalMenu, controlsMenu;
  
   
   public Button ReturnToHubButton;


   private BackgroundMusicPlayer _backgroundMusicPlayer;
   public GameObject gameCanvas, berryCanvas;
   
   [Header("Controls")]
   public int currentSelectedControlsMenu = 0;

   
   public Button arrowLeft, arrowRight;
   public ControlsImageDisplay[] controlsDisplay;

   
   bool isControlsOpen = false;
   private float navInput;

   public MMF_Player hoverEffect;
   
   public MMF_Player selectedEffect;
   [Header("Options")]
   public Slider masterSlider, musicSlider, sfxSlider, uiSlider;

   private Vector3 originalScaleCache;

   public Dictionary<GameObject, Vector3> buttonsInformation = new Dictionary<GameObject, Vector3>();
  
   private void OnEnable()
   {
      PlayerController.OnMenuOpenedEvent += OnMenuOpenedEvent;
      arrowLeft.onClick.AddListener(() => OnNavigatePerformedButton(-1));
      arrowRight.onClick.AddListener(() => OnNavigatePerformedButton(1));
      masterSlider.onValueChanged.AddListener(x => MMSoundManager.Instance.SetVolumeMaster(x));
      musicSlider.onValueChanged.AddListener(x => MMSoundManager.Instance.SetVolumeMusic(x));
      sfxSlider.onValueChanged.AddListener(x => MMSoundManager.Instance.SetVolumeSfx(x));
      uiSlider.onValueChanged.AddListener(x => MMSoundManager.Instance.SetVolumeUI(x));
      
   }
   

   public void OnNavigatePerformedButton(int dir)
   {
      if(!controlsMenu.activeInHierarchy)
         return;
      currentSelectedControlsMenu = (currentSelectedControlsMenu + dir)  % controlsDisplay.Length;
      
      NextControlsMenu();
   }


   private void OnDisable()
   {
      PlayerController.OnMenuOpenedEvent -= OnMenuOpenedEvent;
      arrowLeft.onClick.RemoveAllListeners();
      arrowRight.onClick.RemoveAllListeners();
      masterSlider.onValueChanged.RemoveAllListeners();
      musicSlider.onValueChanged.RemoveAllListeners();
      sfxSlider.onValueChanged.RemoveAllListeners();
      uiSlider.onValueChanged.RemoveAllListeners();
   }


   
   
   public override void Start()
   {
      base.Start();
      _backgroundMusicPlayer = FindFirstObjectByType<BackgroundMusicPlayer>();
   }
   
   public void OnMenuOpenedEvent(bool obj)
   {
      if (obj)
      {
         currentSelectedControlsMenu = 0;
         
         Time.timeScale = 0;
         InputManager.instance.ChangeGameState(GameState.UIState);
         normalMenu.SetActive(true);
         options.SetActive(false);
         controlsMenu.SetActive(false);
         berryCanvas.gameObject.SetActive(false);
         ReturnToHubButton.gameObject.SetActive(SceneManager.GetActiveScene().name != "HubWorld");
        _backgroundMusicPlayer.StopMusic();
        foreach (var kvp in buttonsInformation)
        {
           kvp.Key.transform.localScale = kvp.Value;
        }
         OpenMenu();
      }

      else
      {
         currentSelectedControlsMenu = 0;
  
         InputManager.instance.ChangeGameState(GameState.GameplayState);
         CloseMenu();
         options.SetActive(false);
         normalMenu.SetActive(false);
         controlsMenu.SetActive(false);
         berryCanvas.gameObject.SetActive(true);
         Time.timeScale = 1;
         _backgroundMusicPlayer.PlayMusic();
      }
   }

   public void OnHover(Transform t)
   {

      if (!buttonsInformation.ContainsKey(t.gameObject))
      {
         buttonsInformation.Add(t.gameObject, t.localScale);
      }
      
      t.localScale = buttonsInformation[t.gameObject];
      t.localScale += new Vector3(hoverScale, hoverScale, hoverScale);
      Debug.Log("Hovered");
      hoverEffect.PlayFeedbacks();
   }

   public void OnHoverExit(Transform t)
   {
      if (buttonsInformation.TryGetValue(t.gameObject, out var value))
      {
         t.localScale = value;
      }
     
   }

   public void ReturnToHub()
   {
      options.SetActive(false);
      normalMenu.SetActive(false);
      controlsMenu.SetActive(false);
      currentSelectedControlsMenu = 0;
    
      MMAdditiveSceneLoadingManager.LoadScene("HubWorld");
      
   }

   public void ResumeGame()
   {
      PlayerController.OnMenuOpenedEvent?.Invoke(false);
      options.SetActive(false);
      normalMenu.SetActive(false);
      controlsMenu.SetActive(false);
      currentSelectedControlsMenu = 0;
     
   }

   public void OptionsMenu()
   {
         options.SetActive(true);
         normalMenu.SetActive(false);
         controlsMenu.SetActive(false);
         currentSelectedControlsMenu = 0;
         SetFirstSelected(masterSlider.gameObject);
        
         
   }

   public void Select(Transform t)
   {
      if (buttonsInformation.TryGetValue(t.gameObject, out var value))
      {
         t.localScale = value;
      }
      selectedEffect.PlayFeedbacks();
   }


   public void NextControlsMenu()
   {
      
      
      
      for (int i = 0; i < controlsDisplay.Length; i++)
      {
         if (i == currentSelectedControlsMenu)
         {
            controlsDisplay[i].diplayImage.SetActive(true);
            
         }

         else
         {
            controlsDisplay[i].diplayImage.SetActive(false);
         }
      }
      
   }

   public void PauseMenu()
   {
      options.SetActive(false);
      normalMenu.SetActive(true);
      controlsMenu.SetActive(false);
      currentSelectedControlsMenu = 0;
   }

   public void DisplayControls()
   {
      options.SetActive(false);
      normalMenu.SetActive(false);
      controlsMenu.SetActive(true);
      SetFirstSelected(arrowRight.gameObject);
      currentSelectedControlsMenu = 0;
      NextControlsMenu();
      
      
   }
}
