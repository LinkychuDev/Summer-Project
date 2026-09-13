using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SpecialMovingPlatform : MovingPlatform
{
        public SplineContainer updatedSpline;

        public void UpdatePath()
        {
            waypoints = new List<Vector3>();
            foreach (var knot in updatedSpline.Spline.Knots)
            { 
                waypoints.Add(updatedSpline.transform.TransformPoint(knot.Position));
            }
            waypointIndex = waypoints.Count - 2;
            currentWaypointPos = waypoints[waypointIndex];
          

          
        }
}