using System;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "Level Info", menuName = "ScriptableObjects/LevelInfoSO")]
public class LevelInfoSO : ScriptableObject
{
    public string levelName;
    public int orbCount = 1;
    public int berryCount = 1;
    public bool isCompleted = false;
    public string SceneName;


    public GameFlags berryFlag;
    public int OrbRequirement;
    
}
	