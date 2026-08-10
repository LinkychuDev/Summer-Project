using UnityEngine;

public class CollisionBlock : MonoBehaviour
{
    public void ShowBounds(bool show)
    {
        GetComponent<MeshRenderer>().enabled = show;
    }
}