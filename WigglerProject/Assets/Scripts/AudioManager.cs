using System;
using MoreMountains.Tools;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance{get; private set;}
  
    
    
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    

   
}
