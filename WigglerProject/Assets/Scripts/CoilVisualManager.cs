using System.Collections.Generic;
using UnityEngine;

public class CoilVisualManager : MonoBehaviour
{
   public float minDistance;
   public float stretchDistance;
   public float rotationSpeed;

   public int size;
   private float distance;

   public Transform head;
   public Transform body;

   List<Vector3>  positions = new List<Vector3>();


   void Start()
   {
      for (int i = 0; i < positions.Count; i++)
      {
         positions.Add(Vector3.Lerp(body.position, head.position, i / (float)size));
      }
   }

   void Update()
   {
      for (int i = 1; i < positions.Count; i++)
      {
         Vector3 currentPosition = positions[i];
         Vector3 previousPosition = positions[i - 1];
         
         float dist = Vector3.Distance(previousPosition, currentPosition);
         
         
      }
   }
}