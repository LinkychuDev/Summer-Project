using UnityEngine;
using UnityEngine.Events;

public class GrassInteractor : MonoBehaviour
{
    #region Enums

    public enum CustomMeshQuality
    {
        Low,
        Medium,
        High
    }

    #endregion

    #region Serialized Fields

    [Header("Interaction Settings")]
    [SerializeField] private bool disableWhenIdle = true;
    [SerializeField] private float pushRate = 6f;
    [SerializeField] private float maxPushStrength = 2f;
    [SerializeField] private Mesh customInteractionMesh;
    [SerializeField] private CustomMeshQuality customMeshQuality = CustomMeshQuality.Medium;

    [Header("Motion Settings")]
    [SerializeField] private float movementThreshold = 0.5f;
    [SerializeField] private float speedSmoothing = 0.3f;
    [SerializeField] private float detectionRadius = 1f;
    [SerializeField] private Transform interactionTransformOverride;
    [SerializeField] private float nonMotionInfluenceDuration = 0.35f;

    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private bool showGizmos = true;

    [SerializeField] private UnityEvent onStartedMoving = new UnityEvent();
    [SerializeField] private UnityEvent onStoppedMoving = new UnityEvent();

    private Vector3 lastPosition;
    private float currentSpeed;
    private float smoothedSpeed;
    private bool wasMoving;
    private Mesh cachedMesh;
    private Vector2[] cachedVerticesXZ;
    private int[] cachedTriangles;
    private Bounds cachedLocalBounds;
    private float cachedLocalProjectionRadius;
    private float[] cachedInfluenceMask;
    private int cachedInfluenceMaskResolution;
    private Vector2 cachedLocalMinXZ;
    private Vector2 cachedLocalSizeXZ;
    private CustomMeshQuality cachedCustomMeshQuality;
    private bool forceInfluenceOverride;
    private float nonMotionInfluenceTimer;
    private float lastDetectionRadius;
    private Vector3 lastLossyScale;
    private Quaternion lastRotation;
    private Vector3 lastPositionOffset;
    private Mesh lastCustomInteractionMesh;
    private CustomMeshQuality lastCustomMeshQuality;

    #endregion

    #region Properties

    public float PushRate => pushRate;
    public float MaxPushStrength => maxPushStrength;
    public float SpeedSmoothing => speedSmoothing;
    public float DetectionRadius => detectionRadius;
    public bool DisableWhenIdle => disableWhenIdle;
    public bool IsInfluencing => forceInfluenceOverride || IsMoving || nonMotionInfluenceTimer > 0f;
    public bool HasCustomInteractionMesh => customInteractionMesh != null;
    public CustomMeshQuality MeshQuality => customMeshQuality;
    public Transform InteractionTransformOverride
    {
        get => interactionTransformOverride;
        set => interactionTransformOverride = value;
    }

    private Transform ActiveTransform => interactionTransformOverride != null ? interactionTransformOverride : transform;

    public bool IsMoving
    {
        get
        {
            if (wasMoving)
                return smoothedSpeed > movementThreshold * 0.5f;
            else
                return smoothedSpeed > movementThreshold;
        }
    }

    #endregion

    #region Lifecycle

    private void OnEnable()
    {
        lastPosition = GetInteractionPosition();
        currentSpeed = 0f;
        smoothedSpeed = 0f;
        wasMoving = false;
        nonMotionInfluenceTimer = 0f;
        RefreshMeshCache();
        CacheInfluenceTrackingState();

        if (GrassManager.Instance != null)
            GrassManager.Instance.RegisterInteractor(this);
    }

    private void Start()
    {
        if (GrassManager.Instance == null)
            Debug.LogWarning("[GrassInteractor] No GrassManager found in scene! Add one to enable grass interaction.");
    }

