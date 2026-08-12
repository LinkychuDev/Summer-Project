using System;
using TMPro;
using UnityEngine;

public delegate void GameEvent();
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int points;
    public GameEvent FirstFlowerGrownEvent;

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
    }

    public void ActivateFirstFlower()
    {
        FirstFlowerGrownEvent?.Invoke();
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