using System;
using TMPro;
using UnityEngine;

public delegate void GameEvent();
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int points;
    public GameEvent FirstFlowerGrownEvent;


    public bool showFPS;
    public TextMeshProUGUI fpsText;

    public bool shouldCapFps;
    public int fpsCapFps = 120;
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
        ShowFPS();
    }

    private void OnValidate()
    {
        ShowFPS();
    }

    void ShowFPS()
    {
        fpsText.gameObject.SetActive(showFPS);

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
    }
}