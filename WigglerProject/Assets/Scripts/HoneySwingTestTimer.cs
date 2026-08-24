using DG.Tweening;
using UnityEngine;

public class HoneySwingTestTimer : HoneySwingTest
{
    public Transform targetPosition;
    //public Transform resetPosition;
    public float timer = 1f;
    
    public void Activate()
    {
        transform.DOMove(targetPosition.position, timer);
    }
    
    public void Deactivate()
    {
        transform.DOMove(originPosition, timer);
    }
}
