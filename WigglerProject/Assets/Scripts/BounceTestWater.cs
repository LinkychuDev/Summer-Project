using System;
using UnityEngine;

public class BounceTestWater : BounceTest, IWettable
{
    
    public float waterThreshold = 20;
    [SerializeField]private float currentWaterAmount;
    [SerializeField] private WaterFallParticleTest waterTest;
    public bool shouldCheckForDryness;

   
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

        currentWaterAmount += wetAmount;
        currentWaterAmount = Mathf.Clamp(currentWaterAmount, 0, waterThreshold);
       

        
    }

    private void Update()
    {
        if(!shouldCheckForDryness)
            return;
        if (waterTest != null)
        {
            if(!HasEnoughWater())
                return;
            if (!waterTest.gameObject.activeSelf)
            {
                Dry(50);
            }
        }
    }


    public void Dry(float dryAmount)
    {
        currentWaterAmount -= dryAmount;
        currentWaterAmount = Mathf.Clamp(currentWaterAmount, 0, waterThreshold);
    }

    public virtual void OnWetEvent()
    {
        Setup();
    }
}