    private void OnValidate()
    {
        RefreshMeshCache();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        Vector3 currentPosition = GetInteractionPosition();
        float distanceSqr = (currentPosition - lastPosition).sqrMagnitude;

        if (dt > 0.0001f)
            currentSpeed = Mathf.Min(Mathf.Sqrt(distanceSqr) / dt, 100f);

        float smoothAlpha = 1f - Mathf.Pow(Mathf.Clamp01(speedSmoothing), dt * 60f);
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, currentSpeed, smoothAlpha);

        if (HasNonMotionInfluenceChange())
            nonMotionInfluenceTimer = Mathf.Max(nonMotionInfluenceTimer, nonMotionInfluenceDuration);
        else
            nonMotionInfluenceTimer = Mathf.Max(0f, nonMotionInfluenceTimer - dt);

        bool isMoving = IsMoving;
        if (isMoving != wasMoving)
        {
            if (isMoving)
                onStartedMoving?.Invoke();
            else
                onStoppedMoving?.Invoke();
        }

        wasMoving = isMoving;
        lastPosition = currentPosition;
    }

    private void OnDisable()
    {
        if (GrassManager.Instance != null)
            GrassManager.Instance.UnregisterInteractor(this);
    }

    #endregion

    #region Public API

    public Vector3 GetInteractionPosition()
    {
        return ActiveTransform.position + positionOffset;
    }

    public void SetPositionOffset(Vector3 offset)
    {
        positionOffset = offset;
    }

    public void SetDetectionRadius(float radius)
    {
        detectionRadius = Mathf.Max(0.001f, radius);
    }

    public void SetPushRate(float rate)
    {
        pushRate = Mathf.Max(0f, rate);
    }

    public void SetMaxPushStrength(float strength)
    {
        maxPushStrength = Mathf.Max(0f, strength);
    }

    public void SetCustomInteractionMesh(Mesh mesh)
    {
        customInteractionMesh = mesh;
        RefreshMeshCache(force: true);
    }

    public void SetCustomMeshQuality(CustomMeshQuality quality)
    {
        customMeshQuality = quality;
        RefreshMeshCache(force: true);
    }

    public void SetForceInfluenceOverride(bool enabled)
    {
        forceInfluenceOverride = enabled;
    }

    public float GetInteractionExtentRadius()
    {
        if (!HasValidCustomMeshData())
            return detectionRadius;

        Bounds worldBounds = GetCustomMeshWorldBounds();
        Vector3 extents = worldBounds.extents;
        float radius = Mathf.Sqrt(extents.x * extents.x + extents.z * extents.z);

        if (radius <= 0.0001f)
            return detectionRadius;

        return radius;
    }

    public bool CustomMeshInteractsWithCell(float worldCellX, float worldCellZ, float cellHalf, Vector3 interactionCenter, float padding = 0f)
    {
        if (!HasValidCustomMeshData())
            return false;

        float rectHalf = cellHalf + Mathf.Max(0f, padding);
        float sampleY = interactionCenter.y;
        int sampleRadius = GetSampleRadiusForQuality();
        for (int z = -sampleRadius; z <= sampleRadius; z++)
        {
            for (int x = -sampleRadius; x <= sampleRadius; x++)
            {
                float sampleX = worldCellX + x * rectHalf;
                float sampleZ = worldCellZ + z * rectHalf;
                Vector2 localPoint = WorldToCustomLocalXZ(sampleX, sampleZ, sampleY, interactionCenter);
                if (SampleCachedInfluence(localPoint) > 0.01f)
                    return true;
            }
        }

        return false;
    }

    public float GetCustomMeshCellInfluence(float worldCellX, float worldCellZ, float cellHalf, Vector3 interactionCenter)
    {
        if (!HasValidCustomMeshData())
            return 0f;

        float sampleY = interactionCenter.y;
        float step = cellHalf;
        float insideWeight = 0f;
        int sampleRadius = GetSampleRadiusForQuality();
        int sampleAxisCount = sampleRadius * 2 + 1;
        int sampleCount = sampleAxisCount * sampleAxisCount;

        for (int z = -sampleRadius; z <= sampleRadius; z++)
        {
            for (int x = -sampleRadius; x <= sampleRadius; x++)
            {
                float sampleX = worldCellX + x * step;
                float sampleZ = worldCellZ + z * step;
                Vector2 localPoint = WorldToCustomLocalXZ(sampleX, sampleZ, sampleY, interactionCenter);
                insideWeight += SampleCachedInfluence(localPoint);
            }
        }

        return Mathf.Clamp01(insideWeight / sampleCount);
    }

    #endregion

    #region Mesh Caching

    private bool HasValidCustomMeshData()
    {
        RefreshMeshCache();
        return cachedVerticesXZ != null && cachedTriangles != null;
    }

    private void RefreshMeshCache(bool force = false)
    {
        if (!force && cachedMesh == customInteractionMesh && cachedCustomMeshQuality == customMeshQuality)
            return;

        cachedMesh = customInteractionMesh;
        cachedCustomMeshQuality = customMeshQuality;
        cachedVerticesXZ = null;
        cachedTriangles = null;
        cachedLocalBounds = default;
        cachedLocalProjectionRadius = 0f;
        cachedInfluenceMask = null;
        cachedInfluenceMaskResolution = 0;
        cachedLocalMinXZ = Vector2.zero;
        cachedLocalSizeXZ = Vector2.zero;

        if (cachedMesh == null)
            return;

        Vector3[] vertices = cachedMesh.vertices;
        int[] triangles = cachedMesh.triangles;
        if (vertices == null || vertices.Length < 3 || triangles == null || triangles.Length < 3)
            return;

        cachedVerticesXZ = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            cachedVerticesXZ[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        cachedTriangles = triangles;
        cachedLocalBounds = cachedMesh.bounds;

        Vector3 extents = cachedLocalBounds.extents;
        cachedLocalProjectionRadius = Mathf.Sqrt(extents.x * extents.x + extents.z * extents.z);
        BuildInfluenceMask();
    }

    private void CacheInfluenceTrackingState()
    {
        lastDetectionRadius = detectionRadius;
        lastLossyScale = ActiveTransform.lossyScale;
        lastRotation = ActiveTransform.rotation;
        lastPositionOffset = positionOffset;
        lastCustomInteractionMesh = customInteractionMesh;
        lastCustomMeshQuality = customMeshQuality;
    }

    private bool HasNonMotionInfluenceChange()
    {
        bool changed = false;

        if (Mathf.Abs(lastDetectionRadius - detectionRadius) > 0.0001f)
            changed = true;

        if ((lastLossyScale - ActiveTransform.lossyScale).sqrMagnitude > 0.000001f)
            changed = true;

        if (Quaternion.Angle(lastRotation, ActiveTransform.rotation) > 0.05f)
            changed = true;

        if ((lastPositionOffset - positionOffset).sqrMagnitude > 0.000001f)
            changed = true;

        if (lastCustomInteractionMesh != customInteractionMesh || lastCustomMeshQuality != customMeshQuality)
        {
            RefreshMeshCache(force: true);
            changed = true;
        }

        CacheInfluenceTrackingState();
        return changed;
    }

    private void BuildInfluenceMask()
    {
        float sizeX = cachedLocalBounds.size.x;
        float sizeZ = cachedLocalBounds.size.z;
        if (sizeX <= 0.0001f || sizeZ <= 0.0001f)
            return;

        int res = GetMaskResolutionForQuality();
        cachedInfluenceMaskResolution = res;
        cachedInfluenceMask = new float[res * res];
        cachedLocalMinXZ = new Vector2(cachedLocalBounds.min.x, cachedLocalBounds.min.z);
        cachedLocalSizeXZ = new Vector2(sizeX, sizeZ);

        for (int z = 0; z < res; z++)
        {
            float vz = (z + 0.5f) / res;
            float localZ = cachedLocalMinXZ.y + vz * cachedLocalSizeXZ.y;
            for (int x = 0; x < res; x++)
            {
                float ux = (x + 0.5f) / res;
                float localX = cachedLocalMinXZ.x + ux * cachedLocalSizeXZ.x;
                Vector2 localPoint = new Vector2(localX, localZ);
                cachedInfluenceMask[z * res + x] = ContainsLocalPointOnMeshProjection(localPoint) ? 1f : 0f;
            }
        }
    }

    private Vector2 WorldToCustomLocalXZ(float worldX, float worldZ, float worldY, Vector3 interactionCenter)
    {
        Vector3 centerDelta = GetInteractionPosition() - interactionCenter;
        Vector3 worldPoint = new Vector3(worldX, worldY, worldZ) + centerDelta - positionOffset;
        Vector3 localPoint = ActiveTransform.InverseTransformPoint(worldPoint);
        float customScale = GetCustomMeshScaleFactor();
        if (customScale <= 0.0001f)
            customScale = 0.0001f;

        return new Vector2(localPoint.x / customScale, localPoint.z / customScale);
    }

    private bool ContainsLocalPointOnMeshProjection(Vector2 localPointXZ)
    {
        Vector3 min = cachedLocalBounds.min;
        Vector3 max = cachedLocalBounds.max;
        if (localPointXZ.x < min.x || localPointXZ.x > max.x || localPointXZ.y < min.z || localPointXZ.y > max.z)
            return false;

        for (int i = 0; i < cachedTriangles.Length; i += 3)
        {
            Vector2 a = cachedVerticesXZ[cachedTriangles[i]];
            Vector2 b = cachedVerticesXZ[cachedTriangles[i + 1]];
            Vector2 c = cachedVerticesXZ[cachedTriangles[i + 2]];
            if (PointInTriangleXZ(localPointXZ, a, b, c))
                return true;
        }

        return false;
    }

    private float SampleCachedInfluence(Vector2 localPointXZ)
    {
        if (cachedInfluenceMask == null || cachedInfluenceMaskResolution <= 1)
            return ContainsLocalPointOnMeshProjection(localPointXZ) ? 1f : 0f;

        float sizeX = cachedLocalSizeXZ.x;
        float sizeZ = cachedLocalSizeXZ.y;
        if (sizeX <= 0.0001f || sizeZ <= 0.0001f)
            return 0f;

        float u = (localPointXZ.x - cachedLocalMinXZ.x) / sizeX;
        float v = (localPointXZ.y - cachedLocalMinXZ.y) / sizeZ;
        if (u < 0f || u > 1f || v < 0f || v > 1f)
            return 0f;

        int res = cachedInfluenceMaskResolution;
        float fx = u * (res - 1);
        float fz = v * (res - 1);
        int x0 = Mathf.FloorToInt(fx);
        int z0 = Mathf.FloorToInt(fz);
        int x1 = Mathf.Min(x0 + 1, res - 1);
        int z1 = Mathf.Min(z0 + 1, res - 1);

        float tx = fx - x0;
        float tz = fz - z0;

        float i00 = cachedInfluenceMask[z0 * res + x0];
        float i10 = cachedInfluenceMask[z0 * res + x1];
        float i01 = cachedInfluenceMask[z1 * res + x0];
        float i11 = cachedInfluenceMask[z1 * res + x1];

        float ix0 = Mathf.Lerp(i00, i10, tx);
        float ix1 = Mathf.Lerp(i01, i11, tx);
        return Mathf.Lerp(ix0, ix1, tz);
    }

    private int GetMaskResolutionForQuality()
    {
        switch (customMeshQuality)
        {
            case CustomMeshQuality.Low:
                return 12;
            case CustomMeshQuality.Medium:
                return 24;
            case CustomMeshQuality.High:
                return 48;
            default:
                return 24;
        }
    }

    private int GetSampleRadiusForQuality()
    {
        switch (customMeshQuality)
        {
            case CustomMeshQuality.Low:
                return 0;
            case CustomMeshQuality.Medium:
                return 0;
            case CustomMeshQuality.High:
                return 0;
            default:
                return 0;
        }
    }

    #endregion

    #region Geometry Helpers

    private static bool TriangleOverlapsRect(Vector2 a, Vector2 b, Vector2 c, float rectMinX, float rectMaxX, float rectMinZ, float rectMaxZ)
    {
        float triMinX = Mathf.Min(a.x, Mathf.Min(b.x, c.x));
        float triMaxX = Mathf.Max(a.x, Mathf.Max(b.x, c.x));
        float triMinZ = Mathf.Min(a.y, Mathf.Min(b.y, c.y));
        float triMaxZ = Mathf.Max(a.y, Mathf.Max(b.y, c.y));

        if (triMaxX < rectMinX || triMinX > rectMaxX || triMaxZ < rectMinZ || triMinZ > rectMaxZ)
            return false;

        Vector2 r0 = new Vector2(rectMinX, rectMinZ);
        Vector2 r1 = new Vector2(rectMaxX, rectMinZ);
        Vector2 r2 = new Vector2(rectMaxX, rectMaxZ);
        Vector2 r3 = new Vector2(rectMinX, rectMaxZ);

        if (PointInTriangleXZ(r0, a, b, c) || PointInTriangleXZ(r1, a, b, c) || PointInTriangleXZ(r2, a, b, c) || PointInTriangleXZ(r3, a, b, c))
            return true;

        if (PointInRect(a, rectMinX, rectMaxX, rectMinZ, rectMaxZ) || PointInRect(b, rectMinX, rectMaxX, rectMinZ, rectMaxZ) || PointInRect(c, rectMinX, rectMaxX, rectMinZ, rectMaxZ))
            return true;

        if (SegmentsIntersect(a, b, r0, r1) || SegmentsIntersect(a, b, r1, r2) || SegmentsIntersect(a, b, r2, r3) || SegmentsIntersect(a, b, r3, r0))
            return true;
        if (SegmentsIntersect(b, c, r0, r1) || SegmentsIntersect(b, c, r1, r2) || SegmentsIntersect(b, c, r2, r3) || SegmentsIntersect(b, c, r3, r0))
            return true;
        if (SegmentsIntersect(c, a, r0, r1) || SegmentsIntersect(c, a, r1, r2) || SegmentsIntersect(c, a, r2, r3) || SegmentsIntersect(c, a, r3, r0))
            return true;

        return false;
    }

    private static bool PointInRect(Vector2 p, float minX, float maxX, float minZ, float maxZ)
    {
        return p.x >= minX && p.x <= maxX && p.y >= minZ && p.y <= maxZ;
    }

    private static bool SegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        float o1 = Cross2D(p2 - p1, q1 - p1);
        float o2 = Cross2D(p2 - p1, q2 - p1);
        float o3 = Cross2D(q2 - q1, p1 - q1);
        float o4 = Cross2D(q2 - q1, p2 - q1);

        bool straddle1 = (o1 > 0f && o2 < 0f) || (o1 < 0f && o2 > 0f);
        bool straddle2 = (o3 > 0f && o4 < 0f) || (o3 < 0f && o4 > 0f);
        if (straddle1 && straddle2)
            return true;

        const float epsilon = 0.00001f;
        if (Mathf.Abs(o1) <= epsilon && PointOnSegment(p1, p2, q1)) return true;
        if (Mathf.Abs(o2) <= epsilon && PointOnSegment(p1, p2, q2)) return true;
        if (Mathf.Abs(o3) <= epsilon && PointOnSegment(q1, q2, p1)) return true;
        if (Mathf.Abs(o4) <= epsilon && PointOnSegment(q1, q2, p2)) return true;

        return false;
    }

    private static bool PointOnSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        return p.x >= Mathf.Min(a.x, b.x) - 0.00001f && p.x <= Mathf.Max(a.x, b.x) + 0.00001f &&
               p.y >= Mathf.Min(a.y, b.y) - 0.00001f && p.y <= Mathf.Max(a.y, b.y) + 0.00001f;
    }

    private static bool PointInTriangleXZ(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Cross2D(p - a, b - a);
        float d2 = Cross2D(p - b, c - b);
        float d3 = Cross2D(p - c, a - c);

        bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
        bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;
        return !(hasNeg && hasPos);
    }

    private static float Cross2D(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private Bounds GetCustomMeshWorldBounds()
    {
        Vector3 min = cachedLocalBounds.min;
        Vector3 max = cachedLocalBounds.max;
        float customScale = GetCustomMeshScaleFactor();
        Vector3 meshScale = ActiveTransform.lossyScale * customScale;
        Matrix4x4 meshMatrix = Matrix4x4.TRS(GetInteractionPosition(), ActiveTransform.rotation, meshScale);

        Vector3 worldCorner = meshMatrix.MultiplyPoint3x4(new Vector3(min.x, min.y, min.z));
        Bounds worldBounds = new Bounds(worldCorner, Vector3.zero);

        worldCorner = meshMatrix.MultiplyPoint3x4(new Vector3(min.x, min.y, max.z));
        worldBounds.Encapsulate(worldCorner);
        worldCorner = meshMatrix.MultiplyPoint3x4(new Vector3(min.x, max.y, min.z));
        worldBounds.Encapsulate(worldCorner);
        worldCorner = meshMatrix.MultiplyPoint3x4(new Vector3(min.x, max.y, max.z));
        worldBounds.Encapsulate(worldCorner);
        worldCorner = meshMatrix.MultiplyPoint3x4(new Vector3(max.x, min.y, min.z));
        worldBounds.Encapsulate(worldCorner);
        worldCorner = meshMatrix.MultiplyPoint3x4(new Vector3(max.x, min.y, max.z));
        worldBounds.Encapsulate(worldCorner);
        worldCorner = meshMatrix.MultiplyPoint3x4(new Vector3(max.x, max.y, min.z));
        worldBounds.Encapsulate(worldCorner);
        worldCorner = meshMatrix.MultiplyPoint3x4(new Vector3(max.x, max.y, max.z));
        worldBounds.Encapsulate(worldCorner);

        return worldBounds;
    }

    private float GetCustomMeshScaleFactor()
    {
        float targetRadius = Mathf.Max(0.001f, detectionRadius);
        if (cachedLocalProjectionRadius <= 0.0001f)
            return 1f;

        return targetRadius / cachedLocalProjectionRadius;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Vector3 center = GetInteractionPosition();

        if (HasValidCustomMeshData())
        {
            float customScale = GetCustomMeshScaleFactor();
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(center, ActiveTransform.rotation, ActiveTransform.lossyScale * customScale);

            Gizmos.color = IsMoving ? new Color(1f, 1f, 0f, 0.15f) : new Color(0f, 1f, 0f, 0.15f);
            Gizmos.DrawMesh(customInteractionMesh);

            Gizmos.color = IsMoving ? new Color(1f, 1f, 0f, 0.35f) : new Color(0f, 1f, 0f, 0.35f);
            Gizmos.DrawWireMesh(customInteractionMesh);

            Gizmos.matrix = oldMatrix;
            return;
        }

        Gizmos.color = IsMoving ? new Color(1f, 1f, 0f, 0.15f) : new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawWireSphere(center, detectionRadius);

        float visibleRadius = detectionRadius * 0.7f;
        Gizmos.color = IsMoving ? new Color(1f, 1f, 0f, 0.35f) : new Color(0f, 1f, 0f, 0.35f);
        Gizmos.DrawSphere(center, visibleRadius);
    }

    #endregion
}
