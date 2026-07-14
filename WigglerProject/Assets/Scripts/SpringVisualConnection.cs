using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.Splines;



public struct SpringNode
{
    public Vector3 currentPos;
    public Vector3 lastPos;

    public SpringNode(Vector3 pos)
    {
        currentPos = pos;
        lastPos = pos;
    }
}

public class SpringVisualConnection : MonoBehaviour
{
//public List<Transform> springSegments = new List<Transform>();

    public int segments;
    //public float distancePerSegment;
    
    //private float currentDistancePerSegment;
   // public List<CoilNode> nodes = new List<CoilNode>();

    public Transform headTransform;
    public Transform bodyTransform;
    public Transform playerManager;
    public GameObject pointPrefab;

    [SerializeField] private float splineInstantiateRatio = 1;

    [SerializeField] private PlayerReferenceManager controller;
    [SerializeField] private float damper;
    [SerializeField] private float springForce;
    
    [SerializeField] Rigidbody headRigidbody;

    [SerializeField] private Vector3 gravity;
    [SerializeField] private float dampingRatio;
    [SerializeField] private int numberOfConstraints;
    
    //Vector3 startPosition;
    [SerializeField] private float minSplineDistance = 0.1f;
    [SerializeField] private float blendFactor = 0.5f;

    private float distancePerSegment;

    [SerializeField] private SplineContainer splineContainer;

    private Spline _spline;

    private SplineInstantiate _splineInstantiate;

    private bool isSwinging;

    [SerializeField] private int subStretchSegments = 4;

    private bool isStretching;
    
    [SerializeField] private float minSegmentDistance;
    [SerializeField] private float minStretchSegmentDistance;
    
    
    public float baseSegmentLength = 0.2f;
    public float stretchMultiplier = 1.5f;   // how much the rope can stretch

   public List<SpringNode> springNodes = new List<SpringNode>();

   [SerializeField] private float stretchSegmentLength;
   // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        PlayerReferenceManager.OnStateChange += OnStateChange;
        _spline = splineContainer.Spline;
       _splineInstantiate = splineContainer.GetComponent<SplineInstantiate>();
       _spline.Clear();
       
       
       springNodes.Clear();
       
