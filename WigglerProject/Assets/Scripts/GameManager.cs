using UnityEngine;

public delegate void GameEvent();
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int points;
    public GameEvent FirstFlowerGrownEvent;
    

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


    public void AddPoints(int amount)
    {
        points+= amount;
    }

    public void ActivateFirstFlower()
    {
        FirstFlowerGrownEvent?.Invoke();
    }
}