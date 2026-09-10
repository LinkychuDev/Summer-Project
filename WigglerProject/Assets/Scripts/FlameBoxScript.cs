using System;
using UnityEngine;

public class FlameBoxScript : KillPlaneTest, IWettable
{
    public float currentWetAmount;
    public float desiredWetAmount = 1;
    private bool isWet;
    public bool canBeWet = false;



    public void MakeWettable(bool value)
    {
        canBeWet = value;
    }
    
    
    
    public void OnWet(float wetAmount)
    {
        if(isWet)
            return;
        if(!canBeWet)
            return;
        currentWetAmount += wetAmount;

        if (currentWetAmount > desiredWetAmount)
        {
            isWet = true;
            PutOutFire();
        }
    }


    public void PutOutFire()
    {
        if(!canBeWet)
            return;
        Destroy(gameObject, 0);
    }
}
