using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int points;

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
}