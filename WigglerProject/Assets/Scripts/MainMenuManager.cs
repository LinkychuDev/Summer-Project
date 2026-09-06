using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OpenGame()
    {
        SceneManager.LoadScene("HubWorld");
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}