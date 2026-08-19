using System;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    private DoorCanvasScript doorCanvas;
    public DoorLevelTrigger doorLevelTrigger;
    public LevelInfoSO levelInfo;
    private bool canActivate;

    void Start()
    {
        doorCanvas = FindFirstObjectByType<DoorCanvasScript>();
        if (doorLevelTrigger == null)
        {
            doorLevelTrigger = GetComponentInChildren<DoorLevelTrigger>();
        }
        
        
        canActivate = GameManager.instance.orbCount >= levelInfo.OrbRequirement;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
           
            
            
            doorLevelTrigger.LoadLevelInBank(levelInfo.SceneName, canActivate);
            doorCanvas?.Display(levelInfo, canActivate);
           
         
           // doorCanvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            doorLevelTrigger.ClearLevelBlank();
            doorCanvas?.Clear();
            //doorCanvas.gameObject.SetActive(false);
        }
    }
}
