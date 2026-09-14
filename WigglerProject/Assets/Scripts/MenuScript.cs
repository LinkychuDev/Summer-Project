using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuScript : MonoBehaviour
{
    EventSystem eventSystem;
    public float menuScale = 1.1f;
    public float menuTime = 0.3f;
        
    
    public GameObject displayBackground;
    public GameObject firstSelected;
    
    public virtual void Start()
    {
        eventSystem = EventSystem.current;
    }


    public void SetFirstSelected(GameObject button)
    {
        eventSystem.firstSelectedGameObject =button;
        eventSystem.SetSelectedGameObject(button);
    }
    public void OpenMenu()
    {
        //displayBackground.transform.DOPunchScale((Vector3.one * menuScale), menuTime);
        eventSystem.firstSelectedGameObject = firstSelected;
        eventSystem.SetSelectedGameObject(firstSelected);
    }

    public void CloseMenu()
    {
        eventSystem.SetSelectedGameObject(null);
       // displayBackground.transform.DOPunchScale((Vector3.one), menuTime);
    }
}
