using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public enum GameFlags
{
    FirstFlowerGrown,
    HasUsedFirstStretch,
    HasUsedFirstWallClimb,
    HasUsedFirstPulling,
    HasUsedFirstSticking,
    CollectedFirstBerries,
    CollectedSecondBerries,
    CollectedThirdBerries
    
}
public class GameEventSystem
{
    public Action<GameFlags> OnEventCompleted;
    private HashSet<GameFlags> CompletedFlags = new HashSet<GameFlags>();

    public void CompletedEvent(GameFlags flags)
    {
        if (CompletedFlags.Add(flags))
        {
            OnEventCompleted?.Invoke(flags);
        }
    }
    
    public bool IsEventCompleted(GameFlags flags)
    {
        return CompletedFlags.Contains(flags);
    }
}
[System.Serializable]
public class GiantBerryData
{
    public int id;
    public bool isCollected;

    public GiantBerryData(int id)
    {
        this.id = id;
        isCollected = false;
    }
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

   
   
    public int totalAmountOfBerries;
    public List<GameFlags> Achievements = new List<GameFlags>();
    public Color berryColor;
    public float inGameTime;
    
    
    
    public SerializedDictionary<string, List<GiantBerryData>> GiantBerriesDict = new SerializedDictionary<string, List<GiantBerryData>>();
    public SerializedDictionary<string, int> CollectedOrbs = new SerializedDictionary<string, int>();
    public int orbCount;
  
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

    private void OnEnable()
    {
    }

    public void LevelBoot()
    {
        ShowBounds();
        ShowFPS();
        StartCoroutine(SpawnPlayer());
        AddPoints(0);
       
    }





    public void CollectOrb(string id)
    {
        CollectedOrbs.Add(id, 1);
        orbCount++;
    }
    public void SaveGiantBerryData(string levelName, int berryId)
    {

        
        if (GiantBerriesDict.TryGetValue(levelName, out var value))
        {
            if(value[berryId].isCollected)
                return;
            value[berryId].isCollected = true;
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

    IEnumerator SpawnPlayer()
    {

        if (!LevelDefiner.instance.debuggingMode)
        {
            PlayerReferenceManager.instance.headSegment.isKinematic = true;
            PlayerReferenceManager.instance.bodySegment.isKinematic = true;
            PlayerReferenceManager.instance.tailSegment.isKinematic = true;

            Debug.Log("lastSpawnPositionCount" + lastSpawnPosition.Length);
            Debug.Log("Player segment Count" + PlayerReferenceManager.instance.segments.Count);

            lastSpawnPosition = new Vector3[PlayerReferenceManager.instance.segments.Count];

            var spawnPoint = LevelDefiner.instance.spawnPoint;
            
            Debug.Log("LevelDefiner.instance:  " + LevelDefiner.instance.spawnPoint.position);







            lastSpawnPosition[0] = spawnPoint.transform.position;
            lastSpawnPosition[1] = spawnPoint.transform.position -
                                   Vector3.forward * PlayerReferenceManager.instance.bodyOffset;
            lastSpawnPosition[2] = spawnPoint.transform.position - Vector3.forward *
                (PlayerReferenceManager.instance.tailOffset + PlayerReferenceManager.instance.bodyOffset);

            PlayerReferenceManager.instance.transform.position = lastSpawnPosition[0];

            PlayerReferenceManager.instance.headSegment.linearVelocity = Vector3.zero;
            PlayerReferenceManager.instance.headSegment.angularVelocity = Vector3.zero;


            PlayerReferenceManager.instance.headSegment.transform.rotation = Quaternion.identity;
            PlayerReferenceManager.instance.bodySegment.transform.rotation = Quaternion.identity;
            PlayerReferenceManager.instance.tailSegment.transform.rotation = Quaternion.identity;

            PlayerReferenceManager.instance.bodySegment.linearVelocity = Vector3.zero;
            PlayerReferenceManager.instance.bodySegment.angularVelocity = Vector3.zero;

            PlayerReferenceManager.instance.tailSegment.linearVelocity = Vector3.zero;
            PlayerReferenceManager.instance.tailSegment.angularVelocity = Vector3.zero;



            /*
            if (PlayerReferenceManager.instance.currentState == PlayerState.Stretching)
            {
                if (PlayerReferenceManager.instance.playerStretch.stretchState == PlayerStretch.StretchState.Stretching ||
                    PlayerReferenceManager.instance.playerStretch.stretchState == PlayerStretch.StretchState.Stuck)
                {
                    PlayerReferenceManager.instance.playerStretch.StartCoroutine(PlayerReferenceManager.instance
                        .playerStretch.OnPlayerRetractedEvent());
                }
            }
            */


            PlayerReferenceManager.instance.SetState(PlayerState.Locomotion);

            yield return new WaitForFixedUpdate();
            PlayerReferenceManager.instance.headSegment.transform.position = lastSpawnPosition[0];
            PlayerReferenceManager.instance.bodySegment.transform.position = lastSpawnPosition[1];
            PlayerReferenceManager.instance.tailSegment.transform.position = lastSpawnPosition[2];
            yield return null;
        }

        PlayerReferenceManager.instance.headSegment.gameObject.SetActive(true);
        PlayerReferenceManager.instance.bodySegment.gameObject.SetActive(true);
        PlayerReferenceManager.instance.tailSegment.gameObject.SetActive(true);
        yield return null;
        PlayerReferenceManager.instance.headSegment.isKinematic = false;
        PlayerReferenceManager.instance.bodySegment.isKinematic = false;
        PlayerReferenceManager.instance.tailSegment.isKinematic = false;


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

    public void StartSpecialCutscene()
    {
        PlayerReferenceManager.instance.headSegment.gameObject.SetActive(false);
        PlayerReferenceManager.instance.bodySegment.gameObject.SetActive(false);
        PlayerReferenceManager.instance.tailSegment.gameObject.SetActive(false);
    }
}