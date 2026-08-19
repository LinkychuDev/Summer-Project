using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorLevelTrigger : MonoBehaviour
{
    
    private string sceneName;
    private bool canTraverse;
    private Collider col;

    void Start()
    {
        col = GetComponent<Collider>();
    }
    public void LoadLevelInBank(string sceneName, bool canTraverse)
    {
        this.sceneName = sceneName;
        this.canTraverse = canTraverse;
        col.isTrigger = canTraverse;
        
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
            SceneManager.LoadScene(sceneName);
        }
    }
}
