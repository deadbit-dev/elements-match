using UnityEngine;
using Scellecs.Morpeh;

public class GridDebugRenderer : MonoBehaviour
{
    [Header("Debug Settings")]
    public Color gridLineColor = Color.white;
    public float gridLineWidth = 0.02f;

    private Filter levelFilter;
    private Stash<LevelComponent> levelComponents;

    private void Start()
    {
        if (World.Default == null)
            return;

        var world = World.Default;
        levelFilter = world.Filter.With<LevelComponent>().Build();
        levelComponents = world.GetStash<LevelComponent>();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || levelFilter == null)
            return;

        foreach (var levelEntity in levelFilter)
        {
            ref var levelComponent = ref levelComponents.Get(levelEntity);
            DrawGridLines(levelComponent, Camera.main);
        }
    }

    private void DrawGridLines(LevelComponent levelComponent, Camera camera)
    {
        if (camera == null) return;

        var adaptedCellSize = Helpers.CalculateAdaptGridCellSize(levelComponent);
        var gridBounds = Helpers.CalculateGridBounds(levelComponent, adaptedCellSize);

        var totalGridWidth = gridBounds.totalGridWidth;
        var totalGridHeight = gridBounds.totalGridHeight;
        var gridLeft = gridBounds.gridLeft;
        var gridBottom = gridBounds.gridBottom;

        Gizmos.color = gridLineColor;

        for (int x = 0; x <= levelComponent.width; x++)
        {
            float xPos = gridLeft + x * adaptedCellSize;
            Vector3 start = new Vector3(xPos, gridBottom, 0) + levelComponent.gridOffset;
            Vector3 end = new Vector3(xPos, gridBottom + totalGridHeight, 0) + levelComponent.gridOffset;
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= levelComponent.height; y++)
        {
            float yPos = gridBottom + y * adaptedCellSize;
            Vector3 start = new Vector3(gridLeft, yPos, 0) + levelComponent.gridOffset;
            Vector3 end = new Vector3(gridLeft + totalGridWidth, yPos, 0) + levelComponent.gridOffset;
            Gizmos.DrawLine(start, end);
        }

        Gizmos.color = Color.red;
        Vector3 gridTopLeft = new Vector3(gridLeft, gridBottom + totalGridHeight, 0) + levelComponent.gridOffset;
        Vector3 gridTopRight = new Vector3(gridLeft + totalGridWidth, gridBottom + totalGridHeight, 0) + levelComponent.gridOffset;
        Vector3 gridBottomLeft = new Vector3(gridLeft, gridBottom, 0) + levelComponent.gridOffset;
        Vector3 gridBottomRight = new Vector3(gridLeft + totalGridWidth, gridBottom, 0) + levelComponent.gridOffset;

        Gizmos.DrawLine(gridTopLeft, gridTopRight);
        Gizmos.DrawLine(gridTopRight, gridBottomRight);
        Gizmos.DrawLine(gridBottomRight, gridBottomLeft);
        Gizmos.DrawLine(gridBottomLeft, gridTopLeft);
    }
}