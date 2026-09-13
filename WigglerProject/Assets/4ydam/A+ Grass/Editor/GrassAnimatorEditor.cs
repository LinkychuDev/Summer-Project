using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(GrassAnimator))]
public class GrassAnimatorEditor : Editor
{
    public VisualTreeAsset visualTreeAsset;
    private const string DocumentationUrl = "https://4ydam.com/assets/a+grass/getting-started/";
    private static readonly AnimationCurve DefaultGlobalFalloff = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    private Slider scrubSlider;
    private FloatField scrubField;
    private bool suppressScrubCallbacks;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        if (visualTreeAsset == null)
        {
            root.Add(new HelpBox("Assign a VisualTreeAsset to GrassAnimatorEditor.", HelpBoxMessageType.Warning));
            return root;
        }

        root.Add(new IMGUIContainer(AGrassBannerDrawer.DrawBanner));

        visualTreeAsset.CloneTree(root);
        root.Bind(serializedObject);

        ConfigureAnimatorFields(root);
        
        var documentationButton = root.Q<Button>("documentationButton");
        if (documentationButton != null)
        {
            documentationButton.clicked += () => Application.OpenURL(DocumentationUrl);
        }

        return root;

    }

    private void ConfigureAnimatorFields(VisualElement root)
    {
        HookPlaybackButtons(root);
        HookScrubControls(root);
        HookPathUtilityButtons(root);
        InjectSectionLists(root);
        InjectAnimatorToggles(root);

        var playbackSpeedResetButton = root.Q<VisualElement>("PlaybackSpeed_VisualElement")?.Q<Button>();
        if (playbackSpeedResetButton != null)
        {
            playbackSpeedResetButton.clicked += () =>
            {
                serializedObject.FindProperty("playbackSpeed").floatValue = 1f;
                serializedObject.ApplyModifiedProperties();
            };
        }

        ReplaceEnumControl(root, "PlaybackMode_VisualElement", "playbackMode", GrassAnimator.PlaybackMode.Once);
        ReplaceEnumControl(root, "PathMode_VisualElement", "pathMode", GrassAnimator.PathMode.Curved);

        var globalFalloffResetButton = root.Q<VisualElement>("GlobalFalloff_VisualElement")?.Q<Button>();
        if (globalFalloffResetButton != null)
        {
            globalFalloffResetButton.clicked += () =>
            {
                serializedObject.FindProperty("globalFalloff").animationCurveValue = new AnimationCurve(DefaultGlobalFalloff.keys);
                serializedObject.ApplyModifiedProperties();
            };
        }

        var showGizmosToggle = root.Q<Toggle>("showGizmosToggle");
        var pathColourRow = root.Q<VisualElement>("PathColour_VisualElement");
        var pointsColourRow = root.Q<VisualElement>("PointsColour_VisualElement");

        void ApplyGizmoColorVisibility(bool showGizmos)
        {
            if (pathColourRow != null)
                pathColourRow.style.display = showGizmos ? DisplayStyle.Flex : DisplayStyle.None;

            if (pointsColourRow != null)
                pointsColourRow.style.display = showGizmos ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (showGizmosToggle != null)
        {
            ApplyGizmoColorVisibility(showGizmosToggle.value);
            showGizmosToggle.RegisterValueChangedCallback(evt => ApplyGizmoColorVisibility(evt.newValue));
        }
        else
        {
            ApplyGizmoColorVisibility(false);
        }
    }

    private void InjectAnimatorToggles(VisualElement root)
    {
        AddPropertyBelowElement(
            root,
            "Animation_VisualElement",
            "PlaybackMode_VisualElement",
            "disableInteractorsWhenNotPlaying",
            "Hidden Interactors",
            "Disables linked GrassInteractor components whenever playback is not active.");
    }

    private void HookPathUtilityButtons(VisualElement root)
    {
        var pathSection = root.Q<VisualElement>("Path_VisualElement");
        if (pathSection == null)
            return;

        var buttonRowContainer = new VisualElement();
        buttonRowContainer.style.width = Length.Percent(100f);
        buttonRowContainer.style.alignItems = Align.Stretch;
        buttonRowContainer.style.paddingLeft = 10f;
        buttonRowContainer.style.paddingRight = 10f;
        buttonRowContainer.style.marginBottom = 8f;

        var buttonRow = new VisualElement();
        buttonRow.style.flexDirection = FlexDirection.Row;
        buttonRow.style.justifyContent = Justify.SpaceBetween;
        buttonRow.style.width = Length.Percent(100f);
        buttonRow.style.flexGrow = 1f;

        Button addPointButton = new Button(() =>
        {
            GrassAnimator animator = target as GrassAnimator;
            if (animator == null)
                return;

            Undo.RegisterFullObjectHierarchyUndo(animator.gameObject, "Add Grass Path Point");
            animator.AddPathPoint();
            EditorUtility.SetDirty(animator);
        }) { text = "Add Point" };

        Button removeLastButton = new Button(() =>
        {
            GrassAnimator animator = target as GrassAnimator;
            if (animator == null)
                return;

            Undo.RegisterFullObjectHierarchyUndo(animator.gameObject, "Remove Grass Path Point");
            animator.RemoveLastPathPoint();
            EditorUtility.SetDirty(animator);
        }) { text = "Remove Last" };

        Button clearAllButton = new Button(() =>
        {
            GrassAnimator animator = target as GrassAnimator;
            if (animator == null)
                return;

            Undo.RegisterFullObjectHierarchyUndo(animator.gameObject, "Clear Grass Path Points");
            animator.ClearPathPoints();
            EditorUtility.SetDirty(animator);
        }) { text = "Clear All" };

        buttonRow.Add(addPointButton);
        buttonRow.Add(removeLastButton);
        buttonRow.Add(clearAllButton);

        for (int i = 0; i < buttonRow.childCount; i++)
        {
            if (buttonRow[i] is Button b)
            {
                b.style.width = StyleKeyword.Auto;
                b.style.flexGrow = 1f;
                b.style.marginRight = i < buttonRow.childCount - 1 ? 6f : 0f;
            }
        }

        buttonRowContainer.Add(buttonRow);

        VisualElement separator = pathSection.Q<VisualElement>("Seperator");
        if (separator != null)
        {
            int separatorIndex = pathSection.IndexOf(separator);
            pathSection.Insert(Mathf.Max(1, separatorIndex), buttonRowContainer);
        }
        else
        {
            int insertIndex = pathSection.childCount > 0 ? 1 : 0;
            pathSection.Insert(insertIndex, buttonRowContainer);
        }
    }

    private void InjectSectionLists(VisualElement root)
    {
        AddListToSection(
            root,
            "Animation_VisualElement",
            "animatedInteractors",
            "Animated Interactors",
            "Assign pivots/interactors that will be animated along the configured path.");

        AddListToSection(
            root,
            "Path_VisualElement",
            "pathPointEntries",
            "Path Points",
            "Path point transforms and per-point settings used by the animator.");
    }

    private void AddListToSection(VisualElement root, string sectionName, string propertyName, string label, string tooltip)
    {
        VisualElement section = root.Q<VisualElement>(sectionName);
        if (section == null)
            return;

        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            return;

        var container = new VisualElement();
        container.style.marginLeft = 10f;
        container.style.marginRight = 10f;
        container.style.marginTop = 4f;
        container.style.marginBottom = 8f;

        var resetRow = new VisualElement();
        resetRow.style.flexDirection = FlexDirection.Row;
        resetRow.style.justifyContent = Justify.FlexEnd;
        resetRow.style.marginBottom = 4f;

        var resetButton = new Button(() =>
        {
            GrassAnimator animator = target as GrassAnimator;
            if (animator == null)
                return;

            Undo.RecordObject(animator, "Reset " + label);

            if (propertyName == "animatedInteractors")
                animator.ResetAnimatedInteractorElements();
            else if (propertyName == "pathPointEntries")
                animator.ResetPathPointElements();

            EditorUtility.SetDirty(animator);
            serializedObject.Update();
        })
        {
            text = "Reset " + label
        };

        resetRow.Add(resetButton);
        container.Add(resetRow);

        var fieldRow = new VisualElement();
        fieldRow.style.flexDirection = FlexDirection.Row;
        fieldRow.style.alignItems = Align.FlexStart;
        fieldRow.style.width = Length.Percent(100f);

        var infoLabel = new Label("i");
        infoLabel.tooltip = tooltip;
        infoLabel.style.marginRight = 14f;
        infoLabel.style.marginTop = 3f;
        infoLabel.style.paddingLeft = 5f;
        infoLabel.style.paddingRight = 5f;
        infoLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        infoLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        infoLabel.style.backgroundColor = new Color(1f, 1f, 1f, 1f);
        infoLabel.style.color = new Color(0f, 0f, 0f, 1f);
        infoLabel.style.borderTopLeftRadius = 10f;
        infoLabel.style.borderTopRightRadius = 10f;
        infoLabel.style.borderBottomLeftRadius = 10f;
        infoLabel.style.borderBottomRightRadius = 10f;

        var field = new PropertyField(property, label);
        field.style.flexGrow = 1f;
        field.style.marginLeft = 2f;
        field.Bind(serializedObject);

        fieldRow.Add(infoLabel);
        fieldRow.Add(field);
        container.Add(fieldRow);

        section.Add(container);
    }

    private void AddPropertyToSection(VisualElement root, string sectionName, string propertyName, string label, string tooltip)
    {
        VisualElement section = root.Q<VisualElement>(sectionName);
        if (section == null)
            return;

        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            return;

        var container = new VisualElement();
        container.style.marginLeft = 10f;
        container.style.marginRight = 10f;
        container.style.marginTop = 4f;
        container.style.marginBottom = 8f;

        var fieldRow = new VisualElement();
        fieldRow.style.flexDirection = FlexDirection.Row;
        fieldRow.style.alignItems = Align.FlexStart;
        fieldRow.style.width = Length.Percent(100f);

        var infoLabel = new Label("i");
        infoLabel.tooltip = tooltip;
        infoLabel.style.marginRight = 14f;
        infoLabel.style.marginTop = 3f;
        infoLabel.style.paddingLeft = 5f;
        infoLabel.style.paddingRight = 5f;
        infoLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        infoLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        infoLabel.style.backgroundColor = new Color(1f, 1f, 1f, 1f);
        infoLabel.style.color = new Color(0f, 0f, 0f, 1f);
        infoLabel.style.borderTopLeftRadius = 10f;
        infoLabel.style.borderTopRightRadius = 10f;
        infoLabel.style.borderBottomLeftRadius = 10f;
        infoLabel.style.borderBottomRightRadius = 10f;

        var field = new PropertyField(property, label);
        field.style.flexGrow = 1f;
        field.style.marginLeft = 2f;
        field.Bind(serializedObject);

        fieldRow.Add(infoLabel);
        fieldRow.Add(field);
        container.Add(fieldRow);

        section.Add(container);
    }

    private void AddPropertyBelowElement(VisualElement root, string sectionName, string anchorElementName, string propertyName, string label, string tooltip)
    {
        VisualElement section = root.Q<VisualElement>(sectionName);
        if (section == null)
            return;

        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
            return;

        var container = new VisualElement();
        container.style.marginLeft = 10f;
        container.style.marginRight = 10f;
        container.style.marginTop = 4f;
        container.style.marginBottom = 8f;

        var fieldRow = new VisualElement();
        fieldRow.style.flexDirection = FlexDirection.Row;
        fieldRow.style.justifyContent = Justify.FlexEnd;
        fieldRow.style.alignItems = Align.FlexStart;
        fieldRow.style.width = Length.Percent(100f);
        fieldRow.style.height = 25f;
        fieldRow.style.maxHeight = 34f;

        var infoLabel = new Label("i");
        infoLabel.tooltip = tooltip;
        infoLabel.style.marginRight = 14f;
        infoLabel.style.marginBottom = 7f;
        infoLabel.style.paddingLeft = 5f;
        infoLabel.style.paddingRight = 5f;
        infoLabel.style.alignSelf = Align.FlexEnd;
        infoLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        infoLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        infoLabel.style.backgroundColor = new Color(1f, 1f, 1f, 1f);
        infoLabel.style.color = new Color(0f, 0f, 0f, 1f);
        infoLabel.style.borderTopLeftRadius = 10f;
        infoLabel.style.borderTopRightRadius = 10f;
        infoLabel.style.borderBottomLeftRadius = 10f;
        infoLabel.style.borderBottomRightRadius = 10f;

        var rowLabel = new Label(label);
        rowLabel.style.marginBottom = 7f;
        rowLabel.style.alignSelf = Align.FlexEnd;
        rowLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

        var valueSlot = new VisualElement();
        valueSlot.style.flexGrow = 1f;
        valueSlot.style.flexDirection = FlexDirection.Row;
        valueSlot.style.justifyContent = Justify.FlexEnd;

        var toggle = new Toggle();
        toggle.bindingPath = propertyName;
        toggle.style.right = 5f;
        toggle.Bind(serializedObject);
        valueSlot.Add(toggle);

        fieldRow.Add(infoLabel);
        fieldRow.Add(rowLabel);
        fieldRow.Add(valueSlot);
        container.Add(fieldRow);

        VisualElement anchor = section.Q<VisualElement>(anchorElementName);
        if (anchor == null)
        {
            section.Add(container);
            return;
        }

        int insertIndex = section.IndexOf(anchor) + 1;
        section.Insert(Mathf.Clamp(insertIndex, 0, section.childCount), container);
    }

    private void HookPlaybackButtons(VisualElement root)
    {
        var animationSection = root.Q<VisualElement>("Animation_VisualElement");
        if (animationSection == null)
            return;

        Button playButton = animationSection.Q<Button>("Button");
        if (playButton == null)
            return;

        VisualElement buttonRow = playButton.parent;
        if (buttonRow != null)
        {
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.justifyContent = Justify.SpaceBetween;
            buttonRow.style.width = Length.Percent(100f);
            buttonRow.style.flexGrow = 1f;

            VisualElement buttonRowContainer = buttonRow.parent;
            if (buttonRowContainer != null)
            {
                buttonRowContainer.style.width = Length.Percent(100f);
                buttonRowContainer.style.alignItems = Align.Stretch;
                buttonRowContainer.style.paddingLeft = 10f;
                buttonRowContainer.style.paddingRight = 10f;
            }

            for (int i = 0; i < buttonRow.childCount; i++)
            {
                if (buttonRow[i] is Button b)
                {
                    b.style.width = StyleKeyword.Auto;
                    b.style.flexGrow = 1f;
                    b.style.marginRight = i < buttonRow.childCount - 1 ? 6f : 0f;
                }
            }
        }

        Button pauseButton = null;
        Button stopButton = null;
        foreach (Button btn in animationSection.Query<Button>().ToList())
        {
            if (btn == playButton)
                continue;

            if (btn.text == "Pause")
                pauseButton = btn;
            else if (btn.text == "Stop")
                stopButton = btn;
        }

        playButton.clicked += () =>
        {
            GrassAnimator animator = target as GrassAnimator;
            if (animator == null)
                return;

            animator.Play();
            EditorUtility.SetDirty(animator);
            SyncScrubFromAnimator();
        };

        if (pauseButton != null)
        {
            pauseButton.clicked += () =>
            {
                GrassAnimator animator = target as GrassAnimator;
                if (animator == null)
                    return;

                animator.Pause();
                EditorUtility.SetDirty(animator);
                SyncScrubFromAnimator();
            };
        }

        if (stopButton != null)
        {
            stopButton.clicked += () =>
            {
                GrassAnimator animator = target as GrassAnimator;
                if (animator == null)
                    return;

                animator.Stop();
                EditorUtility.SetDirty(animator);
                SyncScrubFromAnimator();
            };
        }
    }

    private void HookScrubControls(VisualElement root)
    {
        var currentTimeRow = root.Q<VisualElement>("CurrentTime_VisualElement");
        if (currentTimeRow == null)
            return;

        scrubSlider = currentTimeRow.Q<Slider>();
        scrubField = currentTimeRow.Q<FloatField>();
        var scrubResetButton = currentTimeRow.Q<Button>("resetButton");

        if (scrubSlider != null)
        {
            scrubSlider.Unbind();
            scrubSlider.bindingPath = string.Empty;
            scrubSlider.lowValue = 0f;
            scrubSlider.highValue = 1f;
            scrubSlider.showInputField = false;
            scrubSlider.RegisterValueChangedCallback(evt =>
            {
                if (suppressScrubCallbacks)
                    return;

                ApplyScrubValue(evt.newValue);
            });
        }

        if (scrubField != null)
        {
            scrubField.Unbind();
            scrubField.bindingPath = string.Empty;
            scrubField.RegisterValueChangedCallback(evt =>
            {
                if (suppressScrubCallbacks)
                    return;

                ApplyScrubValue(evt.newValue);
            });
        }

        if (scrubResetButton != null)
            scrubResetButton.clicked += () => ApplyScrubValue(0f);

        SyncScrubFromAnimator();
    }

    private void ApplyScrubValue(float value)
    {
        GrassAnimator animator = target as GrassAnimator;
        if (animator == null)
            return;

        float clamped = Mathf.Clamp01(value);
        Undo.RecordObject(animator, "Scrub Grass Animator");
        animator.SetNormalizedTime(clamped);
        EditorUtility.SetDirty(animator);
        SyncScrubFields(clamped);
    }

    private void SyncScrubFromAnimator()
    {
        GrassAnimator animator = target as GrassAnimator;
        if (animator == null)
            return;

        SyncScrubFields(animator.NormalizedTime);
    }

    private void SyncScrubFields(float value)
    {
        float clamped = Mathf.Clamp01(value);
        suppressScrubCallbacks = true;
        if (scrubSlider != null)
            scrubSlider.SetValueWithoutNotify(clamped);
        if (scrubField != null)
            scrubField.SetValueWithoutNotify(clamped);
        suppressScrubCallbacks = false;
    }

    private void ReplaceEnumControl(VisualElement root, string rowName, string propertyPath, System.Enum defaultValue)
    {
        var row = root.Q<VisualElement>(rowName);
        if (row == null || row.childCount == 0)
            return;

        // The right-side value slot is the last child in these rows.
        VisualElement valueSlot = row[row.childCount - 1];
        if (valueSlot == null)
            return;

        valueSlot.Clear();

        var enumField = new EnumField(defaultValue)
        {
            bindingPath = propertyPath
        };
        enumField.style.width = 200f;
        enumField.style.right = 5f;
        enumField.Bind(serializedObject);
        valueSlot.Add(enumField);
    }


    private const double RepaintIntervalSeconds = 0.1;
    private double nextRepaintTime;

    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    public override bool RequiresConstantRepaint()
    {
        return false;
    }

    private void OnEditorUpdate()
    {
        GrassAnimator animator = target as GrassAnimator;
        if (animator == null || !animator.IsPlaying)
            return;

        double now = EditorApplication.timeSinceStartup;
        if (now < nextRepaintTime)
            return;

        nextRepaintTime = now + RepaintIntervalSeconds;
        SyncScrubFromAnimator();
        Repaint();
    }

    private void OnSceneGUI()
    {
        GrassAnimator animator = (GrassAnimator)target;
        Transform[] points = animator.PathPoints;
        if (points == null || points.Length == 0)
            return;

        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i];
            if (point == null)
                continue;

            EditorGUI.BeginChangeCheck();
            Vector3 next = Handles.PositionHandle(point.position, point.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(point, "Move Grass Path Point");
                point.position = next;
                EditorUtility.SetDirty(point);
                EditorUtility.SetDirty(animator);
            }
        }
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy | GizmoType.NonSelected | GizmoType.Pickable)]
    private static void DrawAnimatorHierarchyGizmos(GrassAnimator animator, GizmoType gizmoType)
    {
        if (animator == null)
            return;

        Transform active = Selection.activeTransform;
        if (active == null)
            return;

        bool isAnimatorOrChild = active == animator.transform || active.IsChildOf(animator.transform);
        if (!isAnimatorOrChild)
            return;

        animator.DrawAnimatorGizmos();
    }
}
