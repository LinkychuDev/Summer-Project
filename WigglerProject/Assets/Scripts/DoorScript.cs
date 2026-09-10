using System;
using MoreMountains.Feedbacks;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    private DoorCanvasScript doorCanvas;
    public DoorLevelTrigger doorLevelTrigger;
    public LevelInfoSO levelInfo;
    private bool canActivate;

    public MMF_Player popUpSuccess;
    public MMF_Player popUpFail;

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

            if (canActivate)
            {
                popUpSuccess.PlayFeedbacks();
                popUpFail.StopFeedbacks();
            }

            else
            {
                popUpFail.PlayFeedbacks();
                popUpSuccess.StopFeedbacks();
            }
           // doorCanvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            doorLevelTrigger.ClearLevelBlank();
            popUpSuccess.StopFeedbacks();
            popUpFail.StopFeedbacks();
            doorCanvas?.Clear();
            //doorCanvas.gameObject.SetActive(false);
        }
    }
}
