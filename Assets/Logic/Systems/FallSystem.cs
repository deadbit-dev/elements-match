using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using System.Collections.Generic;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class FallSystem : ISystem
{
    public World World { get; set; }

    private Filter gridFilter;
    private Stash<GridComponent> gridComponents;

    private Filter levelFilter;
    private Stash<LevelComponent> levelComponents;

    private Filter elementFilter;
    private Stash<ElementComponent> elementComponents;
    private Stash<ViewRefComponent> viewRefComponents;

    private Request<FallTweenCompleteRequest> fallTweenCompleteRequest;

    public void OnAwake()
    {
        gridFilter = World.Filter.With<GridComponent>().Build();
        gridComponents = World.GetStash<GridComponent>();

        levelFilter = World.Filter.With<LevelComponent>().Build();
        levelComponents = World.GetStash<LevelComponent>();

        elementFilter = World.Filter.With<ElementComponent>().With<ViewRefComponent>().Without<TweenComponent>().Build();
        elementComponents = World.GetStash<ElementComponent>();
        viewRefComponents = World.GetStash<ViewRefComponent>();

        fallTweenCompleteRequest = World.GetRequest<FallTweenCompleteRequest>();
    }

    public void OnUpdate(float deltaTime)
    {
        if (levelFilter.IsEmpty() || gridFilter.IsEmpty())
            return;

        var levelEntity = levelFilter.First();
        ref var levelComponent = ref levelComponents.Get(levelEntity);

        var gridEntity = gridFilter.First();
        ref var gridComponent = ref gridComponents.Get(gridEntity);

        foreach (var request in fallTweenCompleteRequest.Consume())
        {
            UnlockCellsBetween(ref gridComponent, request.fromPos, request.toPos);
        }

        foreach (var elementEntity in elementFilter)
        {
            ref var elementComponent = ref elementComponents.Get(elementEntity);
            ref var viewRefComponent = ref viewRefComponents.Get(elementEntity);

            Vector2Int currentPosition = Helpers.FindElementPositionInGrid(gridComponent.elements, elementEntity);

            if (currentPosition.x == -1 || currentPosition.y == -1)
                continue;

            if (!CanElementFall(gridComponent, currentPosition))
                continue;

            Vector2Int targetPosition = FindFallTargetPosition(gridComponent, currentPosition);
            if (targetPosition != currentPosition)
                Fall(gridEntity, ref gridComponent, ref levelComponent, currentPosition, targetPosition);
        }
    }

    private Vector2Int FindFallTargetPosition(GridComponent gridComponent, Vector2Int currentPosition)
    {
        int targetY = currentPosition.y;
        for (int y = currentPosition.y - 1; y >= 0; y--)
        {
            if (IsValidCell(gridComponent, currentPosition.x, y)) targetY = y;
            else break;
        }

        return new Vector2Int(currentPosition.x, targetY);
    }

    private void Fall(Entity gridEntity, ref GridComponent gridComponent, ref LevelComponent levelComponent, Vector2Int fromPosition, Vector2Int toPosition)
    {
        int column = fromPosition.x;
        int fallDistance = fromPosition.y - toPosition.y;

        if (fallDistance <= 0) return;

        List<(Entity entity, Vector2Int fromPos, Vector2Int toPos)> elementsToMove = new List<(Entity, Vector2Int, Vector2Int)>();

        for (int y = toPosition.y; y < gridComponent.elements.GetLength(1); y++)
        {
            if (!gridComponent.elements[column, y].HasValue)
                continue;

            Entity elementEntity = gridComponent.elements[column, y].Value;
            Vector2Int elementFromPos = new Vector2Int(column, y);
            Vector2Int elementToPos = new Vector2Int(column, y - fallDistance);

            gridComponent.state[elementToPos.x, elementToPos.y] = true;
            elementsToMove.Add((elementEntity, elementFromPos, elementToPos));
        }

        if (elementsToMove.Count == 0) return;

        foreach (var (elementEntity, fromPos, toPos) in elementsToMove)
        {
            gridComponent.elements[fromPos.x, fromPos.y] = null;
            gridComponent.elements[toPos.x, toPos.y] = elementEntity;

            if (!viewRefComponents.Has(elementEntity))
                continue;

            Helpers.TweenElement(World, levelComponent, gridEntity, elementEntity, fromPos, toPos, levelComponent.fallEasing, levelComponent.fallDuration, () =>
            {
                World.GetRequest<FallTweenCompleteRequest>().Publish(new FallTweenCompleteRequest
                {
                    gridEntity = gridEntity,
                    elementEntity = elementEntity,
                    fromPos = fromPos,
                    toPos = toPos
                }, true);
            });
        }

        Helpers.EmmitGridSaveRequest(World);
    }

    private bool IsValidCell(GridComponent gridComponent, int x, int y)
    {
        return !gridComponent.state[x, y] && !gridComponent.elements[x, y].HasValue;
    }

    private bool CanElementFall(GridComponent gridComponent, Vector2Int position)
    {
        int checkY = position.y - 1;
        return checkY >= 0 && IsValidCell(gridComponent, position.x, checkY);
    }

    private void UnlockCellsBetween(ref GridComponent gridComponent, Vector2Int fromPos, Vector2Int toPos)
    {
        var pathCells = GetCellsBetween(fromPos, toPos);

        foreach (var cell in pathCells)
        {
            if (!Helpers.IsWithinGridBounds(cell, gridComponent.elements))
                continue;

            gridComponent.state[cell.x, cell.y] = false;
        }
    }

    private List<Vector2Int> GetCellsBetween(Vector2Int fromPos, Vector2Int toPos)
    {
        var cells = new List<Vector2Int>();

        int x0 = fromPos.x;
        int y0 = fromPos.y;
        int x1 = toPos.x;
        int y1 = toPos.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        int x = x0;
        int y = y0;

        while (true)
        {
            cells.Add(new Vector2Int(x, y));

            if (x == x1 && y == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

        return cells;
    }

    public void Dispose() { }
}