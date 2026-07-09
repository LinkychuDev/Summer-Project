using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public struct CoilNode
{
    public Vector3 currentPosition;
    public Vector3 lastPosition;
    public Quaternion lastRotation;
    public Quaternion currentRotation;


    public CoilNode(Vector3 currentPosition)
    {
        this.currentPosition = currentPosition;
        lastPosition = currentPosition;
        lastRotation = Quaternion.identity;
        currentRotation =   Quaternion.identity;
    }
    
}
public class SpringVisualConnection : MonoBehaviour
{
    public List<Transform> points = new List<Transform>();

    public int segments;
    public float distancePerSegment;
    
    private int currentDistancePerSegment;
   // public List<CoilNode> nodes = new List<CoilNode>();

    public Transform headTransform;
    public Transform bodyTransform;
    public Transform playerManager;
    public GameObject pointPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < segments; i++)
        {
            var pos =Vector3.Lerp(bodyTransform.position, headTransform.position, i / (float)segments);
            var p = Instantiate(pointPrefab, pos, Quaternion.identity);
            p.name = "Point " + i;
            points.Add(p.transform);
            p.transform.SetParent(playerManager);
            p.transform.forward =  headTransform.forward;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSegments();
    }


    void UpdateSegments()
    {
        /*points[0].position = bodyTransform.position;
        points[^1].position = headTransform.position;*/
        
        
        

        for (int i = 0; i < segments; i++)
        {
            Vector3 pos = Vector3.Lerp(bodyTransform.position, headTransform.position, (i)/ (float)segments);
            points[i].position = pos;
        }
        
        points[0].rotation = bodyTransform.rotation;
        points[^1].rotation = headTransform.rotation;
        
        for (int i = segments - 1; i > 0; i--)
        {
         
           
           var prevPos = points[i - 1].position;
           var currentPos = points[i].position;
           var currentRot = points[i].rotation;
           
           
           
           points[i - 1].LookAt(currentPos);
           var direction = (currentPos - prevPos).normalized;
           
           /*var distance = Vector3.Distance(prevPos, currentPos);
           
           Quaternion desiredRot = Quaternion.LookRotation(direction);
           
           Quaternion targetRotation = Quaternion.Slerp(currentRot, desiredRot, 0.5f);
           
           points[i].rotation = targetRotation;*/
           
        }
    }
}
