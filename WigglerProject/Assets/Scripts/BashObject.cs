using UnityEngine;
using UnityEngine.Events;

public class BashObject : MonoBehaviour, IStretchBashable, IPlayerHint
{
    private bool hasBashed;
    public UnityEvent bashEvent;
    public void StretchBash()
    {
        if(hasBashed)
            return;
        bashEvent?.Invoke();
        hasBashed = true;
        
    }

    public PlayerActionEvent PlayerActionEvent { get; } = PlayerActionEvent.StretchAction;
}