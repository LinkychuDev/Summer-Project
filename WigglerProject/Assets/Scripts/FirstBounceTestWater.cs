using UnityEngine;

public class FirstBounceTestWater : BounceTestWater
{
    
    public override void OnWetEvent()
    {
        Setup();
        GameManager.instance.ActivateGameEvent(GameFlags.FirstFlowerGrown);
    }
}