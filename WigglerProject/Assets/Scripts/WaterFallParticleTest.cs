using System;
using System.Collections.Generic;
using UnityEngine;

public interface IWettable
{
    public void OnWet(float wetAmount);
}
public class WaterFallParticleTest : MonoBehaviour
{
    ParticleSystem particles;
    
    public float wetAmount;
   
    List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }

    private void OnParticleTrigger()
    {
        int numEnter = particles.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter, out var colliderDataEnter);
        

        for (int i = 0; i < numEnter; i++)
        {
            var col = colliderDataEnter.GetCollider(i, 0);
            col.transform.TryGetComponent(out IWettable wettable);
            wettable.OnWet(wetAmount);
        }
        
        
        
        
    }
    
    
}