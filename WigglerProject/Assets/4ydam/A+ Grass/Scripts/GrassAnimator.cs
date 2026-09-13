using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class GrassAnimator : MonoBehaviour
{
    #region Enums

    public enum PlaybackMode
    {
        Once,
        Loop,
        PingPong
    }

    public enum PathMode
    {
        Linear,
        Curved
    }

    #endregion

    #region Nested Types

    [System.Serializable]
    public class AnimatedInteractor
    {
        [FormerlySerializedAs("pivot")]
        [SerializeField, HideInInspector] private Transform legacyPivot;
        [FormerlySerializedAs("interactor")]
        [SerializeField, HideInInspector] private GrassInteractor legacyInteractorComponent;

        public GameObject interactor;
        [Range(0f, 1f)] public float pathOffset;
        public bool orientToPath = true;
        public Vector3 worldPositionOffset = Vector3.zero;

        public Transform PivotTransform => interactor != null ? interactor.transform : null;
        public GrassInteractor InteractorComponent => interactor != null ? interactor.GetComponent<GrassInteractor>() : null;

        public void ResolveLegacyReference()
        {
            if (interactor != null)
                return;

            if (legacyInteractorComponent != null)
                interactor = legacyInteractorComponent.gameObject;
            else if (legacyPivot != null)
                interactor = legacyPivot.gameObject;
        }
    }

    [System.Serializable]
    public class PathPointSettings
    {
        public AnimationCurve pointFalloffCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public bool stopOnReached;
        [Min(0f)] public float cooldown;
        public UnityEvent onPointReached;
    }

    [System.Serializable]
    public class PathPoint
    {
        public Transform point;
        public AnimationCurve pointFalloffCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public bool stopOnReached;
        [Min(0f)] public float cooldown;
        public UnityEvent onPointReached;
    }

    #endregion

    #region Serialized Fields

    [FormerlySerializedAs("animatedPivots")]
    [SerializeField] private AnimatedInteractor[] animatedInteractors = System.Array.Empty<AnimatedInteractor>();
    [SerializeField] private bool autoResolveInteractors = true;
    [SerializeField] private bool playOnEnable;

    [Header("Playback")]
    [SerializeField] private PlaybackMode playbackMode = PlaybackMode.Once;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float playbackSpeed = 1f;
    [FormerlySerializedAs("progressCurve")]
    [SerializeField] private AnimationCurve globalFalloff = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [SerializeField] private bool disableInteractorsWhenNotPlaying;

    [Header("Path")]
    [SerializeField] private PathMode pathMode = PathMode.Curved;
    [SerializeField] private PathPoint[] pathPointEntries = System.Array.Empty<PathPoint>();
    [FormerlySerializedAs("pathPoints")]
    [SerializeField, HideInInspector] private Transform[] legacyPathPoints;
    [FormerlySerializedAs("pathPointSettings")]
    [SerializeField, HideInInspector] private PathPointSettings[] legacyPathPointSettings = System.Array.Empty<PathPointSettings>();

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color pathColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color pointColor = new Color(1f, 1f, 1f, 1f);
    [Min(0.01f)] [SerializeField] private float pointGizmoSize = 0.08f;
    [Min(0.5f)] [SerializeField] private float pathLineWidth = 2f;
    [Min(0.1f)] [SerializeField] private float sceneHandleSizeMultiplier = 0.55f;
    [Min(0.01f)] [SerializeField] private float sceneMoveBoxSize = 0.09f;

    private readonly List<Transform> cachedPoints = new List<Transform>(8);
    private Vector3[] gizmoLinePoints;

    private bool isPlaying;
    private float playhead;
    private int pingPongDirection = 1;
    private bool isPointCooldownActive;
    private float pointCooldownRemaining;
    private bool hasLastPathT;
    private float lastPathT;

    #endregion

    #region Runtime State

    public bool IsPlaying => isPlaying;
    public float NormalizedTime => Mathf.Clamp01(playhead);
    public Transform[] PathPoints
    {
        get
        {
            if (pathPointEntries == null || pathPointEntries.Length == 0)
                return System.Array.Empty<Transform>();

            Transform[] points = new Transform[pathPointEntries.Length];
            for (int i = 0; i < pathPointEntries.Length; i++)
                points[i] = pathPointEntries[i] != null ? pathPointEntries[i].point : null;

            return points;
        }
    }
    public AnimatedInteractor[] AnimatedInteractors => animatedInteractors;
    public float SceneHandleSizeMultiplier => sceneHandleSizeMultiplier;
    public float SceneMoveBoxSize => sceneMoveBoxSize;

    #endregion

    #region Lifecycle

    private void Reset()
    {
        EnsureDefaultAnimatedInteractorAssigned();
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        if (playOnEnable)
            PlayFromStart();
        else
            ApplyPose();

        UpdateInteractorInfluenceState();
    }

    private void OnDisable()
    {
        isPointCooldownActive = false;
        pointCooldownRemaining = 0f;
        hasLastPathT = false;
        SetInteractorInfluenceOverrideForAll(false);
    }

    private void OnValidate()
    {
        duration = Mathf.Max(0.01f, duration);
        playbackSpeed = Mathf.Max(0f, playbackSpeed);
        pointGizmoSize = Mathf.Max(0.01f, pointGizmoSize);
        pathLineWidth = Mathf.Max(0.5f, pathLineWidth);
        sceneHandleSizeMultiplier = Mathf.Max(0.1f, sceneHandleSizeMultiplier);
        sceneMoveBoxSize = Mathf.Max(0.01f, sceneMoveBoxSize);

        if (globalFalloff == null || globalFalloff.length == 0)
            globalFalloff = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        EnsurePathPointEntries();

        ResolveReferences();
        RefreshPointCache();
        ApplyPose();
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        float frameDt = Time.deltaTime;
        if (frameDt <= 0f)
            return;

        if (isPointCooldownActive)
        {
            // Cooldown is measured in real elapsed seconds, independent of playback speed.
            pointCooldownRemaining -= frameDt;
            if (pointCooldownRemaining > 0f)
                return;

            isPointCooldownActive = false;
            pointCooldownRemaining = 0f;
        }

        float dt = frameDt * playbackSpeed;
        if (dt <= 0f)
            return;

        float delta = dt / duration;
        int frameDirection = playbackMode == PlaybackMode.PingPong ? pingPongDirection : 1;

        switch (playbackMode)
        {
            case PlaybackMode.Once:
                playhead += delta;
                if (playhead >= 1f)
                {
                    playhead = 1f;
                    isPlaying = false;
                }
                break;

            case PlaybackMode.Loop:
                playhead = Mathf.Repeat(playhead + delta, 1f);
                break;

            case PlaybackMode.PingPong:
                playhead += delta * pingPongDirection;
                if (playhead > 1f)
                {
                    playhead = 1f;
                    pingPongDirection = -1;
                }
                else if (playhead < 0f)
                {
                    playhead = 0f;
                    pingPongDirection = 1;
                }
                break;
        }

        float currentPathT = GetPathSampleTime(GetSampleTime());
        if (!hasLastPathT)
        {
            hasLastPathT = true;
            lastPathT = currentPathT;
        }
        else
        {
            ProcessPointReachedEvents(lastPathT, currentPathT, frameDirection);
            lastPathT = currentPathT;
        }

        ApplyPose();
        UpdateInteractorInfluenceState();
    }

    #endregion

    #region Playback

    public void Play()
    {
        isPlaying = true;
        if (!hasLastPathT)
        {
            lastPathT = GetPathSampleTime(GetSampleTime());
            hasLastPathT = true;
        }
        UpdateInteractorInfluenceState();
    }

    public void PlayFromStart()
    {
        playhead = 0f;
        pingPongDirection = 1;
        isPlaying = true;
        isPointCooldownActive = false;
        pointCooldownRemaining = 0f;
        lastPathT = GetPathSampleTime(GetSampleTime());
        hasLastPathT = true;
        ApplyPose();
        UpdateInteractorInfluenceState();
    }

    public void Pause()
    {
        isPlaying = false;
        UpdateInteractorInfluenceState();
    }

    public void Stop()
    {
        isPlaying = false;
        playhead = 0f;
        pingPongDirection = 1;
        isPointCooldownActive = false;
        pointCooldownRemaining = 0f;
        hasLastPathT = false;
        ApplyPose();
        UpdateInteractorInfluenceState();
    }

    public void SetNormalizedTime(float t)
    {
        playhead = Mathf.Clamp01(t);
        lastPathT = GetPathSampleTime(GetSampleTime());
        hasLastPathT = true;
        ApplyPose();
        UpdateInteractorInfluenceState();
    }

    #endregion

    #region Path and Interactor Management

    public void AddPathPoint()
    {
        Transform root = EnsurePathRoot();
        int oldCount = pathPointEntries != null ? pathPointEntries.Length : 0;
        PathPoint[] next = new PathPoint[oldCount + 1];

        for (int i = 0; i < oldCount; i++)
            next[i] = pathPointEntries[i];

        GameObject pointObj = new GameObject("Path Point " + (oldCount + 1));
        Transform point = pointObj.transform;
        point.SetParent(root, false);
        point.localPosition = Vector3.forward * oldCount;
        point.localRotation = Quaternion.identity;
        point.localScale = Vector3.one;
        next[oldCount] = new PathPoint
        {
            point = point,
            pointFalloffCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
            stopOnReached = false,
            cooldown = 0f,
            onPointReached = new UnityEvent()
        };

        pathPointEntries = next;
        RefreshPointCache();
        ApplyPose();
    }

    public void RemoveLastPathPoint()
    {
        if (pathPointEntries == null || pathPointEntries.Length == 0)
            return;

        int last = pathPointEntries.Length - 1;
        Transform toDelete = pathPointEntries[last] != null ? pathPointEntries[last].point : null;

        if (toDelete != null)
        {
            if (Application.isPlaying)
                Destroy(toDelete.gameObject);
            else
                DestroyImmediate(toDelete.gameObject);
        }

        PathPoint[] next = new PathPoint[last];
        for (int i = 0; i < last; i++)
            next[i] = pathPointEntries[i];

        pathPointEntries = next;
        RefreshPointCache();
        ApplyPose();
    }

    public void ClearPathPoints()
    {
        if (pathPointEntries != null)
        {
            for (int i = 0; i < pathPointEntries.Length; i++)
            {
                Transform point = pathPointEntries[i] != null ? pathPointEntries[i].point : null;
                if (point == null)
                    continue;

                if (Application.isPlaying)
                    Destroy(point.gameObject);
                else
                    DestroyImmediate(point.gameObject);
            }
        }

        pathPointEntries = System.Array.Empty<PathPoint>();
        legacyPathPoints = null;
        legacyPathPointSettings = System.Array.Empty<PathPointSettings>();
        RefreshPointCache();
        ApplyPose();
    }

    public void AddAnimatedInteractor(Transform pivot)
    {
        AddAnimatedInteractor(pivot != null ? pivot.gameObject : null);
    }

    public void AddAnimatedInteractor(GameObject interactorObject)
    {
        if (interactorObject == null)
            return;

        if (animatedInteractors != null)
        {
            for (int i = 0; i < animatedInteractors.Length; i++)
            {
                AnimatedInteractor existing = animatedInteractors[i];
                if (existing != null && existing.interactor == interactorObject)
                    return;
            }
        }

        int oldCount = animatedInteractors != null ? animatedInteractors.Length : 0;
        AnimatedInteractor[] next = new AnimatedInteractor[oldCount + 1];
        for (int i = 0; i < oldCount; i++)
            next[i] = animatedInteractors[i];

        next[oldCount] = new AnimatedInteractor
        {
            interactor = interactorObject,
            pathOffset = 0f,
            orientToPath = true,
            worldPositionOffset = Vector3.zero
        };

        animatedInteractors = next;
        ResolveReferences();
        ApplyPose();
    }

    public void ResetAnimatedInteractorElements()
    {
        if (animatedInteractors == null || animatedInteractors.Length == 0)
            return;

        for (int i = 0; i < animatedInteractors.Length; i++)
        {
            AnimatedInteractor entry = animatedInteractors[i];
            if (entry == null)
                continue;

            entry.ResolveLegacyReference();

            entry.pathOffset = 0f;
            entry.orientToPath = true;
            entry.worldPositionOffset = Vector3.zero;
        }

        ApplyPose();
    }

    public void ResetPathPointElements()
    {
        EnsurePathPointEntries();
        if (pathPointEntries == null || pathPointEntries.Length == 0)
            return;

        for (int i = 0; i < pathPointEntries.Length; i++)
        {
            PathPoint entry = pathPointEntries[i];
            if (entry == null)
                continue;

            entry.pointFalloffCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            entry.stopOnReached = false;
            entry.cooldown = 0f;
            entry.onPointReached = new UnityEvent();
        }
    }

    public void AddAnimatedPivot(Transform pivot)
    {
        AddAnimatedInteractor(pivot);
    }

    #endregion

    #region Reference Resolution

    private void ResolveReferences()
    {
        if (animatedInteractors == null)
        {
            animatedInteractors = System.Array.Empty<AnimatedInteractor>();
        }

        EnsureDefaultAnimatedInteractorAssigned();
        EnsurePathPointEntries();

        for (int i = 0; i < animatedInteractors.Length; i++)
        {
            AnimatedInteractor entry = animatedInteractors[i];
            if (entry == null)
                continue;

            entry.ResolveLegacyReference();

            if (entry.interactor == null)
                continue;

            if (!autoResolveInteractors)
                continue;

            _ = entry.InteractorComponent;
        }

        RefreshPointCache();
    }

    private void EnsurePathPointEntries()
    {
        if (pathPointEntries == null)
            pathPointEntries = System.Array.Empty<PathPoint>();

        if ((pathPointEntries.Length == 0) && legacyPathPoints != null && legacyPathPoints.Length > 0)
        {
            int legacyCount = legacyPathPoints.Length;
            PathPoint[] migrated = new PathPoint[legacyCount];
            for (int i = 0; i < legacyCount; i++)
            {
                PathPointSettings legacy = (legacyPathPointSettings != null && i < legacyPathPointSettings.Length)
                    ? legacyPathPointSettings[i]
                    : null;

                migrated[i] = new PathPoint
                {
                    point = legacyPathPoints[i],
                    pointFalloffCurve = legacy != null && legacy.pointFalloffCurve != null && legacy.pointFalloffCurve.length > 0
                        ? legacy.pointFalloffCurve
                        : AnimationCurve.Linear(0f, 0f, 1f, 1f),
                    stopOnReached = legacy != null && legacy.stopOnReached,
                    cooldown = legacy != null ? Mathf.Max(0f, legacy.cooldown) : 0f,
                    onPointReached = legacy != null ? legacy.onPointReached : new UnityEvent()
                };
            }

            pathPointEntries = migrated;
            legacyPathPoints = null;
            legacyPathPointSettings = System.Array.Empty<PathPointSettings>();
        }

        int targetCount = pathPointEntries != null ? pathPointEntries.Length : 0;
        if (targetCount <= 0)
            return;

        for (int i = 0; i < pathPointEntries.Length; i++)
        {
            if (pathPointEntries[i] == null)
                pathPointEntries[i] = new PathPoint();

            if (pathPointEntries[i].pointFalloffCurve == null || pathPointEntries[i].pointFalloffCurve.length == 0)
                pathPointEntries[i].pointFalloffCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            pathPointEntries[i].cooldown = Mathf.Max(0f, pathPointEntries[i].cooldown);
        }
    }

    #endregion

    #region Path Evaluation

    private PathPoint GetPathPoint(int index)
    {
        if (pathPointEntries == null || pathPointEntries.Length == 0)
            return null;

        int safeIndex = Mathf.Clamp(index, 0, pathPointEntries.Length - 1);
        return pathPointEntries[safeIndex];
    }

    private AnimationCurve GetSegmentFalloffCurve(int segmentIndex)
    {
        PathPoint point = GetPathPoint(segmentIndex);
        return point != null ? point.pointFalloffCurve : null;
    }

    private void ProcessPointReachedEvents(float previousT, float currentT, int frameDirection)
    {
        int count = cachedPoints.Count;
        if (count == 0)
            return;

        bool looping = playbackMode == PlaybackMode.Loop;

        if (frameDirection >= 0)
        {
            if (looping && currentT < previousT)
            {
                CheckForwardRange(previousT, 1f, count, true);
                if (isPointCooldownActive)
                    return;
                CheckForwardRange(0f, currentT, count, false);
            }
            else
            {
                CheckForwardRange(previousT, currentT, count, false);
            }
        }
        else
        {
            CheckBackwardRange(previousT, currentT, count);
        }
    }

    private void CheckForwardRange(float startExclusive, float endInclusive, int count, bool includeStartPoint)
    {
        const float epsilon = 0.0001f;
        bool includeLowerBound = includeStartPoint || startExclusive <= epsilon;

        for (int i = 0; i < count; i++)
        {
            float pointT = GetPointNormalizedT(i, count);
            bool isCrossed = includeLowerBound
                ? pointT >= startExclusive && pointT <= endInclusive
                : pointT > startExclusive && pointT <= endInclusive;

            if (!isCrossed)
                continue;

            TriggerPathPointReached(i);
            if (isPointCooldownActive)
                return;
        }
    }

    private void CheckBackwardRange(float startExclusive, float endInclusive, int count)
    {
        const float epsilon = 0.0001f;
        bool includeUpperBound = startExclusive >= (1f - epsilon);

        for (int i = count - 1; i >= 0; i--)
        {
            float pointT = GetPointNormalizedT(i, count);
            bool isCrossed = includeUpperBound
                ? pointT <= startExclusive && pointT >= endInclusive
                : pointT < startExclusive && pointT >= endInclusive;

            if (isCrossed)
            {
                TriggerPathPointReached(i);
                if (isPointCooldownActive)
                    return;
            }
        }
    }

    private float GetPointNormalizedT(int pointIndex, int count)
    {
        if (count <= 1)
            return 0f;

        if (playbackMode == PlaybackMode.Loop)
            return pointIndex / (float)count;

        return pointIndex / (float)(count - 1);
    }

    private void TriggerPathPointReached(int pointIndex)
    {
        PathPoint point = GetPathPoint(pointIndex);
        if (point == null)
            return;

        point.onPointReached?.Invoke();

        if (!point.stopOnReached)
            return;

        float cooldown = Mathf.Max(0f, point.cooldown);
        if (cooldown > 0f)
        {
            isPointCooldownActive = true;
            pointCooldownRemaining = cooldown;
        }
    }

    private void UpdateInteractorInfluenceState()
    {
        SetInteractorInfluenceOverrideForAll(isPlaying);
        SetInteractorEnabledStateForAll(isPlaying || !disableInteractorsWhenNotPlaying);
    }

    private void SetInteractorInfluenceOverrideForAll(bool enabled)
    {
        if (animatedInteractors == null)
            return;

        for (int i = 0; i < animatedInteractors.Length; i++)
        {
            AnimatedInteractor entry = animatedInteractors[i];
            if (entry == null)
                continue;

            GrassInteractor component = entry.InteractorComponent;
            if (component == null)
                continue;

            component.SetForceInfluenceOverride(enabled);
        }
    }

    private void SetInteractorEnabledStateForAll(bool enabled)
    {
        if (!Application.isPlaying || animatedInteractors == null)
            return;

        for (int i = 0; i < animatedInteractors.Length; i++)
        {
            AnimatedInteractor entry = animatedInteractors[i];
            if (entry == null)
                continue;

            GrassInteractor component = entry.InteractorComponent;
            if (component == null)
                continue;

            if (component.enabled != enabled)
                component.enabled = enabled;
        }
    }

    private void RefreshPointCache()
    {
        cachedPoints.Clear();

        if (pathPointEntries == null)
            return;

        for (int i = 0; i < pathPointEntries.Length; i++)
        {
            Transform point = pathPointEntries[i] != null ? pathPointEntries[i].point : null;
            if (point != null)
                cachedPoints.Add(point);
        }
    }

    private void ApplyPose()
    {
        if (animatedInteractors == null || animatedInteractors.Length == 0)
            return;

        float sampleTime = GetSampleTime();
        float basePathT = GetPathSampleTime(sampleTime);

        for (int i = 0; i < animatedInteractors.Length; i++)
        {
            AnimatedInteractor entry = animatedInteractors[i];
            if (entry == null)
                continue;

            Transform pivotTransform = entry.PivotTransform;
            if (pivotTransform == null)
                continue;

            float pivotT = playbackMode == PlaybackMode.Loop
                ? Mathf.Repeat(basePathT + entry.pathOffset, 1f)
                : Mathf.Clamp01(basePathT + entry.pathOffset);

            Vector3 worldPosition = EvaluatePosition(pivotT) + entry.worldPositionOffset;
            pivotTransform.position = worldPosition;

            if (entry.orientToPath && cachedPoints.Count >= 2)
            {
                Vector3 forward = EvaluatePathForward(pivotT);
                if (forward.sqrMagnitude > 0.000001f)
                    pivotTransform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
            }
        }
    }

    private float GetSampleTime()
    {
        if (playbackMode == PlaybackMode.Loop)
            return Mathf.Repeat(playhead, 1f);

        return Mathf.Clamp01(playhead);
    }

    private float GetPathSampleTime(float t)
    {
        return Mathf.Clamp01(EvaluateCurve(globalFalloff, t));
    }

    private Vector3 EvaluatePosition(float t)
    {
        if (cachedPoints.Count == 0)
            return transform.position;
        if (cachedPoints.Count == 1)
            return cachedPoints[0].position;

        if (pathMode == PathMode.Curved && cachedPoints.Count >= 3)
            return EvaluateCatmullRom(t);

        return EvaluateLinearPath(t);
    }

    private Vector3 EvaluateLinearPath(float t)
    {
        int count = cachedPoints.Count;
        bool looping = playbackMode == PlaybackMode.Loop;
        float scaled = looping ? Mathf.Repeat(t, 1f) * count : Mathf.Clamp01(t) * (count - 1);

        int segment = Mathf.FloorToInt(scaled);
        float segmentT = scaled - segment;

        int i0;
        int i1;
        if (looping)
        {
            i0 = segment % count;
            if (i0 < 0) i0 += count;
            i1 = (i0 + 1) % count;
        }
        else
        {
            i0 = Mathf.Clamp(segment, 0, count - 1);
            i1 = Mathf.Min(i0 + 1, count - 1);
        }

        segmentT = Mathf.Clamp01(EvaluateCurve(GetSegmentFalloffCurve(i0), segmentT));

        return Vector3.Lerp(cachedPoints[i0].position, cachedPoints[i1].position, segmentT);
    }

    private Vector3 EvaluateCatmullRom(float t)
    {
        int count = cachedPoints.Count;
        bool looping = playbackMode == PlaybackMode.Loop;
        float scaled = looping ? Mathf.Repeat(t, 1f) * count : Mathf.Clamp01(t) * (count - 1);

        int segment = Mathf.FloorToInt(scaled);
        float localT = scaled - segment;

        int curveIndex;
        if (looping)
        {
            curveIndex = segment % count;
            if (curveIndex < 0)
                curveIndex += count;
        }
        else
        {
            curveIndex = Mathf.Clamp(segment, 0, count - 1);
        }

        localT = Mathf.Clamp01(EvaluateCurve(GetSegmentFalloffCurve(curveIndex), localT));

        Vector3 p0 = GetSplinePoint(segment - 1, looping);
        Vector3 p1 = GetSplinePoint(segment, looping);
        Vector3 p2 = GetSplinePoint(segment + 1, looping);
        Vector3 p3 = GetSplinePoint(segment + 2, looping);

        float tt = localT * localT;
        float ttt = tt * localT;

        return 0.5f * ((2f * p1) +
                       (-p0 + p2) * localT +
                       (2f * p0 - 5f * p1 + 4f * p2 - p3) * tt +
                       (-p0 + 3f * p1 - 3f * p2 + p3) * ttt);
    }

    private Vector3 EvaluatePathForward(float t)
    {
        const float sample = 0.01f;
        bool looping = playbackMode == PlaybackMode.Loop;

        float t0 = looping ? Mathf.Repeat(t - sample, 1f) : Mathf.Clamp01(t - sample);
        float t1 = looping ? Mathf.Repeat(t + sample, 1f) : Mathf.Clamp01(t + sample);

        Vector3 p0 = EvaluatePosition(t0);
        Vector3 p1 = EvaluatePosition(t1);
        return p1 - p0;
    }

    private Vector3 GetSplinePoint(int index, bool looping)
    {
        int count = cachedPoints.Count;
        int i;

        if (looping)
        {
            i = index % count;
            if (i < 0)
                i += count;
        }
        else
        {
            i = Mathf.Clamp(index, 0, count - 1);
        }

        return cachedPoints[i].position;
    }

    private static float EvaluateCurve(AnimationCurve curve, float t)
    {
        if (curve == null || curve.length == 0)
            return t;

        return curve.Evaluate(Mathf.Clamp01(t));
    }

    private Transform EnsurePathRoot()
    {
        Transform root = transform.Find("Path Points");
        if (root == null)
        {
            GameObject rootObj = new GameObject("Path Points");
            root = rootObj.transform;
            root.SetParent(transform, false);
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;
            root.localScale = Vector3.one;
        }

        return root;
    }

    private void EnsureDefaultAnimatedInteractorAssigned()
    {
        if (HasAnyAssignedAnimatedInteractor())
            return;

        Transform defaultPivot = FindDefaultAnimatedInteractorTransform();
        if (defaultPivot == null)
            defaultPivot = CreateDefaultAnimatedInteractorTransform();

        if (defaultPivot != null)
            AddAnimatedInteractor(defaultPivot.gameObject);
    }

    private bool HasAnyAssignedAnimatedInteractor()
    {
        if (animatedInteractors == null)
            return false;

        for (int i = 0; i < animatedInteractors.Length; i++)
        {
            AnimatedInteractor entry = animatedInteractors[i];
            if (entry == null)
                continue;

            entry.ResolveLegacyReference();
            if (entry.interactor != null)
                return true;
        }

        return false;
    }

    private Transform FindDefaultAnimatedInteractorTransform()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child != null && child.name == "AnimatedGrassInteractor")
                return child;
        }

        return null;
    }

    private Transform CreateDefaultAnimatedInteractorTransform()
    {
        GameObject interactorObject = new GameObject("AnimatedGrassInteractor");
        Transform interactorTransform = interactorObject.transform;
        interactorTransform.SetParent(transform, false);
        interactorTransform.localPosition = Vector3.zero;
        interactorTransform.localRotation = Quaternion.identity;
        interactorTransform.localScale = Vector3.one;

        if (interactorObject.GetComponent<GrassInteractor>() == null)
            interactorObject.AddComponent<GrassInteractor>();

        return interactorTransform;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        DrawAnimatorGizmos();
    }

    public void DrawAnimatorGizmos()
    {
        if (!showGizmos)
            return;

        RefreshPointCache();
        if (cachedPoints.Count == 0)
            return;

        Gizmos.color = pointColor;
        for (int i = 0; i < cachedPoints.Count; i++)
        {
            Gizmos.DrawSphere(cachedPoints[i].position, pointGizmoSize);

        #if UNITY_EDITOR
            Handles.Label(cachedPoints[i].position + Vector3.up * 0.12f, "P" + (i + 1));
        #endif
        }

        Gizmos.color = pathColor;
        const int samples = 48;

    #if UNITY_EDITOR
        int linePointCount = samples + 1;
        if (gizmoLinePoints == null || gizmoLinePoints.Length != linePointCount)
            gizmoLinePoints = new Vector3[linePointCount];
    #endif

        Vector3 previous = EvaluatePosition(0f);
    #if UNITY_EDITOR
        gizmoLinePoints[0] = previous;
    #endif
        for (int i = 1; i <= samples; i++)
        {
            float t = i / (float)samples;
            Vector3 next = EvaluatePosition(t);

    #if UNITY_EDITOR
            gizmoLinePoints[i] = next;
    #else
            Gizmos.DrawLine(previous, next);
    #endif

            previous = next;
        }

    #if UNITY_EDITOR
        Handles.color = pathColor;
        Handles.DrawAAPolyLine(pathLineWidth, gizmoLinePoints);
    #endif
    }

    #endregion
}
