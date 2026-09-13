using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GrassPrefabLibrary", menuName = "4ydam/A+ Grass/Grass Prefab Library")]
public sealed class GrassPrefabLibrary : ScriptableObject
{
    public List<GameObject> prefabs = new List<GameObject>();
}
