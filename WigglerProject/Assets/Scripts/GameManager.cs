using System;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum GameFlags
{
    FirstFlowerGrown,
    HasUsedFirstStretch,
    HasUsedFirstWallClimb,
    HasUsedFirstPulling,
    HasUsedFirstSticking,
    CollectedFirstBerries
    
}
public class GameEventSystem
{
    public Action<GameFlags> OnEventCompleted;
    private HashSet<GameFlags> CompletedFlags = new HashSet<GameFlags>();

    public void CompletedEvent(GameFlags flags)
    {
        CompletedFlags.Add(flags);
        OnEventCompleted?.Invoke(flags);
    }
    
    public bool IsEventCompleted(GameFlags flags)
    {
        return CompletedFlags.Contains(flags);
    }
}

[Serializable]
public class GiantBerries
{
    public Image sprite;
    public GiantBerryScript  berryScript;
    public bool isCollected;
    
}


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int points;
    public static GameEventSystem GameEvents = new GameEventSystem();

    public GameObject honeyDecal;
    public bool showFPS;
    public TextMeshProUGUI fpsText;

    public bool shouldCapFps;
    public int fpsCapFps = 120;

    public Vector3[] lastSpawnPosition;
    
    public static Action<Vector3> CameraClimbSwitch;
    public bool BoundaryBoxes;
    PlayerReferenceManager playerReferenceManager;

    private Rigidbody headRb;
    public Vector2 xBounds = new Vector2(-1000, 1000), yBounds = new Vector2(-1000, 1000), zBounds = new Vector2(-1000, 1000);
    public TextMeshProUGUI berryText;
    public MMF_Player textPlayer;

    public MMF_Player berryPlayer;
    public List<GiantBerries>  giantBerriesList = new List<GiantBerries>();
    public Dictionary< int, GiantBerries> GiantBerriesDict = new Dictionary<int, GiantBerries>();
    public int totalAmountOfBerries;
    public int berriesCollected;
    public List<GameFlags> Achievements = new List<GameFlags>();
    public Color berryColor;
    public float inGameTime;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
        }

        else
        {
            Destroy(gameObject);
        }
        
    }


  

 
    
    
    

    private void Start()
    {
        ShowBounds();
        ShowFPS();
        SpawnPlayer();
        AddPoints(0);
        SetUpGiantBerryCount();
    }
    
    


    void SetUpGiantBerryCount()
    {
        for (int i = 0; i < giantBerriesList.Count; i++)
        {
            giantBerriesList[i].berryScript.berryId = i;
            GiantBerriesDict.Add(i, giantBerriesList[i]);
            giantBerriesList[i].sprite.color = berryColor;
        }
       
        totalAmountOfBerries = giantBerriesList.Count;
        berriesCollected = 0;

    }

    public void UpdateGiantBerryCount(int id)
    {

        if (GiantBerriesDict.TryGetValue(id, out var value))
        {
            if(value.isCollected)
                return;
            value.isCollected = true;
            berriesCollected++;
            berryPlayer.GetFeedbackOfType<MMF_Image>().BoundImage = value.sprite;
            berryPlayer.PlayFeedbacks();
        }
        
        
        if (berriesCollected == totalAmountOfBerries)
        {
            ActivateGameEvent(GameFlags.CollectedFirstBerries);
        }
    }

    private void OnValidate()
    {
        ShowBounds();
    }


    void ShowBounds()
    {
        #if UNITY_EDITOR
        foreach (var cb in FindObjectsByType<CollisionBlock>(FindObjectsSortMode.None))
        {
            cb.ShowBounds(BoundaryBoxes);
        }
        
        #endif
    }

    void ShowFPS()
    {
        if (showFPS)
        {
            fpsText.gameObject.SetActive(showFPS);
        }

        if (shouldCapFps)
        {
            Application.targetFrameRate = fpsCapFps;
        }
    }

    public void AddPoints(int amount)
    {
        points+= amount;
        berryText.text = "x" + points.ToString();
        textPlayer.PlayFeedbacks();
    }

  

    public void ActivateGameEvent(GameFlags flags)
    {
        GameEvents.CompletedEvent(flags);
        if (!Achievements.Contains(flags))
        {
            Achievements.Add(flags);
        }
    }
    private void Update()
    {
        if (showFPS)
        {
            fpsText.text = "Current FPS: " + Mathf.RoundToInt(1f / Time.smoothDeltaTime).ToString();
        }


        if (IsOutOfBounds())
        {
            ResetPlayerPosition();
        }
        
        
        inGameTime += Time.deltaTime;
    }

    void SpawnPlayer()
    {
        Debug.Log("lastSpawnPositionCount" + lastSpawnPosition.Length);
        Debug.Log("Player segment Count" + PlayerReferenceManager.instance.segments.Count );
        
        lastSpawnPosition = new Vector3[PlayerReferenceManager.instance.segments.Count];
        
        for(int i = 0; i < PlayerReferenceManager.instance.segments.Count; i++)
        {
            lastSpawnPosition[i] = PlayerReferenceManager.instance.segments[i].rb.position;
        }
    }

    public void SavePlayerPosition(Vector3[] positions)
    {
        lastSpawnPosition = positions;
    }

    public void ResetPlayerPosition()
    {
        for (int i = 0; i < 3; i++)
        {
            PlayerReferenceManager.instance.segments[i].rb.position = lastSpawnPosition[i];
        }
    }

    bool IsOutOfBounds()
    {
        Vector3 p = PlayerReferenceManager.instance.headSegment.position;

        if (p.x < xBounds.x || p.x > xBounds.y)
            return true;

        if (p.y < yBounds.x || p.y > yBounds.y)
            return true;

        if (p.z < zBounds.x || p.z > zBounds.y)
            return true;

        return false;
    }
}