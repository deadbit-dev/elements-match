using Scellecs.Morpeh;
using UnityEngine;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public class DestroyAreaSystem : ISystem
{
    public World World { get; set; }

    private Filter gridFilter;
    private Stash<GridComponent> gridComponents;
    private Stash<ElementComponent> elementComponents;
    private Stash<ViewRefComponent> viewRefComponents;
    private Stash<DestroyAnimationTagComponent> destroyAnimationTagComponents;
    private Request<DestroyAnimationEndRequest> destroyAnimationEndRequest;

    public void OnAwake()
    {
        gridFilter = World.Filter.With<GridComponent>().Build();
        gridComponents = World.GetStash<GridComponent>();
        elementComponents = World.GetStash<ElementComponent>();
        viewRefComponents = World.GetStash<ViewRefComponent>();
        destroyAnimationTagComponents = World.GetStash<DestroyAnimationTagComponent>();
        destroyAnimationEndRequest = World.GetRequest<DestroyAnimationEndRequest>();
    }

    public void OnUpdate(float deltaTime)
    {
        if (gridFilter.IsEmpty())
            return;

        var gridEntity = gridFilter.First();
        ref var gridComponent = ref gridComponents.Get(gridEntity);

        ProcessArea(gridComponent);
        ProcessDestroyAnimationEndRequest(gridComponent);
    }

    private void ProcessArea(GridComponent gridComponent)
    {
        List<List<Entity>> destroyAreas = new List<List<Entity>>();
        HashSet<Entity> processedEntities = new HashSet<Entity>();

        for (int y = gridComponent.elements.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < gridComponent.elements.GetLength(0); x++)
            {
                if (!gridComponent.elements[x, y].HasValue || gridComponent.state[x, y])
                    continue;

                Entity currentEntity = gridComponent.elements[x, y].Value;
                if (processedEntities.Contains(currentEntity))
                    continue;

                List<Entity> connectedArea = FindArea(gridComponent, new Vector2Int(x, y), processedEntities);

                if (connectedArea.Count <= 1)
                    continue;

                if (IsAreaValid(connectedArea, gridComponent))
                    destroyAreas.Add(connectedArea);
            }
        }

        if (destroyAreas.Count == 0)
            return;

        Debug.Log($"Destroy areas: {destroyAreas.Count}");

        foreach (var area in destroyAreas)
        {
            foreach (var entity in area)
            {
                Vector2Int position = Helpers.FindElementPositionInGrid(gridComponent.elements, entity);

                if (position.x != -1 && position.y != -1)
                {
                    gridComponent.state[position.x, position.y] = true;
                    Debug.Log($"Set grid state to true: {position}");
                }

                if (!viewRefComponents.Has(entity))
                    continue;

                destroyAnimationTagComponents.Add(entity);

                ref var viewRefComponent = ref viewRefComponents.Get(entity);
                viewRefComponent.viewRef.GetComponent<ElementView>().SetDestroy(() =>
                {
                    destroyAnimationEndRequest.Publish(new DestroyAnimationEndRequest
                    {
                        elementEntity = entity,
                        position = position
                    }, true);
                });

                if (position.x != -1 && position.y != -1)
                    gridComponent.elements[position.x, position.y] = null;
            }
        }

        Helpers.EmmitGridSaveRequest(World);
    }

    private List<Entity> FindArea(GridComponent gridComponent, Vector2Int startPos, HashSet<Entity> processedEntities)
    {
        List<Entity> area = new List<Entity>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        if (!gridComponent.elements[startPos.x, startPos.y].HasValue)
            return area;

        Entity startEntity = gridComponent.elements[startPos.x, startPos.y].Value;
        int elementType = elementComponents.Get(startEntity).type;

        FindConnectedElementsRecursive(gridComponent, startPos, elementType, area, visited, processedEntities);

        return area;
    }

    private void FindConnectedElementsRecursive(GridComponent gridComponent, Vector2Int pos, int elementType,
        List<Entity> area, HashSet<Vector2Int> visited, HashSet<Entity> processedEntities)
    {
        if (visited.Contains(pos))
            return;

        if (!Helpers.IsWithinGridBounds(pos, gridComponent.elements))
            return;

        if (!gridComponent.elements[pos.x, pos.y].HasValue || gridComponent.state[pos.x, pos.y])
            return;

        Entity currentEntity = gridComponent.elements[pos.x, pos.y].Value;

        int currentType = elementComponents.Get(currentEntity).type;
        if (currentType != elementType)
            return;

        if (processedEntities.Contains(currentEntity))
            return;

        area.Add(currentEntity);
        visited.Add(pos);
        processedEntities.Add(currentEntity);

        Vector2Int[] directions = {
            new Vector2Int(0, 1),  // сверху
            new Vector2Int(1, 0),  // справа
            new Vector2Int(0, -1), // снизу
            new Vector2Int(-1, 0)  // слева
        };

        foreach (var direction in directions)
        {
            Vector2Int neighborPos = pos + direction;
            FindConnectedElementsRecursive(gridComponent, neighborPos, elementType, area, visited, processedEntities);
        }
    }

    private bool IsAreaValid(List<Entity> area, GridComponent gridComponent)
    {
        if (HasValidHorizontalLines(area, gridComponent))
            return true;

        if (HasValidVerticalLines(area, gridComponent))
            return true;

        return false;
    }

    private bool HasValidHorizontalLines(List<Entity> area, GridComponent gridComponent)
    {
        Dictionary<int, List<Entity>> elementsByRow = new Dictionary<int, List<Entity>>();

        foreach (var entity in area)
        {
            Vector2Int pos = Helpers.FindElementPositionInGrid(gridComponent.elements, entity);
            if (pos.x != -1 && pos.y != -1)
            {
                if (!elementsByRow.ContainsKey(pos.y))
                    elementsByRow[pos.y] = new List<Entity>();
                elementsByRow[pos.y].Add(entity);
            }
        }

        foreach (var row in elementsByRow.Values)
        {
            if (row.Count >= 3)
            {
                List<Vector2Int> positions = new List<Vector2Int>();
                foreach (var entity in row)
                {
                    positions.Add(Helpers.FindElementPositionInGrid(gridComponent.elements, entity));
                }

                positions.Sort((a, b) => a.x.CompareTo(b.x));

                int consecutiveCount = 1;
                for (int i = 1; i < positions.Count; i++)
                {
                    if (positions[i].x == positions[i - 1].x + 1) consecutiveCount++;
                    else consecutiveCount = 1;

                    if (consecutiveCount >= 3)
                        return true;
                }
            }
        }

        return false;
    }

    private bool HasValidVerticalLines(List<Entity> area, GridComponent gridComponent)
    {
        Dictionary<int, List<Entity>> elementsByColumn = new Dictionary<int, List<Entity>>();

        foreach (var entity in area)
        {
            Vector2Int pos = Helpers.FindElementPositionInGrid(gridComponent.elements, entity);
            if (pos.x != -1 && pos.y != -1)
            {
                if (!elementsByColumn.ContainsKey(pos.x))
                    elementsByColumn[pos.x] = new List<Entity>();
                elementsByColumn[pos.x].Add(entity);
            }
        }

        foreach (var column in elementsByColumn.Values)
        {
            if (column.Count >= 3)
            {
                List<Vector2Int> positions = new List<Vector2Int>();
                foreach (var entity in column)
                {
                    positions.Add(Helpers.FindElementPositionInGrid(gridComponent.elements, entity));
                }

                positions.Sort((a, b) => a.y.CompareTo(b.y));

                int consecutiveCount = 1;
                for (int i = 1; i < positions.Count; i++)
                {
                    if (positions[i].y == positions[i - 1].y + 1) consecutiveCount++;
                    else consecutiveCount = 1;

                    if (consecutiveCount >= 3)
                        return true;
                }
            }
        }

        return false;
    }

    private void ProcessDestroyAnimationEndRequest(GridComponent gridComponent)
    {
        foreach (var request in destroyAnimationEndRequest.Consume())
        {
            gridComponent.state[request.position.x, request.position.y] = false;
            World.RemoveEntity(request.elementEntity);
        }
    }

    public void Dispose() { }
}