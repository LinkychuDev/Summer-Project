using System;
using UnityEngine;
using UnityEngine.UI;


public interface IPlayerHint
{
    public PlayerActionEvent PlayerActionEvent { get; }
    
    
}
public class PlayerHintHelper : MonoBehaviour
{
    private float timer;
    [SerializeField] private float durationUntilPromptShown;
    private bool showingPrompt;
    
    
    [SerializeField] private float promptDuration;
    
    public Sprite ClimbSprite, StretchSprite, ReleaseSprite, PullSprite, StickSprite;
    
    public Canvas promptCanvas;
    public Image promptSprite;

    Rigidbody headRigidBody;
    public float detectionRadius;
    public float sphereRadius;
    private PlayerActionEvent currentPrompt = PlayerActionEvent.NoAction;
    
    bool shouldRetract = false;

    private PlayerStretch.StretchState stretchState;
    
    
    private void Start()
    {
        headRigidBody = PlayerReferenceManager.instance.headSegment;
    }

    private void OnEnable()
    {
       // PlayerReferenceManager.OnStateChange += OnStateChange;
       PlayerStretch.onStretchStateChanged += OnStateChange;
    }

    void OnDisable()
    {
        PlayerStretch.onStretchStateChanged -= OnStateChange;
    }

    private void OnStateChange(PlayerStretch.StretchState stretchState)
    {
        this.stretchState = stretchState;
        
    }

    private void Update()
    {
        if (CanDisplayPrompt(out PlayerActionEvent playerHint))
        {
            Debug.Log("Showing Object");
            //stop the timer from running infinitely
            if (!showingPrompt)
            {
                timer += Time.deltaTime;
            }

                
            //wait a few seconds to display prompt in case the player makes an input
            if (timer >= GetPromptDuration(durationUntilPromptShown, playerHint) && !showingPrompt)
            {
                DisplayPromptEvent(playerHint);
                showingPrompt = true;
            }
        }

        else
        {
            timer = 0;
            showingPrompt = false;
            DisplayPromptEvent(PlayerActionEvent.NoAction);
            Debug.Log("Not detecting prompts");
        }
    }


    float GetPromptDuration(float d, PlayerActionEvent playerActionEvent)
    {
        float duration = d;
        switch (playerActionEvent)
        {
            case PlayerActionEvent.NoAction:
                break;
            case PlayerActionEvent.ClimbAction:
                if (!GameManager.GameEvents.IsEventCompleted(GameFlags.HasUsedFirstWallClimb))
                {
                    duration /= 2;
                }
                break;
            case PlayerActionEvent.StretchAction:
                if (!GameManager.GameEvents.IsEventCompleted(GameFlags.HasUsedFirstStretch))
                {
                    duration /= 2;
                }
                break;
            case PlayerActionEvent.ReleaseAction:
                
                break;
            case PlayerActionEvent.PullAction:
                if (!GameManager.GameEvents.IsEventCompleted(GameFlags.HasUsedFirstPulling))
                {
                    duration /= 2;
                }
                break;
            case PlayerActionEvent.StickAction:
                if (!GameManager.GameEvents.IsEventCompleted(GameFlags.HasUsedFirstSticking))
                {
                    duration /= 2;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerActionEvent), playerActionEvent, null);
        }

        return duration;
    }
    private void DisplayPromptEvent(PlayerActionEvent obj)
    {
        if (obj == PlayerActionEvent.NoAction)
        {
            promptCanvas.gameObject.SetActive(false);
            promptSprite.sprite = null;
        }

        else
        {
            promptCanvas.gameObject.SetActive(true);
            promptSprite.sprite = GetPromptSprite(obj);
        }
    }

    private Sprite GetPromptSprite(PlayerActionEvent playerActionEvent)
    {
        Sprite sprite = null;
        switch (playerActionEvent)
        {
            case PlayerActionEvent.NoAction:
                break;
            case PlayerActionEvent.ClimbAction:
                sprite = ClimbSprite;
                break;
            case PlayerActionEvent.StretchAction:
                sprite = StretchSprite;
                break;
            case PlayerActionEvent.ReleaseAction:
                sprite = ReleaseSprite;
                break;
            case PlayerActionEvent.PullAction:
                sprite = PullSprite;
                break;
            case PlayerActionEvent.StickAction:
                sprite = StickSprite;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerActionEvent), playerActionEvent, null);
        }

        return sprite;
    }


    bool CanDisplayPrompt(out PlayerActionEvent playerAction)
    {
         playerAction = PlayerActionEvent.NoAction;

         if (stretchState == PlayerStretch.StretchState.Stretching)
         {
             playerAction = PlayerActionEvent.ReleaseAction;
             return true;
         }

         if (stretchState == PlayerStretch.StretchState.Stuck)
         {
             playerAction = PlayerActionEvent.StickAction;
             return true;
         }
         
         
        if (Physics.SphereCast(headRigidBody.transform.position, sphereRadius,
                headRigidBody.transform.forward, out RaycastHit hit, detectionRadius,
                PlayerReferenceManager.instance.playerCollisionMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.TryGetComponent(out IPlayerHint playerHint))
            {
                playerAction = playerHint.PlayerActionEvent;
                return true;
            }
            
            
        }

        
        return false;
    }

    

}