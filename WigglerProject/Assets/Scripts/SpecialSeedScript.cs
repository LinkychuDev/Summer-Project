using UnityEngine;
using UnityEngine.Splines;

public class SpecialSeedScript : MonoBehaviour, IWettable
{
    
    public bool isWet;

    public Material wetMaterial;
    public float currentWaterAmount;
    public float waterThreshold = 1;
    private Renderer rend;
    public SeedHoleScript HoleScript;
    private bool planted;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    public void OnWet(float wetAmount)
    {
        if(planted)
            return;
        currentWaterAmount += wetAmount;

        if (currentWaterAmount > waterThreshold)
        {
            isWet = true;
            planted = true;
            rend.material = wetMaterial;
            HoleScript.SpawnPlant();
        }
    }
}
