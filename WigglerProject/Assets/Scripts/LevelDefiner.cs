using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelDefiner : MonoBehaviour
{
    public static LevelDefiner instance;
    public string sceneName => SceneManager.GetActiveScene().name;
    public List<GiantBerryScript>  giantBerriesList = new List<GiantBerryScript>();
    public MMF_Player berryPlayer;
    public List<Image> giantBerrySprites = new List<Image>();
    
    public Transform spawnPoint;
    public float totalAmountOfBerries;
    public GameFlags collectedFlag;

    public List<GiantBerryData>  giantBerryData = new List<GiantBerryData>();

    public bool debuggingMode;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        
        SetUpGiantBerryCount();
        GameManager.instance.LevelBoot();
    }


    private void OnValidate()
    {
        if (giantBerriesList.Count > 0)
        {
            for (int i = 0; i < giantBerriesList.Count; i++)
            {
                giantBerriesList[i].berryId = i;
            }
        }
    }

    public void SetUpGiantBerryCount()
    {
     
        for (int i = 0; i < giantBerriesList.Count; i++)
        {
            
            giantBerriesList[i].berryId = i;
            giantBerrySprites[i].color = GameManager.instance.berryColor;
            giantBerryData.Add(new GiantBerryData(i));
            
        }
       
        GameManager.instance.GiantBerriesDict.TryAdd(sceneName, giantBerryData);

        foreach (var gb in giantBerryData)
        {
            if (gb.isCollected)
            {
                giantBerriesList[gb.id].CollectEvent();
            }
        }
        totalAmountOfBerries = giantBerriesList.Count;

    }
    
    public void UpdateGiantBerryCount(int id)
    {
        if(giantBerriesList.Count == 0)
            return;
        giantBerriesList[id].isCollected = true;
        GameManager.instance.SaveGiantBerryData(sceneName, id);
        berryPlayer.GetFeedbackOfType<MMF_Image>().BoundImage = giantBerrySprites[id];
        berryPlayer.PlayFeedbacks();
        
        
        if (giantBerriesList.FindAll(x => x.isCollected).Count >= totalAmountOfBerries)
        {
            GameManager.instance.ActivateGameEvent(collectedFlag);
        }
    }
    
}