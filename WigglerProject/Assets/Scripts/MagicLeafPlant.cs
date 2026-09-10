using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class MagicLeafPlant : MonoBehaviour
{
    Rigidbody rb;
    List<Vector3> waypoints;
    public SplineContainer splineContainer;
    Vector3 currentWaypointPos;
    
    bool isWaiting = false;
    bool freezePlayer = false;
    
    public MagicLeafPlantTrigger trigger;

    public bool shouldFreezeWhenStretching = true;

    public float moveSpeed = 3f;

    public float resetTime = 3f;
    private List<Rigidbody> overlappingBodies = new List<Rigidbody>();
    public int waypointIndex = 0;
    public bool inverseDirection = false;
    public float waitTime = 3f;


    private bool canMove = false;

    private void OnEnable()
    {
        trigger.OnPlatformStepped += OnPlayerEnter;
        trigger.OnPlatformReleased += OnPlayerExit;
    }

    void OnDisable()
    {
        trigger.OnPlatformStepped -= OnPlayerEnter;
        trigger.OnPlatformReleased -= OnPlayerExit;
    }

    void OnPlayerEnter()
    {
        if(canMove)
            return;
        canMove = true;
        
    }

    void OnPlayerExit()
    {
        if(!canMove)
            return;
        canMove = false;
        StartCoroutine(ResetPeriod());
    }

    IEnumerator ResetPeriod()
    {
        yield return new WaitUntil(() => PlayerReferenceManager.instance.isGrounded);
        yield return new WaitForSeconds(resetTime);
        canMove = true;
    }
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
    }


    private void Update()
    {
        if (shouldFreezeWhenStretching)
        {
            if (trigger.overlappingRigidbodies.Contains(PlayerReferenceManager.instance.headSegment))
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
        }
    }

    private void FixedUpdate()
    {
        if(!canMove)
            return;
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
            
            
            waypointIndex = (waypointIndex - 1) % waypoints.Count;
        }

        else
        {
            
            waypointIndex = (waypointIndex + 1) % waypoints.Count;
        }
        
        
        
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