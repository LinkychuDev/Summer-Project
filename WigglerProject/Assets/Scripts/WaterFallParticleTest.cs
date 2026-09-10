using System;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Feedbacks;
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

    [SerializeField] private MMF_Player rainSound;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
        var list = GameObject.FindObjectsByType<Collider>(FindObjectsSortMode.None)
            .Where(x => x.TryGetComponent(out IWettable wettable));

        foreach (var wettable in list)
        {
            particles.trigger.AddCollider(wettable);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerMovement player))
        {
            player.OnWaterEvent();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        
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