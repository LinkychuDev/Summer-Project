using System;
using System.Collections;
using DG.Tweening;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public interface IBreakable
{
    void Break();
}
public interface IMetalBreakable
{
    void MetalBreak();
}


public enum PlayerActionEvent
{
    NoAction,
    ClimbAction,
    StretchAction,
    ReleaseAction,
    StickAction,
    PullAction,
    SwingAction
}

public class PlayerController : MonoBehaviour
{
    
    [Header("Honey Power")]
    [SerializeField] private GameObject HoneyVisualiser;    
    [SerializeField] private GameObject StretchVisualiser; 
   
    public static event Action<bool> isOnHoneyEvent;
    [SerializeField] private float honeyCooldown;
    
    
    [Header("Sturdy Power")]
    [SerializeField] private GameObject SturdyVisualiser;
    [SerializeField] private float sturdyCooldown;
    public float sturdyRatio = 0.5f;
    public static event Action<float, bool> isOnSturdyEvent;
    
    

    public static Action<EnvironmentObject> OnGrabEvent;
    public static Action OnReleaseEvent;
    
    public static Action OnPreReleaseEvent;
    
    public static event Action<bool> SilkEvent;


    public bool isOnSilk;
    
    
    public static Action<bool> ClimbEvent;

   
    public PlayerState currentState;

    private Transform headVisual;
    private Vector3 scaleEffect = new Vector3(1.5f,  1.5f, 1.5f);
    private Vector3 previousScale;


    [SerializeField] private GameObject honeyCanvas;
    [SerializeField] private Image honeyImage;
    

  
    [Header("Collision")] public static Action OnWaterEvent;


    public MMF_Player honeyTimerPlayer;
    public static Action<bool> OnMenuOpenedEvent;
    private Coroutine honeyCoroutine;

    
    
    private void OnEnable()
    {
        PlayerReferenceManager.OnStateChange += OnStateChange;
        PlayerStretch.onStretchStateChanged += OnStretchStateChanged;
        InputManager.instance.controls.Gameplay.Return.started += ReturnOnStarted;
       
    }

    private void ReturnOnStarted(InputAction.CallbackContext obj)
    {
        OnMenuOpenedEvent?.Invoke(true);
    }


    private void OnStretchStateChanged(PlayerStretch.StretchState obj)
    {
        
    }

    private void OnDisable()
    {
        PlayerReferenceManager.OnStateChange -= OnStateChange;
        PlayerStretch.onStretchStateChanged -= OnStretchStateChanged;
        InputManager.instance.controls.Gameplay.Return.started -= ReturnOnStarted;
      
    }

    private void Start()
    {
        headVisual = PlayerReferenceManager.instance.segments[0].visual;
        previousScale = headVisual.localScale;
        Honeyfied(false);
        Sturdy(false);
    }

    private void OnStateChange(PlayerState obj)
    {
        currentState = obj;

       
    }

    public void Sturdy(bool val)
    {
        StopCoroutine(SturdyCooldown());
        isOnSturdyEvent?.Invoke(!val ? 1 : sturdyRatio, val);
        SturdyVisualiser.SetActive(val);
        if (val)
        {
           
            StartCoroutine(SturdyCooldown());
        }
    }

    public void Honeyfied(bool val)
    {
        if (honeyCoroutine != null)
        {
            StopCoroutine(honeyCoroutine);
            
        }
        
        honeyCanvas.gameObject.SetActive(val);
        HoneyVisualiser.SetActive(val);
        isOnHoneyEvent?.Invoke(val);
        if (val)
        {
            honeyCoroutine = StartCoroutine(HoneyCooldown());
        }

        else
        {
            ResetHoneyState();
        }
        
    }

    IEnumerator HoneyCooldown()
    {
        
        
        
        
        honeyImage.fillAmount = 1;
        
       
      
        if (currentState == PlayerState.Stretching)
        {
            yield return new WaitUntil(() =>
                PlayerReferenceManager.instance.currentState != PlayerState.Stretching);
        }
        honeyTimerPlayer.GetFeedbackOfType<MMF_MMSoundManagerSound>().PlaybackDuration =
            new Vector2(honeyCooldown, honeyCooldown);
        honeyTimerPlayer.GetFeedbackOfType<MMF_MMSoundManagerSound>().SetFeedbackDuration(honeyCooldown);
        honeyTimerPlayer.PlayFeedbacks();
        DOVirtual.Float(1, 0.02f, honeyTimerPlayer.TotalDuration, value =>  honeyImage.fillAmount = value );

        yield return new WaitForSeconds(honeyCooldown);
        yield return new WaitUntil(() => !honeyTimerPlayer.HasFeedbackStillPlaying());
        ResetHoneyState();
        
        //Honeyfied(false);
    }


    void ResetHoneyState()
    {
        HoneyVisualiser.SetActive(false);
        isOnHoneyEvent?.Invoke(false);
        honeyImage.fillAmount = 0;
        honeyCanvas.gameObject.SetActive(false);
        honeyTimerPlayer.GetFeedbackOfType<MMF_MMSoundManagerSound>().PlaybackDuration =
            new Vector2(honeyCooldown, honeyCooldown);
    }
    IEnumerator SturdyCooldown()
    {
        yield return new WaitForSeconds(sturdyCooldown);
        if (currentState == PlayerState.Stretching)
        {
            yield return new WaitUntil(() =>
                PlayerReferenceManager.instance.currentState != PlayerState.Stretching);
        }
        SturdyVisualiser.SetActive(false);
        isOnSturdyEvent?.Invoke(1, false);
    }
    
   

    public static void OnSilkEvent(bool obj)
    {
        SilkEvent?.Invoke(obj);
    }
}
