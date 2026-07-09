using System;
using System.Collections.Generic;
using UnityEngine;

public class ChainVerlet : MonoBehaviour
{
    public struct VerletNode
    {
        public Vector3 currentPosition;
        public Vector3 lastPosition;

        public VerletNode(Vector3 pos)
        {
            currentPosition = pos;
            lastPosition = pos;
        }
    }

    [Header("Verlet Node")] 
    [SerializeField] public int numberOfSegments;
    [SerializeField] private float segmentLength;
    [SerializeField] float dampingFactor;
    [SerializeField] private int constraintRuns = 50;
    [SerializeField] private float gravity = -15f;
    
    LineRenderer lineRenderer;
    public Transform startPoint;
    public Transform endPoint;
    
    private List<VerletNode> nodes = new List<VerletNode>();
    [SerializeField] private float scaleFactor = 0.5f;


    [SerializeField] private PlayerController controller;
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numberOfSegments;

        for (int i = 0; i < numberOfSegments; i++)
        {
            if (i == 0)
            {
                nodes.Add(new VerletNode(startPoint.position));
            }
            
            else if (i == numberOfSegments - 1)
            {
                nodes.Add(new VerletNode(endPoint.position));
            }

            else
            {
                nodes.Add(new VerletNode(Vector3.Lerp(startPoint.position, endPoint.position, i / (float)numberOfSegments)));
            }
        }
    }


    private void LateUpdate()
    {
        DrawChain();
    }

    
    void Update()
    { 
        CacheHeadPosition(); 
        Simulate();
        for (int i = 0; i < constraintRuns; i++)
        {
           ApplyConstraints();
        }
    }
    
    

    void DrawChain()
    {
        Vector3[] points = new Vector3[numberOfSegments];
        for (int i = 0; i < numberOfSegments; i++)
        {
            points[i] = nodes[i].currentPosition;
        }
        
        lineRenderer.SetPositions(points);
    }

    void CacheHeadPosition()
    {
        if(controller.cachedHeadPositions.Count == 0)
            return;
        int index = Mathf.Min(controller.segmentIndexSpacing, controller.cachedHeadPositions.Count - 1);
        VerletNode head = nodes[^1];
        head.currentPosition = controller.cachedHeadPositions[index].lastPosition;
        nodes[^1] = head;

    }
    
    void Simulate()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var currentNode = nodes[i];
            
            Vector3 velocity = (currentNode.currentPosition - currentNode.lastPosition) * dampingFactor;
            currentNode.lastPosition = currentNode.currentPosition;
            currentNode.currentPosition +=  velocity;
            currentNode.currentPosition += (gravity * Vector3.up) * Time.deltaTime;
            nodes[i] = currentNode;
        }
    }
    
    void ApplyConstraints()
    {
        VerletNode firstNode = nodes[0];
        VerletNode lastNode = nodes[numberOfSegments - 1];
        
        firstNode.currentPosition = startPoint.position;
        lastNode.currentPosition = endPoint.position;
        
        nodes[0] = firstNode;
        nodes[numberOfSegments - 1] = lastNode;

        for (int i = 0; i < numberOfSegments - 1; i++)
        {
            VerletNode currentNode = nodes[i];
            VerletNode nextNode = nodes[i + 1];
            
            float distance = (currentNode.currentPosition - nextNode.currentPosition).magnitude;
            float difference = distance - segmentLength;
            
            
            Vector3 direction = (currentNode.currentPosition - nextNode.currentPosition).normalized;
            
            Vector3 changeVector = direction * difference;

            if (i != 0)
            {
                currentNode.currentPosition -= changeVector * scaleFactor;
                
                nextNode.currentPosition += changeVector * scaleFactor;
            }

            else
            {
                nextNode.currentPosition += changeVector;
            }
            
            nodes[i] = currentNode;

            if ((i + 1) != numberOfSegments - 1)
            {
                nodes[i + 1] = nextNode;
            }
          
        }
    }
}
