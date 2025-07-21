using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class SwapSystem : ISystem
{
    public World World { get; set; }

    private Event<SwapEvent> swapElementsEvent;

    private Filter gridFilter;
    private Stash<GridComponent> gridComponents;

    private Filter levelFilter;
    private Stash<LevelComponent> levelComponents;

    private Filter elementFilter;
    private Stash<ViewRefComponent> viewRefComponents;

    private Request<SwapTweenCompleteRequest> swapTweenCompleteRequest;

    public void OnAwake()
    {
        swapElementsEvent = World.GetEvent<SwapEvent>();
        gridFilter = World.Filter.With<GridComponent>().Build();
        gridComponents = World.GetStash<GridComponent>();

        levelFilter = World.Filter.With<LevelComponent>().Build();
        levelComponents = World.GetStash<LevelComponent>();

        elementFilter = World.Filter.With<ElementComponent>().With<ViewRefComponent>().Without<TweenComponent>().Build();
        viewRefComponents = World.GetStash<ViewRefComponent>();

        swapTweenCompleteRequest = World.GetRequest<SwapTweenCompleteRequest>();
    }

    public void OnUpdate(float deltaTime)
    {
        ProcessSwapEvent();
        ProcessSwapTweenCompleteRequest();
    }

    private void ProcessSwapEvent()
    {
        foreach (var eventData in swapElementsEvent.publishedChanges)
        {
            Debug.Log($"Swap event: from {eventData.from} to {eventData.to}");

            if (IsValidSwap(eventData.from, eventData.to))
                Swap(eventData.from, eventData.to);
        }
    }

    private bool IsValidSwap(Vector2Int from, Vector2Int to)
    {
        if (gridFilter.IsEmpty())
            return false;

        var gridEntity = gridFilter.First();
        ref var gridComponent = ref gridComponents.Get(gridEntity);

        if (!Helpers.IsWithinGridBounds(from, gridComponent.elements) || !Helpers.IsWithinGridBounds(to, gridComponent.elements))
        {
            Debug.LogWarning($"Position out of grid bounds: from={from}, to={to}");
            return false;
        }

        if (!IsAdjacentSwap(from, to))
        {
            Debug.LogWarning($"Swap is not adjacent: from={from}, to={to}");
            return false;
        }

        if (gridComponent.state[from.x, from.y] || gridComponent.state[to.x, to.y])
        {
            Debug.Log($"One or both cells are busy: from state={gridComponent.state[from.x, from.y]}, to state={gridComponent.state[to.x, to.y]}");
            return false;
        }

        Vector2Int swapDirection = to - from;

        if (!HasElement(gridComponent.elements[from.x, from.y]))
        {
            Debug.LogWarning($"Cannot swap: source cell doesn't have element: from hasElement={HasElement(gridComponent.elements[from.x, from.y])}");
            return false;
        }

        if (swapDirection == Vector2Int.up && !HasElement(gridComponent.elements[to.x, to.y]))
        {
            Debug.LogWarning($"Cannot swap up to empty cell: to hasElement={HasElement(gridComponent.elements[to.x, to.y])}");
            return false;
        }

        return true;
    }

    private bool IsAdjacentSwap(Vector2Int from, Vector2Int to)
    {
        Vector2Int difference = to - from;

        return (difference == Vector2Int.up) ||
               (difference == Vector2Int.down) ||
               (difference == Vector2Int.left) ||
               (difference == Vector2Int.right);
    }

    private bool HasElement(Entity? elementEntity)
    {
        return elementEntity != null;
    }

    private void Swap(Vector2Int from, Vector2Int to)
    {
        if (levelFilter.IsEmpty() || gridFilter.IsEmpty())
            return;

        var levelEntity = levelFilter.First();
        ref var levelComponent = ref levelComponents.Get(levelEntity);

        var gridEntity = gridFilter.First();
        ref var gridComponent = ref gridComponents.Get(gridEntity);

        Entity? fromElement = gridComponent.elements[from.x, from.y];
        Entity? toElement = gridComponent.elements[to.x, to.y];

        gridComponent.elements[from.x, from.y] = toElement;
        gridComponent.elements[to.x, to.y] = fromElement;

        Helpers.EmmitGridSaveRequest(World);

        if (fromElement.HasValue)
        {
            gridComponent.state[to.x, to.y] = true;
            Helpers.TweenElement(World, levelComponent, gridEntity, fromElement.Value, from, to, levelComponent.swapEasing, levelComponent.swapDuration, () =>
            {
                World.GetRequest<SwapTweenCompleteRequest>().Publish(new SwapTweenCompleteRequest
                {
                    gridEntity = gridEntity,
                    elementEntity = fromElement.Value,
                    pos = to,
                }, true);
            });
        }

        if (toElement.HasValue)
        {
            gridComponent.state[from.x, from.y] = true;
            Helpers.TweenElement(World, levelComponent, gridEntity, toElement.Value, to, from, levelComponent.swapEasing, levelComponent.swapDuration, () =>
            {
                World.GetRequest<SwapTweenCompleteRequest>().Publish(new SwapTweenCompleteRequest
                {
                    gridEntity = gridEntity,
                    elementEntity = toElement.Value,
                    pos = from,
                }, true);
            });
        }
    }

    private void ProcessSwapTweenCompleteRequest()
    {
        foreach (var request in swapTweenCompleteRequest.Consume())
        {
            ref var gridComponent = ref gridComponents.Get(request.gridEntity);
            gridComponent.state[request.pos.x, request.pos.y] = false;
        }
    }

    public void Dispose() { }
}