using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class TailSpringVis : MonoBehaviour
{
    
    [SerializeField] private SplineContainer _splineContainer;
    private Spline _spline;
    List<Vector3> positions = new List<Vector3>();
    public int segmentCount = 8;
    private void Awake()
    {
        _spline = _splineContainer.Spline;
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
