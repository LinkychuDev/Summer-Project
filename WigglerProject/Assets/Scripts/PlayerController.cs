using System;
using System.Collections;
using DG.Tweening;
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
    PullAction
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

   
    private PlayerState currentState;

    private Transform headVisual;
    private Vector3 scaleEffect = new Vector3(1.5f,  1.5f, 1.5f);
    private Vector3 previousScale;


    [SerializeField] private GameObject honeyCanvas;
    [SerializeField] private Image honeyImage;
    

    [Header("Collision")] public static Action OnWaterEvent;
    
    
    private void OnEnable()
    {
        PlayerReferenceManager.OnStateChange += OnStateChange;
        PlayerStretch.onStretchStateChanged += OnStretchStateChanged;
        InputManager.instance.controls.Gameplay.Return.started += ReturnOnstarted;
       
    }

    private void ReturnOnstarted(InputAction.CallbackContext obj)
    {
        MMAdditiveSceneLoadingManager.LoadScene("HubWorld");
    }


    private void OnStretchStateChanged(PlayerStretch.StretchState obj)
    {
        if (obj == PlayerStretch.StretchState.Stuck)
        {
            headVisual.localScale = scaleEffect;
        }

        else
        {
            headVisual.localScale = previousScale;
        }
    }

    private void OnDisable()
    {
        PlayerReferenceManager.OnStateChange -= OnStateChange;
        PlayerStretch.onStretchStateChanged -= OnStretchStateChanged;
        InputManager.instance.controls.Gameplay.Return.started -= ReturnOnstarted;
      
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

        if (currentState == PlayerState.Locomotion)
        {
            Honeyfied(false);
        }
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
        StopCoroutine(HoneyCooldown());
        HoneyVisualiser.SetActive(val);
        isOnHoneyEvent?.Invoke(val);
        if (val)
        {
            honeyCanvas.gameObject.SetActive(true);
            honeyImage.fillAmount = 1;
            DOVirtual.Float(1, 0, honeyCooldown, value =>  honeyImage.fillAmount = value );
            StartCoroutine(HoneyCooldown());
        }
        
    }

    IEnumerator HoneyCooldown()
    {
        
        yield return new WaitForSeconds(honeyCooldown);
        if (currentState == PlayerState.Stretching)
        {
            yield return new WaitUntil(() =>
                PlayerReferenceManager.instance.currentState != PlayerState.Stretching);
            Debug.Log("Waited for retraction");
        }
        
        HoneyVisualiser.SetActive(false);
        isOnHoneyEvent?.Invoke(false);
        honeyImage.fillAmount = 0;
        honeyCanvas.gameObject.SetActive(false);
        //Honeyfied(false);
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