        for (int i = 0; i < segments; i++)
        {

            var position = Vector3.Lerp(bodyTransform.position, headTransform.position, (float)i / segments);
            /*var point = new GameObject("springPoint " + i)
            {
                transform =
                {
                   
                    rotation = Quaternion.LookRotation(bodyTransform.forward, Vector3.up)
                }
            };*/
            //springSegments.Add(point.transform);
            springNodes.Add(new SpringNode(position));
            _spline.Add(splineContainer.transform.InverseTransformPoint(position));
        }

        
    }
    
    void Start()
    {
        
    }

    private void Update()
    {
        //ApplyRotations();
        
        //ApplyRotations();
        UpdateSpline();
        
    }

    void FixedUpdate()
    {
        Simulate();
        for (int i = 0; i < numberOfConstraints; i++)
        {
            ApplyConstraints();
            //UpdateTransforms();
        }
        
       
    }

    void Simulate()
    {
        for (int i = 0; i < springNodes.Count; i++)
        {
            SpringNode node = springNodes[i];

            Vector3 velocity = (node.currentPos - node.lastPos) * dampingRatio;
            node.lastPos = node.currentPos;

            node.currentPos += velocity + gravity * (Time.deltaTime * Time.deltaTime);

            springNodes[i] = node;
        }
    }

    void ApplyConstraints()
    {
        // Pin ends
        var startNode = springNodes[0];
        var lastNode = springNodes[^1];
        startNode.currentPos = bodyTransform.position;
        lastNode.currentPos = headTransform.position;
        
        springNodes[0] = startNode;
        springNodes[^1] = lastNode;

        

        // Stretch based on head-body distance
        float totalDist = Vector3.Distance(bodyTransform.position, headTransform.position);
        //float idealTotal = baseSegmentLength * (segments - 1);


        distancePerSegment = (totalDist / segments) * stretchMultiplier;
        /*if (totalDist > idealTotal)
        {
            float stretchFactor = Mathf.Min(totalDist / idealTotal, stretchMultiplier);
            distancePerSegment *= stretchFactor;
           // distancePerSegment = Mathf.Clamp(distancePerSegment, baseSegmentLength, stretchSegmentLength);
        }*/

        // Solve constraints
        for (int i = 0; i < springNodes.Count - 1; i++)
        {
            var currentNode = springNodes[i];
            var nextNode = springNodes[i + 1];
            Vector3 dir = nextNode.currentPos - currentNode.currentPos;
            float dist = dir.magnitude;

            float error = dist - distancePerSegment;
            Vector3 correction = dir.normalized * (error * 0.5f);

            currentNode.currentPos += correction;
            nextNode.currentPos -= correction;
            
            
            //Quaternion rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            springNodes[i] = currentNode;
            springNodes[i + 1] = nextNode;
        }
    }

    void ApplyRotations()
    {
        Vector3 headDir = headTransform.forward;

        for (int i = 1; i < springNodes.Count; i++)
        {
            var prevNode = springNodes[i - 1];
            var currentNode = springNodes[i];

            // Natural Verlet direction
            Vector3 dir = (currentNode.currentPos - prevNode.currentPos).normalized;
            float distance = (currentNode.currentPos - prevNode.currentPos).magnitude;

            // Blend toward head rotation direction
            float t = (float)i / (springNodes.Count - 1);   // 0 → 1 along rope
            Vector3 blendedDir = Vector3.Slerp(dir, headDir, t * blendFactor);

            // Recompute position using blended direction
            currentNode.currentPos = prevNode.currentPos + blendedDir * distancePerSegment;
            springNodes[i] = currentNode;
        }
    }

    public Vector3[] GetPositions()
    {
        Vector3[] springArray = new Vector3[springNodes.Count];
        for (int i = 0; i < springNodes.Count; i++)
        {
            springArray[i] = springNodes[i].currentPos;
        }
        return springArray;
    }
        //Debug.Log("Distance between Start and Second Position: " +Vector3.Distance(springSegments[0].position, springSegments[1].position));

    private void OnDrawGizmos()
    {
        if(springNodes.Count == 0)
            return;
        
        
        
        for (int i = 0; i < springNodes.Count; i++)
        {
            Gizmos.color = Color.aquamarine;
                Gizmos.DrawWireSphere(springNodes[i].currentPos, 0.1f);
        }
    }
    
    
    void UpdateSpline()
    {
        
        var positions = GetPositions();
        for (int i = 0; i < positions.Length; i++)
        {
            var currentKnot = _spline[i];
           currentKnot.Position = splineContainer.transform.InverseTransformPoint(positions[i]);
           _spline[i] = currentKnot;
        }

        var startingKnot = _spline[0];
        var headKnot = _spline[^1];


        var startingPos = new Vector3(startingKnot.Position.x, startingKnot.Position.y, startingKnot.Position.z);
        var bodyDir = ( startingPos- bodyTransform.position).normalized;
        
        var finalPos =  new Vector3(headKnot.Position.x, headKnot.Position.y, headKnot.Position.z);
        var headDir = (finalPos - headTransform.position).normalized;
        startingKnot.Rotation = Quaternion.LookRotation(bodyDir, Vector3.up);
        headKnot.Rotation = Quaternion.LookRotation(headDir, Vector3.up);
        
        _spline[0] = startingKnot;
        _spline[^1] = headKnot;
        
        
        _splineInstantiate.MinSpacing = minSplineDistance;
        _splineInstantiate.MaxSpacing = minSplineDistance;
        _splineInstantiate.UpdateInstances();
    }


    private void OnStateChange(PlayerState state)
    {
        if (state == PlayerState.Swinging)
        {
            isSwinging = true;
        }

        else
        {
            isSwinging = false;
            if (state == PlayerState.Stretching)
            {
                isStretching = true;
            }

            else
            {
                isStretching = false;
            }
        }
    }
    
    
    
}