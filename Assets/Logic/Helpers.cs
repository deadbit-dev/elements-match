using UnityEngine;
using Scellecs.Morpeh;

public static class Helpers
{
    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("#", "");

        byte r = 255;
        byte g = 255;
        byte b = 255;
        byte a = 255;

        if (hex.Length == 6)
        {
            // RRGGBB
            r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }
        else if (hex.Length == 8)
        {
            // RRGGBBAA
            r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        else if (hex.Length == 3)
        {
            // RGB
            r = byte.Parse(hex[0].ToString() + hex[0].ToString(), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex[1].ToString() + hex[1].ToString(), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex[2].ToString() + hex[2].ToString(), System.Globalization.NumberStyles.HexNumber);
        }
        else if (hex.Length == 4)
        {
            // RGBA
            r = byte.Parse(hex[0].ToString() + hex[0].ToString(), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex[1].ToString() + hex[1].ToString(), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex[2].ToString() + hex[2].ToString(), System.Globalization.NumberStyles.HexNumber);
            a = byte.Parse(hex[3].ToString() + hex[3].ToString(), System.Globalization.NumberStyles.HexNumber);
        }
        else
        {
            Debug.LogError("Invalid hex color format: " + hex);
            return Color.white;
        }

        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public static Vector3 GridToWorldPosition(int gridX, int gridY, int gridWidth, int gridHeight, float cellSize, Vector3 gridOffset)
    {
        float centerX = (gridWidth - 1) * 0.5f;
        float bottomY = 0;

        float worldX = (gridX - centerX) * cellSize;
        float worldY = (gridY - bottomY) * cellSize;
        float worldZ = -(gridX + gridY);

        return new Vector3(worldX, worldY, worldZ) + gridOffset;
    }

    public static Vector3 GridToWorldPosition(int gridX, int gridY, int gridWidth, int gridHeight, Camera camera, Vector3 gridOffset, float screenPadding, float maxGridSize)
    {
        if (camera == null)
        {
            Debug.LogError("Camera is null, using default cell size");
            return GridToWorldPosition(gridX, gridY, gridWidth, gridHeight, 1f, gridOffset);
        }

        float cellSize = CalculateGridCellSize(gridWidth, gridHeight, camera, screenPadding, maxGridSize);

        float centerX = (gridWidth - 1) * 0.5f;
        float gridBottom = -camera.orthographicSize;

        float worldX = (gridX - centerX) * cellSize;
        float worldY = gridBottom + gridY * cellSize + cellSize * 0.5f;
        float worldZ = -(gridX + gridY);

        return new Vector3(worldX, worldY, worldZ) + gridOffset;
    }

    public static Vector3 GridToWorldPosition(Vector2Int gridPosition, int gridWidth, int gridHeight, Camera camera, Vector3 gridOffset, float screenPadding, float maxGridSize)
    {
        return GridToWorldPosition(gridPosition.x, gridPosition.y, gridWidth, gridHeight, camera, gridOffset, screenPadding, maxGridSize);
    }

    public static Vector2Int WorldToGridPosition(Vector3 worldPosition, int gridWidth, int gridHeight, float cellSize, Vector3 gridOffset)
    {
        Vector3 localPosition = worldPosition - gridOffset;

        float centerX = (gridWidth - 1) * 0.5f;
        float bottomY = 0;

        int gridX = Mathf.RoundToInt(localPosition.x / cellSize + centerX);
        int gridY = Mathf.RoundToInt(localPosition.y / cellSize + bottomY);

        return new Vector2Int(gridX, gridY);
    }

    public static Vector2Int WorldToGridPosition(Vector3 worldPosition, int gridWidth, int gridHeight, Camera camera, Vector3 gridOffset, float screenPadding, float maxGridSize)
    {
        if (camera == null)
        {
            Debug.LogError("Camera is null, using default cell size");
            return WorldToGridPosition(worldPosition, gridWidth, gridHeight, 1f, gridOffset);
        }

        float cellSize = CalculateGridCellSize(gridWidth, gridHeight, camera, screenPadding, maxGridSize);

        Vector3 localPosition = worldPosition - gridOffset;

        float centerX = (gridWidth - 1) * 0.5f;
        float gridBottom = -camera.orthographicSize;

        int gridX = Mathf.RoundToInt(localPosition.x / cellSize + centerX);
        int gridY = Mathf.RoundToInt((localPosition.y - gridBottom - cellSize * 0.5f) / cellSize);

        return new Vector2Int(gridX, gridY);
    }

    public static Vector2Int FindElementPositionInGrid(Entity?[,] elements, Entity elementEntity)
    {
        for (int x = 0; x < elements.GetLength(0); x++)
        {
            for (int y = 0; y < elements.GetLength(1); y++)
            {
                if (elements[x, y].HasValue && elements[x, y].Value.CompareTo(elementEntity) == 0)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    public static void ApplyElementTransform(Transform targetTransform, Vector2Int gridPosition, LevelComponent levelComponent)
    {
        Vector3 worldPosition = GridToWorldPosition(gridPosition, levelComponent.width, levelComponent.height, levelComponent.camera, levelComponent.gridOffset, levelComponent.screenPadding, levelComponent.maxGridSize);
        targetTransform.position = worldPosition;

        if (levelComponent.camera != null)
        {
            float cellSize = CalculateGridCellSize(levelComponent.width, levelComponent.height, levelComponent.camera, levelComponent.screenPadding, levelComponent.maxGridSize);

            float elementScale = cellSize / levelComponent.elementSize;
            targetTransform.localScale = new Vector3(elementScale, elementScale, 1f);
        }
        else
        {
            targetTransform.localScale = Vector3.one;
        }
    }

    public static float CalculateGridCellSize(int gridWidth, int gridHeight, Camera camera, float screenPadding, float maxGridSize)
    {
        if (camera == null)
        {
            Debug.LogError("Camera is null, using default cell size");
            return 1f;
        }

        float cellSize = 0.5f;

        float screenHeight = camera.orthographicSize * 2f;
        float screenWidth = screenHeight * camera.aspect;

        float gridWidthInUnits = gridWidth * cellSize;
        float gridHeightInUnits = gridHeight * cellSize;

        float scaleX = screenWidth / gridWidthInUnits;
        float scaleY = screenHeight / gridHeightInUnits;

        float gridScale = Mathf.Min(scaleX, scaleY) * (1 - screenPadding);

        float maxAllowedScale = maxGridSize / gridWidthInUnits;
        gridScale = Mathf.Min(gridScale, maxAllowedScale);

        return cellSize * gridScale;
    }

    public static (float cellSize, float totalGridWidth, float totalGridHeight, float gridLeft, float gridBottom) CalculateGridBounds(int gridWidth, int gridHeight, Camera camera, float screenPadding, float maxGridSize)
    {
        float cellSize = CalculateGridCellSize(gridWidth, gridHeight, camera, screenPadding, maxGridSize);

        float totalGridWidth = gridWidth * cellSize;
        float totalGridHeight = gridHeight * cellSize;

        float gridLeft = -totalGridWidth * 0.5f;
        float gridBottom = -camera.orthographicSize;

        return (cellSize, totalGridWidth, totalGridHeight, gridLeft, gridBottom);
    }
}