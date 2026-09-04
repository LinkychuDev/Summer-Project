using System;
using UnityEngine;
using UnityEngine.UI;

public class OrbPartHud : MonoBehaviour
{
    
    public Button[] orbs;


    private void Start()
    {
        foreach (Button orb in orbs)
        {
            orb.interactable = false;
        }
    }

    public void ActivateOrb(int id)
    {
        orbs[id].interactable = true;
    }
}
