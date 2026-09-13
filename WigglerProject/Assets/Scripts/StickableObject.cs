using UnityEngine;

public class StickableObject : MonoBehaviour
{
    public bool canStick = true;

    public virtual bool CanStick()
    {
        return canStick;
    }
}