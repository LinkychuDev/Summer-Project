using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    public float hoverScale = 0.1f;
    public MMF_Player hoverEffect;
    public MMF_Player selectEffect;
    public void OnHover(Transform t)
    {
        t.localScale = new Vector3(hoverScale, hoverScale, hoverScale);
        Debug.Log("Hovered");
        hoverEffect.PlayFeedbacks();
    }

    public void OnHoverExit(Transform t)
    {
        t.localScale = Vector3.one;
     
    }
    
    
    public void Select(Transform t)
    {
        selectEffect.PlayFeedbacks();
    }
    public void OpenGame()
    {
        SceneManager.LoadScene("HubWorld");
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}