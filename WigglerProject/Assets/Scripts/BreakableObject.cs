using UnityEngine;

public class BreakableObject : MonoBehaviour, IBreakable
{
    public void Break()
    {
        Destroy(gameObject);
    }
}
