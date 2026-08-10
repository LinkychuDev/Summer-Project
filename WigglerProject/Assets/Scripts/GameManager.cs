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
        if (playerReferenceManager == null)
        {
            if (PlayerReferenceManager.instance == null)
            {
                playerReferenceManager = FindFirstObjectByType<PlayerReferenceManager>();
            }
            else
            {
                playerReferenceManager = PlayerReferenceManager.instance;
            }
            
            
        }
        ShowBounds();
        ShowFPS();
        SpawnPlayer();
    }

    private void OnValidate()
    {
        Start();
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
        for(int i = 0; i < 3; i++)
        {
            lastSpawnPosition[i] = playerReferenceManager.segments[i].rb.position;
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
            playerReferenceManager.segments[i].rb.position = lastSpawnPosition[i];
        }
    }

    bool IsOutOfBounds()
    {
        Vector3 p = playerReferenceManager.headSegment.position;

        if (p.x < xBounds.x || p.x > xBounds.y)
            return true;

        if (p.y < yBounds.x || p.y > yBounds.y)
            return true;

        if (p.z < zBounds.x || p.z > zBounds.y)
            return true;

        return false;
    }
}