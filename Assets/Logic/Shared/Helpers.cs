using UnityEngine;
using Scellecs.Morpeh;
using DG.Tweening;
using System.IO;

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

    public static Vector3 GridToWorldPosition(int gridX, int gridY, LevelComponent levelComponent)
    {
        float cellSize = CalculateAdaptGridCellSize(levelComponent);

        float centerX = (levelComponent.width - 1) * 0.5f;
        float gridBottom = -levelComponent.camera.orthographicSize;

        float worldX = (gridX - centerX) * cellSize;
        float worldY = gridBottom + gridY * cellSize + cellSize * 0.5f;
        float worldZ = -(gridX + gridY) * 0.1f;

        return new Vector3(worldX, worldY, worldZ) + levelComponent.gridOffset;
    }

    public static Vector3 GridToWorldPosition(Vector2Int gridPosition, LevelComponent levelComponent)
    {
        return GridToWorldPosition(gridPosition.x, gridPosition.y, levelComponent);
    }

    public static Vector2Int WorldToGridPosition(Vector3 worldPosition, LevelComponent levelComponent)
    {
        float adaptedCellSize = CalculateAdaptGridCellSize(levelComponent);

        Vector3 localPosition = worldPosition - levelComponent.gridOffset;

        float centerX = (levelComponent.width - 1) * 0.5f;
        float gridBottom = -levelComponent.camera.orthographicSize;

        int gridX = Mathf.RoundToInt(localPosition.x / adaptedCellSize + centerX);
        int gridY = Mathf.RoundToInt((localPosition.y - gridBottom - adaptedCellSize * 0.5f) / adaptedCellSize);

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
        Vector3 worldPosition = GridToWorldPosition(gridPosition, levelComponent);
        targetTransform.position = worldPosition;

        if (levelComponent.camera != null)
        {
            float adaptedCellSize = CalculateAdaptGridCellSize(levelComponent);

            float elementScale = adaptedCellSize / levelComponent.elementSize;
            targetTransform.localScale = new Vector3(elementScale, elementScale, 1f);
        }
        else targetTransform.localScale = Vector3.one;
    }

    public static float CalculateAdaptGridCellSize(LevelComponent levelComponent)
    {
        float screenHeight = levelComponent.camera.orthographicSize * 2f;
        float screenWidth = screenHeight * levelComponent.camera.aspect;

        float gridWidthInUnits = levelComponent.width * levelComponent.cellSize;
        float gridHeightInUnits = levelComponent.height * levelComponent.cellSize;

        float scaleX = screenWidth / gridWidthInUnits;
        float scaleY = screenHeight / gridHeightInUnits;

        float gridScale = Mathf.Min(scaleX, scaleY) * (1 - levelComponent.screenPadding);

        float maxAllowedScale = levelComponent.maxGridSizeInUnits / gridWidthInUnits;
        gridScale = Mathf.Min(gridScale, maxAllowedScale);

        return levelComponent.cellSize * gridScale;
    }

    public static GridBounds CalculateGridBounds(LevelComponent levelComponent, float cellSize)
    {
        float totalGridWidth = levelComponent.width * cellSize;
        float totalGridHeight = levelComponent.height * cellSize;

        float gridLeft = -totalGridWidth * 0.5f;
        float gridBottom = -levelComponent.camera.orthographicSize;

        return new GridBounds
        {
            totalGridWidth = totalGridWidth,
            totalGridHeight = totalGridHeight,
            gridLeft = gridLeft,
            gridBottom = gridBottom
        };
    }

    public static bool IsWithinGridBounds(Vector2Int position, Entity?[,] grid)
    {
        return position.x >= 0 && position.x < grid.GetLength(0) &&
               position.y >= 0 && position.y < grid.GetLength(1);
    }

    public static void TweenElement(World world, LevelComponent levelComponent, Entity gridEntity, Entity elementEntity, Vector2Int fromPos, Vector2Int toPos, Ease easeType = Ease.OutQuad, float duration = 0.3f, TweenCallback onComplete = null)
    {
        Vector3 fromWorldPos = GridToWorldPosition(fromPos, levelComponent);
        Vector3 toWorldPos = GridToWorldPosition(toPos, levelComponent);

        ref var tweenComponent = ref world.GetStash<TweenComponent>().Add(elementEntity);
        tweenComponent.tween = world.GetStash<ViewRefComponent>().Get(elementEntity).viewRef.transform
            .DOMove(toWorldPos, duration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                world.GetStash<TweenComponent>().Remove(elementEntity);
            });
    }

    public static void EmmitLevelIndexSaveRequest(World world)
    {
        var saveRequest = world.GetRequest<SaveRequest>();
        saveRequest.Publish(new SaveRequest
        {
            actionType = SaveRequestActionType.SaveLevelIndex
        });
    }

    public static void EmmitGridSaveRequest(World world)
    {
        var saveRequest = world.GetRequest<SaveRequest>();
        saveRequest.Publish(new SaveRequest
        {
            actionType = SaveRequestActionType.SaveGrid
        });
    }

    public static void EmmitClearGridSaveRequest(World world)
    {
        var saveRequest = world.GetRequest<SaveRequest>();
        saveRequest.Publish(new SaveRequest
        {
            actionType = SaveRequestActionType.ClearGrid
        });
    }

    public static void EmmitReloadSceneRequest(World world)
    {
        var reloadSceneRequest = world.GetRequest<ReloadSceneRequest>();
        reloadSceneRequest.Publish(new ReloadSceneRequest());
    }

    public static string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, "game_save.dat");
    }

    public static bool LoadData(out SaveData saveData)
    {
        saveData = new SaveData();

        var savePath = GetSaveFilePath();

        if (!File.Exists(savePath))
            return false;

        try
        {
            using (FileStream fs = new FileStream(savePath, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                saveData.currentLevelIndex = reader.ReadInt32();
                int gridStateLength = reader.ReadInt32();
                if (gridStateLength != 0)
                {
                    saveData.gridData = new int[gridStateLength];
                    for (int i = 0; i < gridStateLength; i++)
                    {
                        saveData.gridData[i] = reader.ReadInt32();
                    }
                }
            }

            Debug.Log($"Loaded from: {savePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load save data: {e.Message}");
            return false;
        }
    }



    public static bool IsGridEmpty(GridComponent gridComponent)
    {
        for (int x = 0; x < gridComponent.elements.GetLength(0); x++)
        {
            for (int y = 0; y < gridComponent.elements.GetLength(1); y++)
            {
                if (gridComponent.elements[x, y].HasValue)
                    return false;
            }
        }
        return true;
    }

    public static bool IsAllCellsUnlocked(GridComponent gridComponent)
    {
        for (int x = 0; x < gridComponent.state.GetLength(0); x++)
        {
            for (int y = 0; y < gridComponent.state.GetLength(1); y++)
            {
                if (gridComponent.state[x, y])
                {
                    return false;
                }
            }
        }
        return true;
    }

    public static Vector3 GetRandomBalloonSpawnPosition(LevelComponent levelComponent, float side)
    {
        float screenHeight = levelComponent.camera.orthographicSize * 2f;
        float screenWidth = screenHeight * levelComponent.camera.aspect;

        float minHeight = -levelComponent.camera.orthographicSize + levelComponent.balloonsPreset.spawnOffsetFromBottom;
        float maxHeight = -levelComponent.camera.orthographicSize + screenHeight * levelComponent.balloonsPreset.spawnHeightPercentage;
        float randomHeight = Random.Range(minHeight, maxHeight);

        float xPosition = side > 0.5f ?
            screenWidth * 0.5f + levelComponent.balloonsPreset.spawnOffsetFromSide :
            -screenWidth * 0.5f - levelComponent.balloonsPreset.spawnOffsetFromSide;

        return new Vector3(xPosition, randomHeight, 0f);
    }

    public static Vector3 GetRandomBalloonTargetPosition(LevelComponent levelComponent, float side)
    {
        float screenHeight = levelComponent.camera.orthographicSize * 2f;
        float screenWidth = screenHeight * levelComponent.camera.aspect;

        float minHeight = levelComponent.camera.orthographicSize - screenHeight * levelComponent.balloonsPreset.targetHeightPercentage;
        float maxHeight = levelComponent.camera.orthographicSize - levelComponent.balloonsPreset.targetOffsetFromTop;
        float randomHeight = Random.Range(minHeight, maxHeight);

        float xPosition = side > 0.5f ?
            screenWidth * 0.5f + levelComponent.balloonsPreset.targetOffsetFromSide :
            -screenWidth * 0.5f - levelComponent.balloonsPreset.targetOffsetFromSide;

        return new Vector3(xPosition, randomHeight, 0f);
    }

    public static Vector3 CalculateBalloonPosition(Vector3 startPos, Vector3 endPos, float progress, float amplitude, float frequency)
    {
        Vector3 linearPosition = Vector3.Lerp(startPos, endPos, progress);

        float sineOffset = Mathf.Sin(progress * Mathf.PI * frequency) * amplitude;
        Vector3 sineDirection = Vector3.up;

        return linearPosition + sineDirection * sineOffset;
    }
}