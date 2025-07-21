// TODO: сделать отображение не цветом, а спрайтами элементов

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    private const float MIN_GRID_SIZE = 2;
    private const float MAX_GRID_SIZE = 10;

    private const float GRID_OFSSETS = 40f;
    private const float MAX_GRID_HEIGHT_RATIO = 0.35f;
    private const float GRID_CELL_THICKNESS = 1f;
    private const float MAX_GRID_CELL_SIZE = 20f;

    private const float SPACING = 2f;

    private const float PALETTE_CELL_SIZE = 30f;

    private const float CLEAR_GRID_BUTTON_PADDING = 40f;

    private const float EMPTY_CELL_FONT_SIZE_MULTIPLIER = 0.3f;
    private const string EMPTY_CELL_SYMBOL = "—";

    private static readonly Color EMPTY_CELL_COLOR = Helpers.HexToColor("#4D4D4DCC");
    private static readonly Color GRID_BACKGROUND_COLOR = Helpers.HexToColor("#33333380");
    private static readonly Color PALETTE_BACKGROUND_COLOR = Helpers.HexToColor("#3333334D");

    private static Texture2D trashIcon;

    private static readonly float DATABASE_HINT_WIDTH = 200f;
    private static readonly string DATABASE_HINT_TEXT = "Drag or click (ElementDataBase)";
    private static readonly Color DATABASE_HINT_TEXT_COLOR = Helpers.HexToColor("#777777");
    private static readonly int DATABASE_HINT_FONT_SIZE = 10;

    private int[] grid;
    private List<Element> databaseElements = new List<Element>();

    private bool gridInitialized = false;
    private bool databaseNeedsUpdate = true;

    private int selectedElementIndex = -1;

    private int previousWidth = -1;
    private int previousHeight = -1;

    private void OnEnable()
    {
        if (trashIcon == null)
            trashIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Content/Sprites/can.png");
    }

    public override void OnInspectorGUI()
    {
        Level level = (Level)target;

        DrawPropertiesExcluding(serializedObject, "m_Script");

        HandleObjectSelector();
        GridSizeValidation(level);

        UpdateGrid(level);

        DrawElementDatabase();
        DrawGrid(level);
        DrawClearGridButton(level);

        HandleSaveGrid(level);
    }

    private void HandleObjectSelector()
    {
        if (Event.current.commandName == "ObjectSelectorUpdated")
        {
            ElementDataBase selectedDataBase = EditorGUIUtility.GetObjectPickerObject() as ElementDataBase;
            if (selectedDataBase != null)
            {
                Level level = (Level)target;
                level.elementDataBase = selectedDataBase;
                EditorUtility.SetDirty(level);
                databaseNeedsUpdate = true;
            }
        }
    }

    private void GridSizeValidation(Level level)
    {
        EditorGUILayout.Space();

        if (level.width < MIN_GRID_SIZE || level.height < MIN_GRID_SIZE)
        {
            EditorGUILayout.HelpBox("Width and Height must be at least 2", MessageType.Warning);
            return;
        }

        if (level.width > MAX_GRID_SIZE || level.height > MAX_GRID_SIZE)
        {
            EditorGUILayout.HelpBox("Width and Height cannot exceed 10", MessageType.Warning);
            return;
        }
    }

    private void UpdateGrid(Level level)
    {
        if (HasGridSizeChanged(level))
        {
            InitializeGrid(level);
            databaseNeedsUpdate = true;
        }
    }

    private void HandleSaveGrid(Level level)
    {
        if (HasGridSizeChanged(level))
        {
            ApplyGridToLevel(level);
            EditorUtility.SetDirty(level);
        }
    }

    private bool HasGridSizeChanged(Level level)
    {
        return !gridInitialized || grid == null ||
            grid.Length != level.width * level.height;
    }

    private void DrawClearGridButton(Level level)
    {
        float buttonAvailableWidth = EditorGUIUtility.currentViewWidth - CLEAR_GRID_BUTTON_PADDING;
        float buttonCellSize = (buttonAvailableWidth - (level.width - 1) * SPACING) / level.width;
        buttonCellSize = Mathf.Max(buttonCellSize, 20f);
        float buttonWidth = level.width * (buttonCellSize + SPACING) - SPACING;

        EditorGUILayout.Space();
        if (GUILayout.Button("Clear Grid", GUILayout.Width(buttonWidth)))
        {
            ClearGrid();
            ApplyGridToLevel(level);
            EditorUtility.SetDirty(level);
        }
    }

    private void InitializeGrid(Level level)
    {
        int[] oldGrid = grid;
        grid = new int[level.width * level.height];

        for (int i = 0; i < grid.Length; i++)
            grid[i] = -1;

        if (level.grid == null || level.grid.Length != level.width * level.height)
        {
            level.grid = new int[level.width * level.height];
            for (int i = 0; i < level.grid.Length; i++)
            {
                level.grid[i] = -1;
            }
        }

        if (oldGrid != null && gridInitialized)
            TransferGridData(oldGrid, previousWidth, previousHeight, grid, level.width, level.height);
        else if (level.grid != null && level.grid.Length == level.width * level.height && HasNonDefaultData(level))
            LoadGridFromLevel(level);

        gridInitialized = true;
        previousWidth = level.width;
        previousHeight = level.height;
    }

    private bool HasNonDefaultData(Level level)
    {
        for (int i = 0; i < level.grid.Length; i++)
        {
            if (level.grid[i] != -1)
                return true;
        }
        return false;
    }

    private void TransferGridData(int[] oldGrid, int oldWidth, int oldHeight, int[] newGrid, int newWidth, int newHeight)
    {
        int minWidth = Mathf.Min(oldWidth, newWidth);
        int minHeight = Mathf.Min(oldHeight, newHeight);

        for (int y = 0; y < minHeight; y++)
        {
            for (int x = 0; x < minWidth; x++)
            {
                int oldIndex = y * oldWidth + x;
                int newIndex = y * newWidth + x;

                if (oldIndex < oldGrid.Length && newIndex < newGrid.Length)
                    newGrid[newIndex] = oldGrid[oldIndex];
            }
        }
    }

    private void LoadGridFromLevel(Level level)
    {
        for (int i = 0; i < grid.Length && i < level.grid.Length; i++)
            grid[i] = level.grid[i];
    }

    private void DrawElementDatabase()
    {
        UpdateDatabaseElements();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Element Database", EditorStyles.boldLabel);

        float totalWidth = (databaseElements.Count + 2) * (PALETTE_CELL_SIZE + SPACING) - SPACING;
        Rect paletteRect = GUILayoutUtility.GetRect(totalWidth, PALETTE_CELL_SIZE + 10f);

        EditorGUI.DrawRect(paletteRect, PALETTE_BACKGROUND_COLOR);

        float currentX = paletteRect.x;

        DrawEmptyButton(paletteRect, ref currentX);
        DrawElementButtons(paletteRect, ref currentX);
        DrawDatabaseControls(paletteRect, currentX);
    }

    private void UpdateDatabaseElements()
    {
        if (!databaseNeedsUpdate) return;

        Level level = (Level)target;
        databaseElements.Clear();

        if (level.elementDataBase == null)
            return;

        if (level.elementDataBase.elements != null)
        {
            foreach (var element in level.elementDataBase.elements)
            {
                if (element != null)
                    databaseElements.Add(element);
            }
        }

        databaseNeedsUpdate = false;
    }

    private void DrawEmptyButton(Rect paletteRect, ref float currentX)
    {
        Rect emptyRect = new Rect(currentX, paletteRect.y + 5f, PALETTE_CELL_SIZE, PALETTE_CELL_SIZE);
        EditorGUI.DrawRect(emptyRect, EMPTY_CELL_COLOR);

        Color emptyBorderColor = selectedElementIndex == -1 ? Color.yellow : Color.white;
        DrawCellBorder(emptyRect, emptyBorderColor, 2);

        DrawCenteredText(emptyRect, EMPTY_CELL_SYMBOL, Color.white, 12);

        if (Event.current.type == EventType.MouseDown && emptyRect.Contains(Event.current.mousePosition))
        {
            selectedElementIndex = -1;
            Event.current.Use();
        }

        currentX += PALETTE_CELL_SIZE + SPACING;
    }

    private void DrawElementButtons(Rect paletteRect, ref float currentX)
    {
        for (int i = 0; i < databaseElements.Count; i++)
        {
            var element = databaseElements[i];
            if (element == null) continue;

            Rect elementRect = new Rect(currentX, paletteRect.y + 5f, PALETTE_CELL_SIZE, PALETTE_CELL_SIZE);
            EditorGUI.DrawRect(elementRect, element.color);

            Color elementBorderColor = selectedElementIndex == i ? Color.yellow : Color.white;
            DrawCellBorder(elementRect, elementBorderColor, 2);

            if (elementRect.Contains(Event.current.mousePosition))
                GUI.Label(elementRect, new GUIContent("", element.name));

            if (Event.current.type == EventType.MouseDown && elementRect.Contains(Event.current.mousePosition))
            {
                selectedElementIndex = i;
                Event.current.Use();
            }

            currentX += PALETTE_CELL_SIZE + SPACING;
        }
    }

    private void DrawDatabaseControls(Rect paletteRect, float currentX)
    {
        Level level = (Level)target;
        if (level.elementDataBase == null)
            DrawDatabaseHint(paletteRect, currentX, level);
        else
            DrawUnlinkButton(paletteRect, level);
    }

    private void DrawDatabaseHint(Rect paletteRect, float currentX, Level level)
    {
        Rect hintRect = new Rect(currentX, paletteRect.y + 5f, DATABASE_HINT_WIDTH, PALETTE_CELL_SIZE);
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.normal.textColor = DATABASE_HINT_TEXT_COLOR;
        hintStyle.fontSize = DATABASE_HINT_FONT_SIZE;
        EditorGUI.LabelField(hintRect, DATABASE_HINT_TEXT, hintStyle);

        UpdateDatabaseHintInteraction(hintRect, level);
    }

    private void UpdateDatabaseHintInteraction(Rect hintRect, Level level)
    {
        if (!hintRect.Contains(Event.current.mousePosition)) return;

        if (Event.current.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            Event.current.Use();
        }
        else if (Event.current.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
            {
                ElementDataBase dataBase = draggedObject as ElementDataBase;
                if (dataBase != null)
                {
                    level.elementDataBase = dataBase;
                    EditorUtility.SetDirty(level);
                    databaseNeedsUpdate = true;
                    break;
                }
            }
            Event.current.Use();
        }
        else if (Event.current.type == EventType.MouseDown)
        {
            EditorGUIUtility.ShowObjectPicker<ElementDataBase>(null, false, "", 0);
            Event.current.Use();
        }
    }

    private void DrawUnlinkButton(Rect paletteRect, Level level)
    {
        Rect unlinkRect = new Rect(paletteRect.x + paletteRect.width - PALETTE_CELL_SIZE, paletteRect.y + 5f, PALETTE_CELL_SIZE, PALETTE_CELL_SIZE);

        GUIContent buttonContent = new GUIContent(trashIcon);

        if (GUI.Button(unlinkRect, buttonContent))
        {
            level.elementDataBase = null;
            EditorUtility.SetDirty(level);
            databaseNeedsUpdate = true;
        }
    }

    private void DrawGrid(Level level)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Element Grid", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Size: {level.width} x {level.height}", EditorStyles.miniLabel);

        GridLayout gridLayout = CalculateGridLayout(level);
        Rect gridRect = GUILayoutUtility.GetRect(gridLayout.totalWidth, gridLayout.totalHeight);
        gridRect.x = (EditorGUIUtility.currentViewWidth - gridLayout.totalWidth) / 2f;

        EditorGUI.DrawRect(gridRect, GRID_BACKGROUND_COLOR);

        DrawGridCells(gridRect, gridLayout, level);
    }

    private struct GridLayout
    {
        public float cellSize;
        public float totalWidth;
        public float totalHeight;
    }

    private GridLayout CalculateGridLayout(Level level)
    {
        float availableWidth = EditorGUIUtility.currentViewWidth - GRID_OFSSETS;
        float maxHeight = Screen.height * MAX_GRID_HEIGHT_RATIO;
        float cellSize = (availableWidth - (level.width - 1) * SPACING) / level.width;
        cellSize = Mathf.Max(cellSize, MAX_GRID_CELL_SIZE);

        float totalWidth = level.width * (cellSize + SPACING) - SPACING;
        float totalHeight = level.height * (cellSize + SPACING) - SPACING;

        bool needsHeightScaling = totalHeight > maxHeight;
        if (needsHeightScaling)
        {
            float scaleFactor = maxHeight / totalHeight;
            cellSize *= scaleFactor;
            totalWidth = level.width * (cellSize + SPACING) - SPACING;
            totalHeight = level.height * (cellSize + SPACING) - SPACING;
        }

        return new GridLayout { cellSize = cellSize, totalWidth = totalWidth, totalHeight = totalHeight };
    }

    private void DrawGridCells(Rect gridRect, GridLayout layout, Level level)
    {
        for (int x = 0; x < level.width; x++)
        {
            for (int y = 0; y < level.height; y++)
            {
                int idx = y * level.width + x;
                float xPos = gridRect.x + x * (layout.cellSize + SPACING);
                float yPos = gridRect.y + (level.height - 1 - y) * (layout.cellSize + SPACING);
                Rect cellRect = new Rect(xPos, yPos, layout.cellSize, layout.cellSize);

                Element element = GetElementAtIndex(grid[idx], level);
                Color cellColor = GetCellColor(element);
                EditorGUI.DrawRect(cellRect, cellColor);
                DrawCellBorder(cellRect, Color.white, GRID_CELL_THICKNESS);

                UpdateGridCellInteraction(cellRect, idx, level);

                if (element == null)
                {
                    DrawCenteredText(
                        cellRect,
                        EMPTY_CELL_SYMBOL,
                        Color.white,
                        Mathf.RoundToInt(layout.cellSize * EMPTY_CELL_FONT_SIZE_MULTIPLIER)
                    );
                }
            }
        }
    }

    private void UpdateGridCellInteraction(Rect cellRect, int idx, Level level)
    {
        if (!cellRect.Contains(Event.current.mousePosition)) return;

        if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.button == 0)
        {
            if (selectedElementIndex == -1)
                grid[idx] = -1;
            else if (selectedElementIndex >= 0 && selectedElementIndex < databaseElements.Count)
            {
                var selectedElement = databaseElements[selectedElementIndex];
                grid[idx] = GetElementIndexInDatabase(selectedElement, level);
            }
            ApplyGridToLevel(level);
            EditorUtility.SetDirty(level);
            Event.current.Use();
        }
    }

    private void ApplyGridToLevel(Level level)
    {
        if (level.grid == null || level.grid.Length != grid.Length)
            level.grid = new int[grid.Length];
        for (int i = 0; i < grid.Length; i++)
        {
            level.grid[i] = grid[i];
        }
    }

    private void ClearGrid()
    {
        if (grid == null) return;
        for (int i = 0; i < grid.Length; i++)
            grid[i] = -1;
    }

    private Element GetElementAtIndex(int index, Level level)
    {
        if (index == -1 || level.elementDataBase == null || level.elementDataBase.elements == null ||
            index >= level.elementDataBase.elements.Length)
            return null;

        return level.elementDataBase.elements[index];
    }

    private int GetElementIndexInDatabase(Element element, Level level)
    {
        if (level.elementDataBase == null || level.elementDataBase.elements == null)
            return -1;

        return Array.IndexOf(level.elementDataBase.elements, element);
    }

    private Color GetCellColor(Element element)
    {
        if (element == null)
            return EMPTY_CELL_COLOR;

        return element.color;
    }

    private void DrawCellBorder(Rect rect, Color color, float thickness)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
    }

    private void DrawCenteredText(Rect rect, string text, Color color, int fontSize)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = color;
        style.fontSize = fontSize;
        EditorGUI.LabelField(rect, text, style);
    }
}