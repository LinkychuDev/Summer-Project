using UnityEngine;

public class WireTestManager : MonoBehaviour
{
    public LineRenderer HeadandBody;

    public LineRenderer BodyandTail;

    private Transform headSegment;
    private Transform bodySegment;
    private Transform tailSegment;

    public int segments;
    public float width = 0.6f;

    public int cornerVertices = 5;
    int segmentCount;
    
    PlayerController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GameObject.FindFirstObjectByType<PlayerController>();
        segmentCount = segments + 1;
        HeadandBody.positionCount = segmentCount;
        BodyandTail.positionCount = segmentCount;
        
        HeadandBody.startWidth = width;
        BodyandTail.startWidth = width;
        HeadandBody.endWidth = width;
        BodyandTail.endWidth = width;


        HeadandBody.numCornerVertices = cornerVertices;
        BodyandTail.numCornerVertices = cornerVertices;

        headSegment = controller.segments[0].t;
        bodySegment = controller.segments[1].t;
        tailSegment = controller.segments[2].t;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < segmentCount; i++)
        {
            HeadandBody.SetPosition(i, Vector3.Lerp(headSegment.position, bodySegment.position, i / (float)segmentCount));
            BodyandTail.SetPosition(i, Vector3.Lerp(tailSegment.position, bodySegment.position, i / (float)segmentCount));
        }
    }
}
