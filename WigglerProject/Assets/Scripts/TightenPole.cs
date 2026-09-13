using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class TightenPole : MonoBehaviour
{
    private SplineContainer _splineContainer;
    
    [SerializeField] private float pointRadius = 0.25f;
    [SerializeField] private float pushForce = 0.1f;
    [SerializeField] private LayerMask collisionLayer;

    List<Vector3> originalPositions =  new List<Vector3>();
    
    List<BezierKnot>  bezierKnots = new List<BezierKnot>();

    private bool hasPlayerOn;

    private void Start()
    {
        _splineContainer = GetComponent<SplineContainer>();
        foreach (var knot in _splineContainer.Spline.Knots)
        {
            originalPositions.Add(knot.Position);
            bezierKnots.Add(knot);
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            hasPlayerOn = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            hasPlayerOn = false;
        }
    }

    private void FixedUpdate()
    {
        if(!hasPlayerOn)
            return;
        for (int i = 0; i < bezierKnots.Count; i++)
        {
            if(i == 0)
                continue;
            if(i == bezierKnots.Count - 1)
                continue;
            
            var bezierKnot = bezierKnots[i];
            if (Physics.CheckSphere(_splineContainer.transform.TransformPoint(bezierKnot.Position), pointRadius,
                    collisionLayer))
            {
                var newPos = _splineContainer.transform.TransformPoint(originalPositions[i] + (Vector3.up *  Time.deltaTime));
                bezierKnot.Position = _splineContainer.transform.InverseTransformPoint(newPos);
            }

            else
            {
                bezierKnot.Position = originalPositions[i];
            }
            
            bezierKnots[i]  = bezierKnot;
        }
    }
}
