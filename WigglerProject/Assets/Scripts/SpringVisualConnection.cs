using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public struct SpringSegment
{
    public Vector3 currentPosition;
    public Vector3 lastPosition;
    
    //public Quaternion currentRotation;

    public SpringSegment(Vector3 pos)
    {
        currentPosition = pos;
        lastPosition = pos;
        //currentRotation = Quaternion.LookRotation(pos);
    }
}
public class SpringVisualConnection : MonoBehaviour
{
    public List<Transform> springSegments = new List<Transform>();

    public int segments;
    //public float distancePerSegment;
    
    //private float currentDistancePerSegment;
   // public List<CoilNode> nodes = new List<CoilNode>();

    public Transform headTransform;
    public Transform bodyTransform;
    public Transform playerManager;
    public GameObject pointPrefab;


    [SerializeField] private PlayerController controller;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] private float damper;
    [SerializeField] private float springForce;
    
    [SerializeField] Rigidbody headRigidbody;

    [SerializeField] private Vector3 gravity;
    [SerializeField] private float dampingRatio;
    [SerializeField] private int numberOfConstraints;
    
    //Vector3 startPosition;
    [SerializeField] private float blendFactor = 0.5f;

    private float distancePerSegment;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        for (int i = 0; i < segments; i++)
        {
            var point = new GameObject("springPoint " + i)
            {
                transform =
                {
                    position = Vector3.Lerp(bodyTransform.position, headTransform.position, (float)i / segments),
                    rotation = Quaternion.LookRotation(bodyTransform.forward, Vector3.up)
                }
            };
            springSegments.Add(point.transform);
        }
    }


    private void Update()
    {
        UpdatePositions();
//UpdateRotations();
    }


    void UpdatePositions()
    {
        // Pin first and last segments
        springSegments[0].position = bodyTransform.position;
        springSegments[^1].position = headTransform.position;

        float segmentLength = Vector3.Distance(bodyTransform.position, headTransform.position) / segments;
        
        for (int i = segments - 2; i > 0; i--)
        {
            Vector3 nextPos = springSegments[i + 1].position;

            // Correct direction: from next → current
            Vector3 dir = (springSegments[i].position - nextPos).normalized;

            if (dir == Vector3.zero)
                dir = (bodyTransform.position - nextPos).normalized;

            springSegments[i].position = nextPos + dir * segmentLength;
        }
    }

    void UpdateRotations()
    {
        springSegments[0].rotation = bodyTransform.rotation;
        Vector3 direction = headTransform.forward;
        for (int i = segments -1; i > 0; i--)
        {
            Vector3 currentPosition = springSegments[i].position;
            Vector3 lastPosition = springSegments[i -1].position;
            
            Vector3 targetDir = (currentPosition - lastPosition).normalized;
            
            direction = Vector3.Slerp(direction, targetDir, blendFactor);
            
            Quaternion rotation = Quaternion.LookRotation(direction);
            springSegments[i].rotation = rotation;
            
            //springSegments[i - 1].position = Vector3.Slerp(lastPosition, targetPosition, blendFactor);

        }
    }


    private void OnDrawGizmos()
    {
        if(springSegments.Count == 0)
            return;
        
        
        
        for (int i = 0; i < segments; i++)
        {
            Gizmos.color = Color.aquamarine;
            Gizmos.DrawWireSphere(springSegments[i].position, 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(springSegments[i].position, springSegments[i].position - springSegments[i].forward);
        }
    }
}
