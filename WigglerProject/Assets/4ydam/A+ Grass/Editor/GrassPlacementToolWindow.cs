using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class GrassPlacementToolWindow : EditorWindow
{
    private enum PlacementMode
    {
        Off,
        Pin,
        Brush,
        Eraser
    }

    private const string WindowTitle = "Grass Placement Tool";
    private const string DocumentationUrl = "https://4ydam.com/assets/a+grass/getting-started/";
    private const string DefaultParentName = "Grass Container";
    private const float DefaultMaxRayDistance = 2000f;
    private const float DefaultBrushRadius = 2f;
    private const float DefaultBrushSpacing = 0.5f;
    private const int DefaultBrushDensity = 3;
    private const float DefaultBrushOverlapSpacing = 0.75f;
    private const float DefaultBrushEdgePadding = 0.3f;
    private const float DefaultNonSurfaceIntersectionPadding = -0.2f;
    private const float DefaultEraseRadius = 2f;
    private const float SurfaceProbeOffset = 3f;
    private const float ThumbnailSize = 64f;
    private const float PreviewSurfaceOffset = 0.03f;
    private const float PreviewCenterLineScale = 0.45f;
    private const float PaletteGridMinHeight = 84f;
    private const float PaletteGridMaxHeight = 420f;
    private const float PaletteResizeHandleHeight = 18f;
    private const float PaletteGridLeftPadding = 8f;
    private const float PaletteGridRightPadding = 8f;
    private const float PaletteGridTopPadding = 8f;
    private const float PaletteCardRemoveButtonSize = 14f;
    private const float PaletteCardRemoveButtonInset = 1f;

    private PlacementMode placementMode = PlacementMode.Off;
    private GrassPrefabLibrary prefabLibrary;
    private Transform placementParent;
    private LayerMask placementMask = ~0;
    private float maxRayDistance = DefaultMaxRayDistance;
    private float brushRadius = DefaultBrushRadius;
    private float brushSpacing = DefaultBrushSpacing;
    private int brushDensity = DefaultBrushDensity;
    private bool preventBrushOverlap = false;
    private bool preventBrushOverlapSelectedPrefabOnly = false;
    private float brushOverlapSpacing = DefaultBrushOverlapSpacing;
    private bool brushEdgePaddingEnabled = false;
    private float brushEdgePadding = DefaultBrushEdgePadding;
    private bool preventNonSurfaceIntersection = true;
    private float nonSurfaceIntersectionPadding = DefaultNonSurfaceIntersectionPadding;
    private float eraseRadius = DefaultEraseRadius;
    private bool eraseSelectedPrefabsOnly = false;
    private bool randomRotation = true;
    private bool randomScale = true;
    private Vector3 randomRotationRangeStart = new Vector3(0f, 0f, 0f);
    private Vector3 randomRotationRangeEnd = new Vector3(0f, 360f, 0f);
    private Vector2 randomScaleRange = new Vector2(0.9f, 1.25f);
    private bool alignToSurfaceNormal = false;
    private float surfaceNormalAlignment = 1f;
    private readonly HashSet<int> selectedPrefabIndices = new HashSet<int>();
    private readonly List<GameObject> validPrefabBuffer = new List<GameObject>();
    private readonly List<GameObject> selectedPrefabBuffer = new List<GameObject>();
    private readonly HashSet<GameObject> selectedPrefabSetBuffer = new HashSet<GameObject>();
    private int selectedPaletteIndex = -1;
    private Vector3 lastStrokePoint;
    private bool strokeActive;
    private Vector2 windowScroll;
    private Vector2 prefabPreviewScroll;
    private float paletteGridHeight = 160f;
    private bool isResizingPaletteGrid;
    private float resizeStartMouseY;
    private float resizeStartHeight;
    private readonly List<GrassPrefabLibrary> availablePalettes = new List<GrassPrefabLibrary>();
    private readonly List<string> availablePaletteNames = new List<string>();
    private Collider[] nonSurfaceOverlapBuffer = new Collider[16];
    private GUIStyle sectionHeaderStyle;
    private static readonly string[] BuiltinTabIconNames =
    {
        "d_FilterByType",
        "FilterByType",
        "d_SceneViewOrtho",
        "SceneViewOrtho",
        "d_Terrain Icon",
        "Terrain Icon"
    };

    [MenuItem("Tools/4ydam/A+ Grass/Grass Placement Tool", false, 20)]
    public static void Open()
    {
        GrassPlacementToolWindow window = GetWindow<GrassPlacementToolWindow>(WindowTitle);
        window.ApplyWindowTitle();
    }

    private void OnEnable()
    {
        ApplyWindowTitle();
        RefreshPaletteList();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        EnsureStyles();

        windowScroll = EditorGUILayout.BeginScrollView(windowScroll);

        AGrassBannerDrawer.DrawBanner();
        DrawHeaderTab();

        DrawPaletteTab();

        if (prefabLibrary == null)
        {
            EditorGUILayout.HelpBox("Create or select a palette to start painting prefabs.", MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }

        DrawLibraryList();

        DrawSectionSeparator();

        EditorGUILayout.Space(8f);
        DrawSectionHeader("Tools");
        DrawToolButtons();

        DrawSectionSeparator();

        EditorGUILayout.Space(8f);
        DrawSectionHeader("General Settings");

        placementParent = (Transform)EditorGUILayout.ObjectField("Parent", placementParent, typeof(Transform), true);
        placementMask = LayerMaskField("Surface Layers", placementMask);

        
        if (placementMode != PlacementMode.Eraser && placementMode != PlacementMode.Off)
        {
            DrawSectionSeparator();

            alignToSurfaceNormal = EditorGUILayout.Toggle("Align To Surface Normal", alignToSurfaceNormal);
            if (alignToSurfaceNormal)
            {
                surfaceNormalAlignment = Mathf.Clamp01(EditorGUILayout.Slider("Normal Alignment", surfaceNormalAlignment, 0f, 1f));
            }
        }

        if (placementMode != PlacementMode.Eraser && placementMode != PlacementMode.Off)
        {
            DrawSectionSeparator();

            randomRotation = EditorGUILayout.Toggle("Random Rotation", randomRotation);
            if (randomRotation)
            {
                randomRotationRangeStart = EditorGUILayout.Vector3Field("Rotation Range Start", randomRotationRangeStart);
                randomRotationRangeEnd = EditorGUILayout.Vector3Field("Rotation Range End", randomRotationRangeEnd);

                randomRotationRangeEnd.x = Mathf.Max(randomRotationRangeStart.x, randomRotationRangeEnd.x);
                randomRotationRangeEnd.y = Mathf.Max(randomRotationRangeStart.y, randomRotationRangeEnd.y);
                randomRotationRangeEnd.z = Mathf.Max(randomRotationRangeStart.z, randomRotationRangeEnd.z);
            }

            randomScale = EditorGUILayout.Toggle("Random Scale", randomScale);
            if (randomScale)
            {
                randomScaleRange = EditorGUILayout.Vector2Field("Scale Range", randomScaleRange);
                randomScaleRange.x = Mathf.Max(0.01f, randomScaleRange.x);
                randomScaleRange.y = Mathf.Max(randomScaleRange.x, randomScaleRange.y);
            }
        }

        bool hasPlacementModeSettings = placementMode == PlacementMode.Brush || placementMode == PlacementMode.Eraser;
        if (hasPlacementModeSettings)
        {
            DrawSectionSeparator();
            DrawSectionHeader(GetPlacementModeLabel(placementMode) + " Settings");
        }

        if (placementMode == PlacementMode.Brush)
        {
            brushRadius = Mathf.Max(0.05f, DrawFloatFieldWithReset("Brush Radius", brushRadius, DefaultBrushRadius));
            brushSpacing = Mathf.Max(0.05f, DrawFloatFieldWithReset("Brush Spacing", brushSpacing, DefaultBrushSpacing));
            brushDensity = Mathf.Clamp(DrawIntFieldWithReset("Brush Density", brushDensity, DefaultBrushDensity), 1, 32);

            DrawSectionSeparator();

            preventBrushOverlap = EditorGUILayout.Toggle("Global Overlap", preventBrushOverlap);
            preventBrushOverlapSelectedPrefabOnly = EditorGUILayout.Toggle("Selected Overlap", preventBrushOverlapSelectedPrefabOnly);
            if (preventBrushOverlap || preventBrushOverlapSelectedPrefabOnly)
            {
                brushOverlapSpacing = Mathf.Max(0f, DrawFloatFieldWithReset("Overlap Spacing", brushOverlapSpacing, DefaultBrushOverlapSpacing));
            }

            DrawSectionSeparator();

            brushEdgePaddingEnabled = EditorGUILayout.Toggle("Edge Padding", brushEdgePaddingEnabled);
            if (brushEdgePaddingEnabled)
            {
                brushEdgePadding = Mathf.Max(0f, DrawFloatFieldWithReset("Edge Padding Distance", brushEdgePadding, DefaultBrushEdgePadding));
            }

            DrawSectionSeparator();

            preventNonSurfaceIntersection = EditorGUILayout.Toggle("Prevent Clipping", preventNonSurfaceIntersection);
            if (preventNonSurfaceIntersection)
            {
                nonSurfaceIntersectionPadding = DrawFloatFieldWithReset("Clipping Padding", nonSurfaceIntersectionPadding, DefaultNonSurfaceIntersectionPadding);
            }
        }
        else if (placementMode == PlacementMode.Eraser)
        {
            eraseRadius = Mathf.Max(0.05f, DrawFloatFieldWithReset("Erase Radius", eraseRadius, DefaultEraseRadius));

            DrawSectionSeparator();

            eraseSelectedPrefabsOnly = EditorGUILayout.Toggle("Selected Prefabs Only", eraseSelectedPrefabsOnly);
        }

        EditorGUILayout.Space(8f);
        if (placementMode == PlacementMode.Off)
        {
            EditorGUILayout.HelpBox("Tool is off. Select Pin, Brush, or Eraser to get started.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Use left click in Scene view to paint. Hold and drag for brush/eraser.", MessageType.None);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawPaletteTab()
    {
        DrawSectionHeader("Palette");

        EditorGUILayout.BeginVertical("box");
        using (new EditorGUILayout.HorizontalScope())
        {
            if (availablePalettes.Count == 0)
            {
                GUILayout.Label("No palettes found", EditorStyles.miniLabel);
            }
            else
            {
                int currentIndex = Mathf.Clamp(selectedPaletteIndex, 0, availablePalettes.Count - 1);
                int nextIndex = EditorGUILayout.Popup(currentIndex, availablePaletteNames.ToArray());
                if (nextIndex != currentIndex)
                {
                    selectedPaletteIndex = nextIndex;
                    prefabLibrary = availablePalettes[selectedPaletteIndex];
                    ResetSelection();
                }
            }

            if (GUILayout.Button("New", GUILayout.Width(56f)))
            {
                GrassPrefabLibrary created = CreateNewLibraryAsset();
                RefreshPaletteList();
                if (created != null)
                {
                    prefabLibrary = created;
                    selectedPaletteIndex = availablePalettes.IndexOf(created);
                    ResetSelection();
                }
            }

            if (GUILayout.Button("Refresh", GUILayout.Width(68f)))
            {
                RefreshPaletteList();
            }

            using (new EditorGUI.DisabledScope(prefabLibrary == null))
            {
                if (GUILayout.Button("Remove", GUILayout.Width(68f)))
                {
                    DeleteSelectedLibraryAsset();
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void RefreshPaletteList()
    {
        availablePalettes.Clear();
        availablePaletteNames.Clear();

        string[] guids = AssetDatabase.FindAssets("t:GrassPrefabLibrary");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GrassPrefabLibrary palette = AssetDatabase.LoadAssetAtPath<GrassPrefabLibrary>(path);
            if (palette == null)
                continue;

            // Ensure palette assets remain visible in the Project window.
            if (palette.hideFlags != HideFlags.None)
            {
                palette.hideFlags = HideFlags.None;
                EditorUtility.SetDirty(palette);
            }

            availablePalettes.Add(palette);
            availablePaletteNames.Add(palette.name);
        }

        if (availablePalettes.Count == 0)
        {
            prefabLibrary = null;
            selectedPaletteIndex = -1;
            return;
        }

        if (prefabLibrary != null)
        {
            int existingIndex = availablePalettes.IndexOf(prefabLibrary);
            if (existingIndex >= 0)
            {
                selectedPaletteIndex = existingIndex;
                return;
            }
        }

        selectedPaletteIndex = Mathf.Clamp(selectedPaletteIndex, 0, availablePalettes.Count - 1);
        prefabLibrary = availablePalettes[selectedPaletteIndex];
        PopulateValidPrefabs(validPrefabBuffer);
        SanitizeSelection(validPrefabBuffer.Count);
    }

    private void DrawHeaderTab()
    {
        Rect tabRect = GUILayoutUtility.GetRect(10f, 40f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(tabRect, new Color(0.14f, 0.14f, 0.14f, 0.95f));

        const float horizontalPadding = 8f;
        const float verticalPadding = 6f;
        Rect contentRect = new Rect(
            tabRect.x + horizontalPadding,
            tabRect.y + verticalPadding,
            tabRect.width - horizontalPadding * 2f,
            tabRect.height - verticalPadding * 2f);

        Rect infoRect = new Rect(contentRect.x, contentRect.y + 3f, 16f, 16f);

        Handles.BeginGUI();
        Color previousHandlesColor = Handles.color;
        Handles.color = new Color(0.86f, 0.86f, 0.86f, 1f);
        Handles.DrawSolidDisc(infoRect.center, Vector3.forward, infoRect.width * 0.5f);
        Handles.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        Handles.DrawWireDisc(infoRect.center, Vector3.forward, infoRect.width * 0.5f);
        Handles.color = previousHandlesColor;
        Handles.EndGUI();

        GUIStyle infoStyle = new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };
        infoStyle.normal.textColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        GUI.Label(infoRect, "i", infoStyle);
        GUI.Label(infoRect, new GUIContent(string.Empty, "Grass Placement Tool uses selected prefabs to spawn in set world space."), GUIStyle.none);

        Rect titleRect = new Rect(infoRect.xMax + 8f, contentRect.y + 2f, contentRect.width - 136f, 20f);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold
        };
        Color titleColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        titleStyle.normal.textColor = titleColor;
        titleStyle.hover.textColor = titleColor;
        titleStyle.active.textColor = titleColor;
        titleStyle.focused.textColor = titleColor;
        GUI.Label(titleRect, WindowTitle, titleStyle);

        Rect docsRect = new Rect(tabRect.xMax - 112f - horizontalPadding, contentRect.y + 1f, 104f, 22f);
        if (GUI.Button(docsRect, "Documentation"))
        {
            Application.OpenURL(DocumentationUrl);
        }
    }

    private static void DrawSectionSeparator()
    {
        EditorGUILayout.Space(6f);
        Rect separatorRect = EditorGUILayout.GetControlRect(false, 1f);
        EditorGUI.DrawRect(separatorRect, new Color(0.28f, 0.28f, 0.28f, 1f));
        EditorGUILayout.Space(6f);
    }

    private void EnsureStyles()
    {
        sectionHeaderStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            margin = new RectOffset(8, 0, 5, 8)
        };

        sectionHeaderStyle.normal.textColor = Color.white;
        sectionHeaderStyle.hover.textColor = Color.white;
        sectionHeaderStyle.active.textColor = Color.white;
        sectionHeaderStyle.focused.textColor = Color.white;
        sectionHeaderStyle.onNormal.textColor = Color.white;
        sectionHeaderStyle.onHover.textColor = Color.white;
        sectionHeaderStyle.onActive.textColor = Color.white;
        sectionHeaderStyle.onFocused.textColor = Color.white;
    }

    private void DrawSectionHeader(string label)
    {
        GUILayout.Label(label, sectionHeaderStyle);
    }

    private void ApplyWindowTitle()
    {
        Texture tabIcon = ResolveBuiltinTabIcon();
        titleContent = new GUIContent(WindowTitle, tabIcon);
    }

    private static Texture ResolveBuiltinTabIcon()
    {
        for (int i = 0; i < BuiltinTabIconNames.Length; i++)
        {
            string iconName = BuiltinTabIconNames[i];
            GUIContent iconContent = EditorGUIUtility.IconContent(iconName);
            if (iconContent != null && iconContent.image != null)
            {
                return iconContent.image;
            }

            Texture texture = EditorGUIUtility.FindTexture(iconName);
            if (texture != null)
            {
                return texture;
            }
        }

        return null;
    }

    private static string GetPlacementModeLabel(PlacementMode mode)
    {
        switch (mode)
        {
            case PlacementMode.Pin:
                return "Pin";
            case PlacementMode.Brush:
                return "Brush";
            case PlacementMode.Eraser:
                return "Eraser";
            default:
                return "Tool";
        }
    }

    private void DrawToolButtons()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawToolToggleButton(PlacementMode.Pin, "Pin");
            DrawToolToggleButton(PlacementMode.Brush, "Brush");
            DrawToolToggleButton(PlacementMode.Eraser, "Eraser");
        }
    }

    private void DrawToolToggleButton(PlacementMode mode, string label)
    {
        bool isSelected = placementMode == mode;
        bool next = GUILayout.Toggle(isSelected, label, EditorStyles.miniButton, GUILayout.Height(24f));

        if (next != isSelected)
        {
            placementMode = next ? mode : PlacementMode.Off;
            GUI.FocusControl(null);
        }
    }

    private void DrawLibraryList()
    {
        if (prefabLibrary.prefabs == null)
        {
            prefabLibrary.prefabs = new List<GameObject>();
            EditorUtility.SetDirty(prefabLibrary);
        }

        EditorGUILayout.Space(4f);

        Rect dropRect = GUILayoutUtility.GetRect(10f, 44f, GUILayout.ExpandWidth(true));
        bool isDragging = dropRect.Contains(Event.current.mousePosition) &&
            (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform);
        EditorGUI.DrawRect(dropRect, isDragging ? new Color(0.20f, 0.55f, 0.22f, 0.45f) : new Color(0.13f, 0.13f, 0.13f, 0.85f));
        GUI.Label(dropRect, "Drag and drop prefab assets here to add to palette", EditorStyles.centeredGreyMiniLabel);
        HandleQuickSelectDragAndDrop(dropRect);

        PopulateValidPrefabs(validPrefabBuffer);
        List<GameObject> validPrefabs = validPrefabBuffer;
        if (validPrefabs.Count == 0)
        {
            EditorGUILayout.HelpBox("Drop grass prefabs into the palette area above.", MessageType.Info);
            return;
        }

        SanitizeSelection(validPrefabs.Count);

        float cellSpacing = 10f;
        float cellSize = ThumbnailSize + cellSpacing;

        Rect gridRect = GUILayoutUtility.GetRect(10f, paletteGridHeight, GUILayout.ExpandWidth(true));
        float scrollbarWidth = GUI.skin.verticalScrollbar.fixedWidth > 0f ? GUI.skin.verticalScrollbar.fixedWidth : 15f;
        float availableWidth = Mathf.Max(1f, gridRect.width - PaletteGridLeftPadding - PaletteGridRightPadding - scrollbarWidth);
        int columns = Mathf.Max(1, Mathf.FloorToInt(availableWidth / cellSize));
        int rows = Mathf.CeilToInt(validPrefabs.Count / (float)columns);
        float contentHeight = PaletteGridTopPadding + rows * cellSize;
        float contentWidth = PaletteGridLeftPadding + columns * cellSize + PaletteGridRightPadding;
        Rect viewRect = new Rect(0f, 0f, contentWidth, contentHeight);

        prefabPreviewScroll.x = 0f;
        prefabPreviewScroll = GUI.BeginScrollView(gridRect, prefabPreviewScroll, viewRect, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);
        prefabPreviewScroll.x = 0f;

        for (int i = 0; i < validPrefabs.Count; i++)
        {
            int row = i / columns;
            int col = i % columns;
            float x = PaletteGridLeftPadding + col * cellSize;
            float y = PaletteGridTopPadding + row * cellSize;
            Rect cardRect = new Rect(x, y, ThumbnailSize, ThumbnailSize + 2f);
            DrawQuickSelectCard(validPrefabs, i, cardRect);
        }

        GUI.EndScrollView();

        DrawPaletteGridResizeHandle();

    EditorGUILayout.Space(6f);

        Rect footerRect = EditorGUILayout.GetControlRect(false, 22f);
        Rect countRect = new Rect(footerRect.x, footerRect.y, 90f, footerRect.height);
        Rect clearPaletteRect = new Rect(footerRect.xMax - 110f, footerRect.y, 110f, footerRect.height);
        Rect separatorRect = new Rect(clearPaletteRect.x - 12f, footerRect.y + 2f, 1f, footerRect.height - 4f);
        Rect clearSelectionRect = new Rect(separatorRect.x - 110f - 12f, footerRect.y, 110f, footerRect.height);

        GUI.Label(countRect, $"Selected: {selectedPrefabIndices.Count}", EditorStyles.miniLabel);

        if (GUI.Button(clearSelectionRect, "Clear Selection"))
        {
            selectedPrefabIndices.Clear();
        }

        DrawInlineSeparator(separatorRect);

        if (GUI.Button(clearPaletteRect, "Clear Palette"))
        {
            Undo.RecordObject(prefabLibrary, "Clear Grass Prefab Palette");
            prefabLibrary.prefabs.Clear();
            selectedPrefabIndices.Clear();
            EditorUtility.SetDirty(prefabLibrary);
        }
    }

    private void DrawQuickSelectCard(List<GameObject> prefabs, int index, Rect cardRect)
    {
        GameObject prefab = prefabs[index];
        Texture2D preview = GetPrefabPreviewTexture(prefab);
        Event currentEvent = Event.current;

        Rect thumbRect = new Rect(cardRect.x + 1f, cardRect.y + 1f, ThumbnailSize - 2f, ThumbnailSize - 2f);
        Rect removeRect = new Rect(
            thumbRect.xMax - PaletteCardRemoveButtonSize - PaletteCardRemoveButtonInset,
            thumbRect.y + PaletteCardRemoveButtonInset,
            PaletteCardRemoveButtonSize,
            PaletteCardRemoveButtonSize);
        bool isSelected = selectedPrefabIndices.Contains(index);

        Color cardColor = new Color(0.22f, 0.22f, 0.22f, 0.95f);
        EditorGUI.DrawRect(cardRect, cardColor);

        if (isSelected)
        {
            DrawCardOutline(cardRect, new Color(0.2f, 0.65f, 0.25f, 0.95f), 2f);
        }

        GUI.DrawTexture(thumbRect, preview != null ? (Texture)preview : Texture2D.grayTexture, ScaleMode.ScaleAndCrop);
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && cardRect.Contains(currentEvent.mousePosition) && !removeRect.Contains(currentEvent.mousePosition))
        {
            ToggleSelection(index);
            GUI.FocusControl(null);
            currentEvent.Use();
        }

        EditorGUI.DrawRect(removeRect, new Color(0.2f, 0.2f, 0.2f, 0.98f));

        GUIStyle removeLabelStyle = new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 9
        };
        removeLabelStyle.normal.textColor = Color.white;
        GUI.Label(removeRect, "x", removeLabelStyle);

        if (GUI.Button(removeRect, GUIContent.none, GUIStyle.none))
        {
            RemovePrefabFromLibrary(prefab);
            GUIUtility.ExitGUI();
            return;
        }
    }

    private void DrawPaletteGridResizeHandle()
    {
        Rect handleRect = GUILayoutUtility.GetRect(12f, PaletteResizeHandleHeight, GUILayout.ExpandWidth(true));
        Rect gripRect = new Rect(handleRect.x + 6f, handleRect.y + 2f, handleRect.width - 12f, handleRect.height - 4f);
        Rect labelRect = new Rect(gripRect.x + 10f, gripRect.y, gripRect.width - 20f, gripRect.height);

        bool isHovering = gripRect.Contains(Event.current.mousePosition);
        Color gripFill = isHovering ? new Color(0.23f, 0.23f, 0.23f, 0.95f) : new Color(0.18f, 0.18f, 0.18f, 0.9f);
        Color gripOutline = new Color(0.38f, 0.38f, 0.38f, 1f);

        EditorGUI.DrawRect(gripRect, gripFill);
        EditorGUI.DrawRect(new Rect(gripRect.x, gripRect.y, gripRect.width, 1f), gripOutline);
        EditorGUI.DrawRect(new Rect(gripRect.x, gripRect.yMax - 1f, gripRect.width, 1f), gripOutline);

        GUIStyle gripLabelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };
        gripLabelStyle.normal.textColor = new Color(0.72f, 0.72f, 0.72f, 1f);

        Vector2 labelSize = gripLabelStyle.CalcSize(new GUIContent("Drag to resize"));
        float linePadding = 10f;
        float lineY = gripRect.center.y - 1f;
        float lineHeight = 2f;
        float leftLineWidth = Mathf.Max(0f, labelRect.center.x - labelSize.x * 0.5f - linePadding - (gripRect.x + 8f));
        float rightLineX = labelRect.center.x + labelSize.x * 0.5f + linePadding;
        float rightLineWidth = Mathf.Max(0f, gripRect.xMax - 8f - rightLineX);

        if (leftLineWidth > 0f)
        {
            EditorGUI.DrawRect(new Rect(gripRect.x + 8f, lineY, leftLineWidth, lineHeight), gripOutline);
        }

        if (rightLineWidth > 0f)
        {
            EditorGUI.DrawRect(new Rect(rightLineX, lineY, rightLineWidth, lineHeight), gripOutline);
        }

        GUI.Label(labelRect, "Drag to resize", gripLabelStyle);

        EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeVertical);

        Event evt = Event.current;
        if (evt == null)
            return;

        if (!isResizingPaletteGrid && evt.type == EventType.MouseDown && evt.button == 0 && gripRect.Contains(evt.mousePosition))
        {
            isResizingPaletteGrid = true;
            resizeStartMouseY = evt.mousePosition.y;
            resizeStartHeight = paletteGridHeight;
            evt.Use();
            return;
        }

        if (isResizingPaletteGrid && evt.type == EventType.MouseDrag)
        {
            float delta = evt.mousePosition.y - resizeStartMouseY;
            paletteGridHeight = Mathf.Clamp(resizeStartHeight + delta, PaletteGridMinHeight, PaletteGridMaxHeight);
            Repaint();
            evt.Use();
            return;
        }

        if (isResizingPaletteGrid && (evt.type == EventType.MouseUp || evt.rawType == EventType.MouseUp))
        {
            isResizingPaletteGrid = false;
            evt.Use();
        }
    }

    private void HandleQuickSelectDragAndDrop(Rect dropRect)
    {
        Event evt = Event.current;
        if (!dropRect.Contains(evt.mousePosition))
            return;

        if (evt.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
            return;
        }

        if (evt.type != EventType.DragPerform)
            return;

        DragAndDrop.AcceptDrag();
        Undo.RecordObject(prefabLibrary, "Add Grass Prefabs To Palette");

        bool changed = false;
        bool selectionChanged = false;
        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
        {
            Object dragged = DragAndDrop.objectReferences[i];
            if (dragged is GameObject prefab && PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                if (!prefabLibrary.prefabs.Contains(prefab))
                {
                    prefabLibrary.prefabs.Add(prefab);
                    changed = true;
                    selectedPrefabIndices.Add(prefabLibrary.prefabs.Count - 1);
                    selectionChanged = true;
                }
            }
        }

        if (changed || selectionChanged)
        {
            EditorUtility.SetDirty(prefabLibrary);
            PopulateValidPrefabs(validPrefabBuffer);
            SanitizeSelection(validPrefabBuffer.Count);
        }

        evt.Use();
    }

    private void RemovePrefabFromLibrary(GameObject prefab)
    {
        if (prefabLibrary == null || prefab == null)
            return;

        Undo.RecordObject(prefabLibrary, "Remove Grass Prefab From Palette");
        int removedIndex = prefabLibrary.prefabs.IndexOf(prefab);
        prefabLibrary.prefabs.Remove(prefab);
        if (removedIndex >= 0)
        {
            ShiftSelectionAfterRemoval(removedIndex);
        }

        PopulateValidPrefabs(validPrefabBuffer);
        SanitizeSelection(validPrefabBuffer.Count);
        EditorUtility.SetDirty(prefabLibrary);
    }

    private Texture2D GetPrefabPreviewTexture(GameObject prefab)
    {
        if (prefab == null)
            return null;

        Texture2D preview = AssetPreview.GetAssetPreview(prefab);
        if (preview != null)
            return preview;

        preview = AssetPreview.GetMiniThumbnail(prefab);
        if (AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID()))
        {
            Repaint();
        }

        return preview;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (prefabLibrary == null)
            return;

        if (placementMode == PlacementMode.Off)
            return;

        Event current = Event.current;
        if (current == null)
            return;

        // Only participate in relevant SceneView events.
        switch (current.type)
        {
            case EventType.Layout:
            case EventType.Repaint:
            case EventType.MouseMove:
            case EventType.MouseDown:
            case EventType.MouseDrag:
            case EventType.MouseUp:
                break;
            default:
                return;
        }

        if (current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            return;
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, placementMask.value))
            return;

        if (current.type == EventType.Repaint || current.type == EventType.MouseMove)
        {
            DrawBrushPreview(hit);
        }

        if (current.alt)
            return;

        if (current.type == EventType.MouseDown && current.button == 0)
        {
            strokeActive = true;
            lastStrokePoint = hit.point;
            ExecutePaint(hit, forceSingle: placementMode == PlacementMode.Pin);
            current.Use();
            return;
        }

        if (current.type == EventType.MouseDrag && current.button == 0 && strokeActive)
        {
            if (placementMode == PlacementMode.Pin)
            {
                current.Use();
                return;
            }

            float minStep = placementMode == PlacementMode.Eraser ? eraseRadius * 0.25f : brushSpacing;
            if (Vector3.Distance(hit.point, lastStrokePoint) >= Mathf.Max(0.05f, minStep))
            {
                ExecutePaint(hit, forceSingle: false);
                lastStrokePoint = hit.point;
            }

            current.Use();
            return;
        }

        if ((current.type == EventType.MouseUp || current.rawType == EventType.MouseUp) && current.button == 0)
        {
            strokeActive = false;
        }
    }

    private void DrawBrushPreview(RaycastHit hit)
    {
        Color fill = new Color(0.1f, 0.75f, 0.1f, 0.08f);
        Color wire = new Color(0.1f, 1f, 0.1f, 0.9f);
        Vector3 previewCenter = hit.point + hit.normal * PreviewSurfaceOffset;

        if (placementMode == PlacementMode.Eraser)
        {
            fill = new Color(0.85f, 0.15f, 0.15f, 0.08f);
            wire = new Color(1f, 0.25f, 0.25f, 0.95f);
        }

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        float centerLineLength = HandleUtility.GetHandleSize(previewCenter) * PreviewCenterLineScale;
        Handles.color = wire;
        Handles.DrawLine(previewCenter, previewCenter + Vector3.up * centerLineLength);

        if (placementMode == PlacementMode.Pin || placementMode == PlacementMode.Brush)
        {
            DrawPlacementMeshPreview(hit);
        }

        if (placementMode == PlacementMode.Pin)
        {
            Handles.SphereHandleCap(0, previewCenter, Quaternion.identity, HandleUtility.GetHandleSize(previewCenter) * 0.1f, EventType.Repaint);
        }
        else
        {
            float radius = placementMode == PlacementMode.Eraser ? eraseRadius : brushRadius;
            Handles.color = fill;
            Handles.DrawSolidDisc(previewCenter, hit.normal, radius);
            Handles.color = wire;
            Handles.DrawWireDisc(previewCenter, hit.normal, radius);
        }
    }

    private void DrawPlacementMeshPreview(RaycastHit hit)
    {
        if (Event.current == null || Event.current.type != EventType.Repaint)
            return;

        Camera previewCamera = SceneView.currentDrawingSceneView != null
            ? SceneView.currentDrawingSceneView.camera
            : Camera.current;
        if (previewCamera == null)
            return;

        PopulateValidPrefabs(validPrefabBuffer);
        PopulateSelectedPrefabs(validPrefabBuffer, selectedPrefabBuffer);
        List<GameObject> selectedPrefabs = selectedPrefabBuffer;
        if (selectedPrefabs.Count == 0)
            return;

        Vector3 seedPoint = GetPreviewSeedPoint(hit.point);
        int seed = GetPreviewSeedFromPoint(seedPoint);
        Random.State previousState = Random.state;
        Random.InitState(seed);

        if (placementMode == PlacementMode.Pin)
        {
            bool randomPick = selectedPrefabs.Count > 1;
            GameObject previewPrefab = SelectPrefab(selectedPrefabs, randomPick);
            DrawPreviewInstance(previewPrefab, hit.point, hit.normal, useRandomizedTransform: false, previewCamera);
            Random.state = previousState;
            return;
        }

        int previewCount = Mathf.Max(1, brushDensity);
        for (int i = 0; i < previewCount; i++)
        {
            Vector2 circle = Random.insideUnitCircle * brushRadius;
            Vector3 offset = GetPlaneOffset(hit.normal, circle);
            Vector3 samplePoint = hit.point + offset;

            if (TryFindSurfaceNearPoint(samplePoint, hit.normal, out RaycastHit sampleHit))
            {
                if (brushEdgePaddingEnabled && IsTooCloseToSurfaceEdge(sampleHit, brushEdgePadding))
                    continue;

                GameObject previewPrefab = SelectPrefab(selectedPrefabs, true);
                DrawPreviewInstance(previewPrefab, sampleHit.point, sampleHit.normal, useRandomizedTransform: true, previewCamera);
            }
        }

        Random.state = previousState;
    }

    private Quaternion GetPreviewRotation(Vector3 normal, bool useRandomizedTransform)
    {
        Quaternion rotation = Quaternion.identity;
        if (alignToSurfaceNormal)
        {
            Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, normal.normalized);
            rotation = Quaternion.Slerp(Quaternion.identity, normalRotation, surfaceNormalAlignment);
        }

        if (randomRotation && useRandomizedTransform)
        {
            Vector3 previewEuler = new Vector3(
                Random.Range(randomRotationRangeStart.x, randomRotationRangeEnd.x),
                Random.Range(randomRotationRangeStart.y, randomRotationRangeEnd.y),
                Random.Range(randomRotationRangeStart.z, randomRotationRangeEnd.z));
            rotation *= Quaternion.Euler(previewEuler);
        }

        return rotation;
    }

    private float GetPreviewScaleMultiplier(bool useRandomizedTransform)
    {
        if (!randomScale)
            return 1f;

        if (!useRandomizedTransform)
            return (randomScaleRange.x + randomScaleRange.y) * 0.5f;

        return Random.Range(randomScaleRange.x, randomScaleRange.y);
    }

    private void DrawPreviewInstance(GameObject prefab, Vector3 position, Vector3 normal, bool useRandomizedTransform, Camera previewCamera)
    {
        if (prefab == null)
            return;

        Quaternion previewRotation = GetPreviewRotation(normal, useRandomizedTransform);
        float previewScaleMultiplier = GetPreviewScaleMultiplier(useRandomizedTransform);
        Vector3 previewScale = Vector3.one * previewScaleMultiplier;
        DrawPrefabRenderedRecursive(prefab.transform, position, previewRotation, previewScale, previewCamera);
    }

    private void DrawPrefabRenderedRecursive(Transform source, Vector3 parentPosition, Quaternion parentRotation, Vector3 parentScale, Camera previewCamera)
    {
        Vector3 localOffset = Vector3.Scale(source.localPosition, parentScale);
        Vector3 worldPosition = parentPosition + parentRotation * localOffset;
        Quaternion worldRotation = parentRotation * source.localRotation;
        Vector3 worldScale = Vector3.Scale(parentScale, source.localScale);

        MeshFilter meshFilter = source.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = source.GetComponent<MeshRenderer>();
        if (meshFilter != null && meshRenderer != null && meshFilter.sharedMesh != null && meshRenderer.enabled)
        {
            Mesh mesh = meshFilter.sharedMesh;
            Matrix4x4 matrix = Matrix4x4.TRS(worldPosition, worldRotation, worldScale);
            Material[] materials = meshRenderer.sharedMaterials;
            int subMeshCount = Mathf.Min(mesh.subMeshCount, materials != null ? materials.Length : 0);

            for (int subMesh = 0; subMesh < subMeshCount; subMesh++)
            {
                Material material = materials[subMesh];
                if (material == null)
                    continue;

                Graphics.DrawMesh(
                    mesh,
                    matrix,
                    material,
                    0,
                    previewCamera,
                    subMesh,
                    null,
                    UnityEngine.Rendering.ShadowCastingMode.Off,
                    false);
            }
        }

        for (int i = 0; i < source.childCount; i++)
        {
            DrawPrefabRenderedRecursive(source.GetChild(i), worldPosition, worldRotation, worldScale, previewCamera);
        }
    }

    private Vector3 GetPreviewSeedPoint(Vector3 point)
    {
        float snapSize;
        if (placementMode == PlacementMode.Brush)
        {
            float spacingSnap = Mathf.Max(0.05f, brushSpacing * 0.5f);
            float radiusSnap = Mathf.Max(0.05f, brushRadius * 0.15f);
            snapSize = Mathf.Max(spacingSnap, radiusSnap);
        }
        else
        {
            snapSize = 0.1f;
        }

        return new Vector3(
            Mathf.Round(point.x / snapSize) * snapSize,
            Mathf.Round(point.y / snapSize) * snapSize,
            Mathf.Round(point.z / snapSize) * snapSize);
    }

    private static int GetPreviewSeedFromPoint(Vector3 point)
    {
        unchecked
        {
            int x = Mathf.RoundToInt(point.x * 100f);
            int y = Mathf.RoundToInt(point.y * 100f);
            int z = Mathf.RoundToInt(point.z * 100f);
            int seed = 17;
            seed = seed * 31 + x;
            seed = seed * 31 + y;
            seed = seed * 31 + z;
            return seed;
        }
    }

    private void ExecutePaint(RaycastHit hit, bool forceSingle)
    {
        if (placementMode == PlacementMode.Eraser)
        {
            EraseAt(hit.point, eraseRadius);
            return;
        }

        Vector3 seedPoint = GetPreviewSeedPoint(hit.point);
        int seed = GetPreviewSeedFromPoint(seedPoint);
        Random.State previousState = Random.state;
        Random.InitState(seed);

        PopulateValidPrefabs(validPrefabBuffer);
        PopulateSelectedPrefabs(validPrefabBuffer, selectedPrefabBuffer);
        List<GameObject> selectedPrefabs = selectedPrefabBuffer;
        if (selectedPrefabs.Count == 0)
        {
            Random.state = previousState;
            return;
        }

        if (placementMode == PlacementMode.Pin || forceSingle)
        {
            bool randomPick = selectedPrefabs.Count > 1 || placementMode == PlacementMode.Brush;
            GameObject prefab = SelectPrefab(selectedPrefabs, randomPick);
            PlaceSingle(prefab, hit.point, hit.normal);
            Random.state = previousState;
            return;
        }

        int count = Mathf.Max(1, brushDensity);
        for (int i = 0; i < count; i++)
        {
            Vector2 circle = Random.insideUnitCircle * brushRadius;
            Vector3 offset = GetPlaneOffset(hit.normal, circle);
            Vector3 samplePoint = hit.point + offset;

            if (TryFindSurfaceNearPoint(samplePoint, hit.normal, out RaycastHit sampleHit))
            {
                if (brushEdgePaddingEnabled && IsTooCloseToSurfaceEdge(sampleHit, brushEdgePadding))
                    continue;

                GameObject prefab = SelectPrefab(selectedPrefabs, true);
                bool blockedByGlobal = preventBrushOverlap && HasOverlapWithPlacedInstances(sampleHit.point, brushOverlapSpacing, null);
                bool blockedBySelectedType = preventBrushOverlapSelectedPrefabOnly && HasOverlapWithPlacedInstances(sampleHit.point, brushOverlapSpacing, prefab);
                if (blockedByGlobal || blockedBySelectedType)
                    continue;

                PlaceSingle(prefab, sampleHit.point, sampleHit.normal);
            }
        }

        Random.state = previousState;
    }

    private GameObject SelectPrefab(List<GameObject> prefabs, bool randomPick)
    {
        if (prefabs.Count == 1)
            return prefabs[0];

        if (!randomPick)
        {
            return prefabs[0];
        }

        int index = Random.Range(0, prefabs.Count);
        return prefabs[index];
    }

    private void PlaceSingle(GameObject prefab, Vector3 position, Vector3 normal)
    {
        if (prefab == null)
            return;

        Quaternion rotation = Quaternion.identity;
        if (alignToSurfaceNormal)
        {
            Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, normal.normalized);
            rotation = Quaternion.Slerp(Quaternion.identity, normalRotation, surfaceNormalAlignment);
        }

        if (randomRotation)
        {
            Vector3 randomEuler = new Vector3(
                Random.Range(randomRotationRangeStart.x, randomRotationRangeEnd.x),
                Random.Range(randomRotationRangeStart.y, randomRotationRangeEnd.y),
                Random.Range(randomRotationRangeStart.z, randomRotationRangeEnd.z));
            rotation *= Quaternion.Euler(randomEuler);
        }

        float uniformScale = 1f;
        if (randomScale)
        {
            uniformScale = Random.Range(randomScaleRange.x, randomScaleRange.y);
        }

        if (placementMode == PlacementMode.Brush && preventNonSurfaceIntersection && WouldIntersectNonSurfaceGeometry(prefab, position, rotation, uniformScale, nonSurfaceIntersectionPadding))
            return;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (instance == null)
            return;

        Undo.RegisterCreatedObjectUndo(instance, "Paint Grass Prefab");

        Transform parent = EnsurePlacementParent();
        if (parent != null)
        {
            Undo.SetTransformParent(instance.transform, parent, "Parent Grass Prefab");
        }

        instance.transform.SetPositionAndRotation(position, rotation);

        if (randomScale)
        {
            instance.transform.localScale *= uniformScale;
        }

        if (instance.GetComponent<GrassPlacementInstanceTag>() == null)
        {
            Undo.AddComponent<GrassPlacementInstanceTag>(instance);
        }

        Selection.activeObject = instance;
        EditorSceneManager.MarkSceneDirty(instance.scene);
    }

    private bool WouldIntersectNonSurfaceGeometry(GameObject prefab, Vector3 position, Quaternion rotation, float uniformScale, float padding)
    {
        if (prefab == null)
            return false;

        Matrix4x4 rootMatrix = Matrix4x4.TRS(position, rotation, Vector3.one * uniformScale);
        return WouldIntersectNonSurfaceGeometryRecursive(prefab.transform, rootMatrix, padding);
    }

    private bool WouldIntersectNonSurfaceGeometryRecursive(Transform source, Matrix4x4 parentMatrix, float padding)
    {
        Matrix4x4 localMatrix = Matrix4x4.TRS(source.localPosition, source.localRotation, source.localScale);
        Matrix4x4 worldMatrix = parentMatrix * localMatrix;

        MeshFilter meshFilter = source.GetComponent<MeshFilter>();
        Renderer renderer = source.GetComponent<Renderer>();
        if (meshFilter != null && meshFilter.sharedMesh != null && (renderer == null || renderer.enabled))
        {
            if (MeshBoundsIntersectsNonSurface(meshFilter.sharedMesh.bounds, worldMatrix, padding))
                return true;
        }

        for (int i = 0; i < source.childCount; i++)
        {
            if (WouldIntersectNonSurfaceGeometryRecursive(source.GetChild(i), worldMatrix, padding))
                return true;
        }

        return false;
    }

    private bool MeshBoundsIntersectsNonSurface(Bounds localBounds, Matrix4x4 worldMatrix, float padding)
    {
        Vector3 center = worldMatrix.MultiplyPoint3x4(localBounds.center);

        Vector3 axisX = worldMatrix.MultiplyVector(Vector3.right);
        Vector3 axisY = worldMatrix.MultiplyVector(Vector3.up);
        Vector3 axisZ = worldMatrix.MultiplyVector(Vector3.forward);

        Vector3 extents = new Vector3(
            localBounds.extents.x * axisX.magnitude,
            localBounds.extents.y * axisY.magnitude,
            localBounds.extents.z * axisZ.magnitude);
        extents += Vector3.one * padding;
        extents.x = Mathf.Max(0.005f, extents.x);
        extents.y = Mathf.Max(0.005f, extents.y);
        extents.z = Mathf.Max(0.005f, extents.z);

        Quaternion orientation = Quaternion.LookRotation(
            axisZ.sqrMagnitude > 0.000001f ? axisZ.normalized : Vector3.forward,
            axisY.sqrMagnitude > 0.000001f ? axisY.normalized : Vector3.up);

        int overlapCount = CollectNonSurfaceOverlaps(center, extents, orientation);
        for (int i = 0; i < overlapCount; i++)
        {
            Collider collider = nonSurfaceOverlapBuffer[i];
            if (collider == null)
                continue;

            if (IsSurfaceLayer(collider.gameObject.layer))
                continue;

            if (collider.GetComponentInParent<GrassPlacementInstanceTag>() != null)
                continue;

            return true;
        }

        return false;
    }

    private bool IsSurfaceLayer(int layer)
    {
        return (placementMask.value & (1 << layer)) != 0;
    }

    private void EraseAt(Vector3 center, float radius)
    {
        Transform parent = EnsurePlacementParent();
        if (parent == null)
            return;

        HashSet<GameObject> selectedPrefabs = null;
        if (eraseSelectedPrefabsOnly)
        {
            PopulateValidPrefabs(validPrefabBuffer);
            PopulateSelectedPrefabs(validPrefabBuffer, selectedPrefabBuffer);
            selectedPrefabSetBuffer.Clear();
            for (int i = 0; i < selectedPrefabBuffer.Count; i++)
            {
                selectedPrefabSetBuffer.Add(selectedPrefabBuffer[i]);
            }

            if (selectedPrefabSetBuffer.Count == 0)
                return;

            selectedPrefabs = selectedPrefabSetBuffer;
        }

        float radiusSqr = radius * radius;
        List<GameObject> toDelete = new List<GameObject>();

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child == null)
                continue;

            GrassPlacementInstanceTag tag = child.GetComponent<GrassPlacementInstanceTag>();
            if (tag == null)
                continue;

            if (selectedPrefabs != null)
            {
                GameObject sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject) as GameObject;
                if (sourcePrefab == null || !selectedPrefabs.Contains(sourcePrefab))
                    continue;
            }

            Vector3 delta = child.position - center;
            if (delta.sqrMagnitude <= radiusSqr)
            {
                toDelete.Add(child.gameObject);
            }
        }

        if (toDelete.Count == 0)
            return;

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Erase Grass Prefabs");

        for (int i = 0; i < toDelete.Count; i++)
        {
            Undo.DestroyObjectImmediate(toDelete[i]);
        }

        Undo.CollapseUndoOperations(undoGroup);

        if (parent.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(parent.gameObject.scene);
        }
    }

    private Transform EnsurePlacementParent()
    {
        if (placementParent != null)
            return placementParent;

        GameObject existing = GameObject.Find(DefaultParentName);
        if (existing != null)
        {
            placementParent = existing.transform;
            return placementParent;
        }

        GameObject created = new GameObject(DefaultParentName);
        Undo.RegisterCreatedObjectUndo(created, "Create Grass Placement Root");
        placementParent = created.transform;
        EditorSceneManager.MarkSceneDirty(created.scene);
        return placementParent;
    }

    private bool TryFindSurfaceNearPoint(Vector3 point, Vector3 normal, out RaycastHit hit)
    {
        Ray probe = new Ray(point + normal.normalized * SurfaceProbeOffset, -normal.normalized);
        if (Physics.Raycast(probe, out hit, SurfaceProbeOffset * 2f + 0.5f, placementMask.value))
            return true;

        Ray fallback = new Ray(point + Vector3.up * SurfaceProbeOffset, Vector3.down);
        return Physics.Raycast(fallback, out hit, SurfaceProbeOffset * 2f + 0.5f, placementMask.value);
    }

    private bool IsTooCloseToSurfaceEdge(RaycastHit hit, float padding)
    {
        if (padding <= 0f || hit.collider == null)
            return false;

        Vector3 normal = hit.normal.sqrMagnitude > 0.0001f ? hit.normal.normalized : Vector3.up;
        Vector2[] probes =
        {
            new Vector2(1f, 0f),
            new Vector2(-1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(0f, -1f),
            new Vector2(0.7071f, 0.7071f),
            new Vector2(0.7071f, -0.7071f),
            new Vector2(-0.7071f, 0.7071f),
            new Vector2(-0.7071f, -0.7071f)
        };

        for (int i = 0; i < probes.Length; i++)
        {
            Vector3 offset = GetPlaneOffset(normal, probes[i] * padding);
            Vector3 samplePoint = hit.point + offset;
            Ray probeRay = new Ray(samplePoint + normal * SurfaceProbeOffset, -normal);

            if (!Physics.Raycast(probeRay, out RaycastHit probeHit, SurfaceProbeOffset * 2f + 0.5f, placementMask.value))
                return true;

            if (probeHit.collider != hit.collider)
                return true;
        }

        return false;
    }

    private void PopulateValidPrefabs(List<GameObject> result)
    {
        result.Clear();
        if (prefabLibrary == null || prefabLibrary.prefabs == null)
            return;

        for (int i = 0; i < prefabLibrary.prefabs.Count; i++)
        {
            GameObject prefab = prefabLibrary.prefabs[i];
            if (prefab == null)
                continue;

            if (PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                result.Add(prefab);
            }
        }
    }

    private GrassPrefabLibrary CreateNewLibraryAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Grass Prefab Palette",
            "GrassPrefabPalette",
            "asset",
            "Choose where to save the new prefab palette asset.");

        if (string.IsNullOrEmpty(path))
            return null;

        GrassPrefabLibrary asset = CreateInstance<GrassPrefabLibrary>();
        asset.hideFlags = HideFlags.None;
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
        return asset;
    }

    private void DeleteSelectedLibraryAsset()
    {
        if (prefabLibrary == null)
            return;

        string assetPath = AssetDatabase.GetAssetPath(prefabLibrary);
        if (string.IsNullOrEmpty(assetPath))
            return;

        bool confirmed = EditorUtility.DisplayDialog(
            "Delete Grass Prefab Palette",
            $"Delete palette '{prefabLibrary.name}'? This cannot be undone.",
            "Delete",
            "Cancel");

        if (!confirmed)
            return;

        GrassPrefabLibrary paletteToDelete = prefabLibrary;
        prefabLibrary = null;
        selectedPaletteIndex = -1;
        selectedPrefabIndices.Clear();

        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (Selection.activeObject == paletteToDelete)
        {
            Selection.activeObject = null;
        }

        RefreshPaletteList();
    }

    private static LayerMask LayerMaskField(string label, LayerMask current)
    {
        var layers = new List<string>();
        var layerNumbers = new List<int>();

        for (int i = 0; i <= 31; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName))
            {
                layers.Add(layerName);
                layerNumbers.Add(i);
            }
        }

        int maskWithoutEmpty = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            int layerNumber = layerNumbers[i];
            if ((current.value & (1 << layerNumber)) != 0)
            {
                maskWithoutEmpty |= 1 << i;
            }
        }

        maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());

        int finalMask = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) != 0)
            {
                finalMask |= 1 << layerNumbers[i];
            }
        }

        current.value = finalMask;
        return current;
    }

    private static float DrawFloatFieldWithReset(string label, float value, float defaultValue)
    {
        EditorGUILayout.BeginHorizontal();
        value = EditorGUILayout.FloatField(label, value);
        if (GUILayout.Button("R", GUILayout.Width(20f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
        {
            value = defaultValue;
        }

        EditorGUILayout.EndHorizontal();
        return value;
    }

    private static int DrawIntFieldWithReset(string label, int value, int defaultValue)
    {
        EditorGUILayout.BeginHorizontal();
        value = EditorGUILayout.IntField(label, value);
        if (GUILayout.Button("R", GUILayout.Width(20f), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
        {
            value = defaultValue;
        }

        EditorGUILayout.EndHorizontal();
        return value;
    }

    private static Vector3 GetPlaneOffset(Vector3 normal, Vector2 circleOffset)
    {
        Vector3 tangent = Vector3.Cross(normal, Vector3.up);
        if (tangent.sqrMagnitude < 0.0001f)
        {
            tangent = Vector3.Cross(normal, Vector3.right);
        }

        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;
        return tangent * circleOffset.x + bitangent * circleOffset.y;
    }

    private static void DrawInlineSeparator(Rect separatorRect)
    {
        EditorGUI.DrawRect(separatorRect, new Color(0.32f, 0.32f, 0.32f, 1f));
    }

    private static void DrawCardOutline(Rect rect, Color color, float thickness)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + thickness, thickness, rect.height - thickness * 2f), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y + thickness, thickness, rect.height - thickness * 2f), color);
    }

    private void ToggleSelection(int index)
    {
        if (!selectedPrefabIndices.Add(index))
        {
            selectedPrefabIndices.Remove(index);
        }
    }

    private void ResetSelection()
    {
        selectedPrefabIndices.Clear();

        if (prefabLibrary == null)
            return;

        PopulateValidPrefabs(validPrefabBuffer);
        if (validPrefabBuffer.Count > 0)
        {
            selectedPrefabIndices.Add(0);
        }
    }

    private void SanitizeSelection(int validPrefabCount)
    {
        if (validPrefabCount <= 0)
        {
            selectedPrefabIndices.Clear();
            return;
        }

        selectedPrefabIndices.RemoveWhere(index => index < 0 || index >= validPrefabCount);
    }

    private void ShiftSelectionAfterRemoval(int removedIndex)
    {
        if (selectedPrefabIndices.Count == 0)
            return;

        int[] indices = selectedPrefabIndices.OrderBy(index => index).ToArray();
        selectedPrefabIndices.Clear();

        for (int i = 0; i < indices.Length; i++)
        {
            int index = indices[i];
            if (index == removedIndex)
                continue;

            selectedPrefabIndices.Add(index > removedIndex ? index - 1 : index);
        }
    }

    private void PopulateSelectedPrefabs(List<GameObject> validPrefabs, List<GameObject> result)
    {
        result.Clear();
        for (int i = 0; i < validPrefabs.Count; i++)
        {
            if (selectedPrefabIndices.Contains(i))
            {
                result.Add(validPrefabs[i]);
            }
        }
    }

    private int CollectNonSurfaceOverlaps(Vector3 center, Vector3 extents, Quaternion orientation)
    {
        while (true)
        {
            int overlapCount = Physics.OverlapBoxNonAlloc(center, extents, nonSurfaceOverlapBuffer, orientation, ~0, QueryTriggerInteraction.Ignore);
            if (overlapCount < nonSurfaceOverlapBuffer.Length)
                return overlapCount;

            System.Array.Resize(ref nonSurfaceOverlapBuffer, nonSurfaceOverlapBuffer.Length * 2);
        }
    }

    private bool HasOverlapWithPlacedInstances(Vector3 position, float minSpacing, GameObject restrictToPrefab)
    {
        if (minSpacing <= 0f)
            return false;

        Transform parent = GetPlacementParentIfExists();
        if (parent == null)
            return false;

        float minSpacingSqr = minSpacing * minSpacing;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child == null)
                continue;

            if (child.GetComponent<GrassPlacementInstanceTag>() == null)
                continue;

            if (restrictToPrefab != null)
            {
                GameObject sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject) as GameObject;
                if (sourcePrefab != restrictToPrefab)
                    continue;
            }

            Vector3 delta = child.position - position;
            if (delta.sqrMagnitude <= minSpacingSqr)
                return true;
        }

        return false;
    }

    private Transform GetPlacementParentIfExists()
    {
        if (placementParent != null)
            return placementParent;

        GameObject existing = GameObject.Find(DefaultParentName);
        if (existing != null)
        {
            placementParent = existing.transform;
            return placementParent;
        }

        return null;
    }
}
