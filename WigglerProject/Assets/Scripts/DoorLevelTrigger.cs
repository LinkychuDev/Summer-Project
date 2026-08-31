using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorLevelTrigger : MonoBehaviour
{
    
    private string sceneName;
    private bool canTraverse;
    private Collider col;
    private MMF_LoadScene loadScene;
    public MMF_Player mmfPlayer;

    void Start()
    {
        col = GetComponent<Collider>();
        loadScene = mmfPlayer.GetFeedbackOfType<MMF_LoadScene>();
    }
    public void LoadLevelInBank(string sceneName, bool canTraverse)
    {
        this.sceneName = sceneName;
        this.canTraverse = canTraverse;
        col.isTrigger = canTraverse;
        if (canTraverse)
        {
            loadScene.DestinationSceneName  = sceneName;
        }

        else
        {
            loadScene.DestinationSceneName = "";
        }
    }

    public void ClearLevelBlank()
    {
        sceneName = "";
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            if(!canTraverse)
                return;
            mmfPlayer.PlayFeedbacks();
        }
    }
}
