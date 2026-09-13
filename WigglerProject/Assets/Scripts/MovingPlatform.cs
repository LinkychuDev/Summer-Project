using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class MovingPlatform : MonoBehaviour
{

    [SerializeField] private float moveSpeed;
    public int waypointIndex;

    private Rigidbody rb;

    [SerializeField] private SplineContainer splineContainer;

    public Vector3 currentWaypointPos;

    private bool isWaiting;

    public List<Vector3> waypoints;
    
    [SerializeField] private float waitTime;


    public MovingPlatformTrigger trigger;


    public bool shouldFreezeWhenStretching = true;
    public bool inverseDirection;

    public bool freezePlayer;

    public List<Rigidbody> overlappingBodies = new List<Rigidbody>();
    private void OnValidate()
    {
        waypoints = new List<Vector3>();
        foreach (var knot in splineContainer.Spline.Knots)
        {
            waypoints.Add(splineContainer.transform.TransformPoint(knot.Position));
        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;
        rb.useGravity = false;
        rb.isKinematic = true;
        waypoints = new List<Vector3>();
        foreach (var knot in splineContainer.Spline.Knots)
        {
            waypoints.Add(splineContainer.transform.TransformPoint(knot.Position));
        }

        currentWaypointPos = waypoints[0];


        StartCoroutine(ShouldFreeze());
    }


    IEnumerator ShouldFreeze()
    {
        while (shouldFreezeWhenStretching)
        {
            if (trigger.HasPlayer)
            {
                if (PlayerReferenceManager.instance.playerStretch.stretchState != PlayerStretch.StretchState.None)
                {
                    freezePlayer = true;
                }

                else
                {
                    freezePlayer = false;
                }
            }

            else
            {
                freezePlayer = false;
            }

            yield return null;
        }
    }


    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (freezePlayer && shouldFreezeWhenStretching)
            return;
        MovePlatform();
    }

    void MovePlatform()
    {
        if (isWaiting)
            return;
        

        Vector3 dir = currentWaypointPos - transform.position;
        Vector3 movement = dir.normalized * moveSpeed * Time.fixedDeltaTime;

        if (movement.magnitude >= dir.magnitude || movement.magnitude == 0f)
        {
            //has reached target position
            rb.transform.position = currentWaypointPos;
            UpdateWaypoint();
        }

        else
        {
            rb.transform.position += movement;
        }

        if(trigger.overlappingRigidbodies.Count == 0)
            return;
        foreach (var overlappingRigidbody in trigger.overlappingRigidbodies)
        {
            
            overlappingRigidbody?.MovePosition(overlappingRigidbody.position + movement);
        }

        overlappingBodies = trigger.overlappingRigidbodies.ToList();

    }

    void UpdateWaypoint()
    {
        
        
        if (waypointIndex == 0 && inverseDirection || waypointIndex == waypoints.Count - 1 && !inverseDirection)
        {
        
            StartCoroutine(CooldownTime());
                
        }


        if (inverseDirection)
        {
           
            waypointIndex = ((waypointIndex - 1) % waypoints.Count);
            Debug.Log("WaypointIndex2: " + waypointIndex);
        }

        else
        {
            
            waypointIndex = (waypointIndex + 1) % waypoints.Count;
            Debug.Log("WaypointIndexF: " + waypointIndex);
        }



        /*if (waypointIndex < 0)
        {
            waypointIndex = waypoints.Count - 1;
        }

        else if (waypointIndex >= waypoints.Count)
        {
            waypointIndex = 0;
        }*/
        currentWaypointPos = waypoints[waypointIndex];
        
        
    }

    IEnumerator CooldownTime()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        inverseDirection = !inverseDirection;
        isWaiting = false;
    }
}
