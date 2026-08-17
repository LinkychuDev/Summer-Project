using UnityEngine;

public class VineScript : MonoBehaviour, IBreakable, IPlayerHint
{
    public PlayerActionEvent PlayerActionEvent { get; } = PlayerActionEvent.StretchAction;
    public void Break()
    {
        Destroy(gameObject);
    }

   
}
