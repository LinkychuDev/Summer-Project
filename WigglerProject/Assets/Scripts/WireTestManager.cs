using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;


public class WireTestManager : MonoBehaviour
{
    public LineRenderer HeadandBody;

    public LineRenderer BodyandTail;

    [SerializeField] private Transform headSegment;
    [SerializeField] private Transform bodySegment;
    [SerializeField]  private Transform tailSegment;

    public int segments;
    public float width = 0.6f;

    public int cornerVertices = 5;
    int segmentCount;
    
    PlayerReferenceManager controller;

    
    private List<GameObject> objectsBodyToHead;
    private List<GameObject> objectsTailToBody;

    public SplineContainer container;
    private Spline spline;

    private BezierKnot headPositionKnot;
    private BezierKnot bodyPositionKnot;

    [SerializeField] private float blendValue = 2;

    private BezierKnot[] bezierKnots;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spline = container.Spline;
        controller = GameObject.FindFirstObjectByType<PlayerReferenceManager>();
        
        //Create spline

        //UpdateSegmentsHead();

    }


    private void FixedUpdate()
    {
        controller.UpdateCachedHeadPositions(headSegment.position, headSegment.transform.forward, Vector3.forward);
        UpdateSegmentsHead();
    }

    void UpdateSegmentsHead()
    {
        spline.Clear();
        spline.Add(new BezierKnot(container.transform.InverseTransformPoint(bodySegment.position)));
        spline.Add(new BezierKnot(container.transform.InverseTransformPoint(controller.cachedHeadMovementPositions[0].lastPosition)));
        
       for (int i = 0; i < controller.cachedHeadMovementPositions.Count ; i++)
       {
         
           /*Vector3 position = controller.cachedHeadPositions[i].lastPosition;
           Quaternion lastRotation = controller.cachedHeadPositions[i].lastRotation;
           
           
           position = container.transform.InverseTransformPoint(position);
           
           lastRotation = Quaternion.Inverse(container.transform.rotation) * lastRotation;;
          ;

          var knot = new BezierKnot(position)
          {
              Rotation = Quaternion.Inverse(lastRotation)
          };
          */

         // spline.Add(knot);
          
       

       }
       
      
       
       
        
       // spline.SetTangentMode(0, TangentMode.Mirrored, BezierTangent.Out);
       // spline.SetTangentMode(1, TangentMode.Mirrored, BezierTangent.In);
    }
    // Update is called once per frame
    void Update()
    {
        /*Vector3 startHeadPosition = headSegment.position;
        Vector3 endHeadPosition = bodySegment.position;
        
        Vector3 startTailPosition = tailSegment.position + tailSegment.forward;
        Vector3 endTailPosition = bodySegment.position - tailSegment.forward;
        for (int i = 0; i < segmentCount; i++)
        {
            HeadandBody.SetPosition(i, Vector3.Lerp(headSegment.position - headSegment.forward, bodySegment.position + bodySegment.forward, i / (float)segmentCount));
            BodyandTail.SetPosition(i, Vector3.Lerp(tailSegment.position + tailSegment.forward, bodySegment.position + bodySegment.forward, i / (float)segmentCount));
        }*/
        
        UpdateSegmentsHead();
    }

    private void OnDrawGizmos()
    {
        if(spline == null)
            return;
        if(spline.Count == 0)
            return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(bodyPositionKnot.Position, 0.5f);
        Gizmos.color = Color.aquamarine;
        Gizmos.DrawWireSphere(headPositionKnot.Position, 0.5f);
    }
}
