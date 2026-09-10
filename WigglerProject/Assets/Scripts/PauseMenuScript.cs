using System;
using DG.Tweening;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuScript : MenuScript
{
   public float hoverTime;
   public float hoverScale = 1.25f;

   public GameObject options, normalMenu, controlsMenu;
   public MMF_Player loadScene;
   
   public Button ReturnToHubButton;


   private BackgroundMusicPlayer _backgroundMusicPlayer;
   public GameObject gameCanvas, berryCanvas;

   private void OnEnable()
   {
      PlayerController.OnMenuOpenedEvent += OnMenuOpenedEvent;
   }

   private void OnDisable()
   {
      PlayerController.OnMenuOpenedEvent -= OnMenuOpenedEvent;
   }


   public override void Start()
   {
      base.Start();
      _backgroundMusicPlayer = FindFirstObjectByType<BackgroundMusicPlayer>();
   }
   
   private void OnMenuOpenedEvent(bool obj)
   {
      if (obj)
      {
         
         Time.timeScale = 0;
         InputManager.instance.ChangeGameState(GameState.UIState);
         normalMenu.SetActive(true);
         options.SetActive(false);
         controlsMenu.SetActive(false);
         gameCanvas.SetActive(false);
         berryCanvas.gameObject.SetActive(false);
         ReturnToHubButton.gameObject.SetActive(SceneManager.GetActiveScene().name != "HubWorld");
        _backgroundMusicPlayer.StopMusic();
         
         OpenMenu();
      }

      else
      {
        
         InputManager.instance.ChangeGameState(GameState.GameplayState);
         CloseMenu();
         options.SetActive(false);
         normalMenu.SetActive(false);
         controlsMenu.SetActive(false);
         gameCanvas.SetActive(true);
         berryCanvas.gameObject.SetActive(true);
         Time.timeScale = 1;
         _backgroundMusicPlayer.PlayMusic();
      }
   }

   public void OnHover(Transform t)
   {
      t.localScale = new Vector3(hoverScale, hoverScale, hoverScale);
      Debug.Log("Hovered");
   }

   public void OnHoverExit(Transform t)
   {
      t.localScale = Vector3.one;
   }

   public void ReturnToHub()
   {
      options.SetActive(false);
      normalMenu.SetActive(false);
      controlsMenu.SetActive(false);
      MMAdditiveSceneLoadingManager.LoadScene("HubWorld");
      
   }

   public void ResumeGame()
   {
      PlayerController.OnMenuOpenedEvent?.Invoke(false);
      options.SetActive(false);
      normalMenu.SetActive(false);
      controlsMenu.SetActive(false);
   }

   public void OptionsMenu()
   {
         options.SetActive(true);
         normalMenu.SetActive(false);
         controlsMenu.SetActive(false);
   }

   public void PauseMenu()
   {
      options.SetActive(false);
      normalMenu.SetActive(true);
      controlsMenu.SetActive(false);
   }

   public void DisplayControls()
   {
      options.SetActive(false);
      normalMenu.SetActive(false);
      controlsMenu.SetActive(true);
   }
}
