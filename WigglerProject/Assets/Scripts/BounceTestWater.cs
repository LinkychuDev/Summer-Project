using UnityEngine;

public class BounceTestWater : BounceTest, IWettable
{
    
    public float waterThreshold = 20;
    [SerializeField]private float currentWaterAmount;
    
    protected override void Setup()
    {
        if (!HasEnoughWater())
        {
            Deactivate();
        }

        else
        {
            Activate();
        }
    }
    
    bool HasEnoughWater()
    {
        
        isActive = currentWaterAmount >= waterThreshold;
        return isActive;
    }
    
    

    public void OnWet(float wetAmount)
    {
        
        if (HasEnoughWater())
        {
            OnWetEvent();
        }

        else
        {
            currentWaterAmount += wetAmount;
        }
       

        
    }

    public virtual void OnWetEvent()
    {
        Setup();
    }
}