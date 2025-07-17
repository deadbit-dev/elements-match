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

    public void OnAwake()
    {
        swapElementsEvent = World.GetEvent<SwapEvent>();
        gridFilter = World.Filter.With<GridComponent>().Build();
        gridComponents = World.GetStash<GridComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var eventData in swapElementsEvent.publishedChanges)
        {
            Debug.Log($"Swap event: from {eventData.from} to {eventData.to}");

            if (IsValidSwap(eventData.from, eventData.to))
            {
                Debug.Log("Swap is valid - can proceed with swap logic");
            }
            else
            {
                Debug.Log("Swap is invalid - ignoring swap request");
            }
        }
    }

    private bool IsValidSwap(Vector2Int from, Vector2Int to)
    {
        var gridEntity = gridFilter.First();
        ref var gridComponent = ref gridComponents.Get(gridEntity);

        if (!IsWithinGridBounds(from, gridComponent.elements) || !IsWithinGridBounds(to, gridComponent.elements))
        {
            Debug.Log($"Position out of grid bounds: from={from}, to={to}");
            return false;
        }

        if (!IsAdjacentSwap(from, to))
        {
            Debug.Log($"Swap is not adjacent: from={from}, to={to}");
            return false;
        }

        if (gridComponent.state[from.x, from.y] || gridComponent.state[to.x, to.y])
        {
            Debug.Log($"One or both cells are busy: from state={gridComponent.state[from.x, from.y]}, to state={gridComponent.state[to.x, to.y]}");
            return false;
        }

        Vector2Int swapDirection = to - from;
        if (swapDirection == Vector2Int.up)
        {
            if (!HasElement(gridComponent.elements[from.x, from.y]) || !HasElement(gridComponent.elements[to.x, to.y]))
            {
                Debug.Log($"Cannot swap up: one or both cells don't have elements: from hasElement={HasElement(gridComponent.elements[from.x, from.y])}, to hasElement={HasElement(gridComponent.elements[to.x, to.y])}");
                return false;
            }
        }
        else
        {
            if (!HasElement(gridComponent.elements[from.x, from.y]))
            {
                Debug.Log($"Cannot swap: source cell doesn't have element: from hasElement={HasElement(gridComponent.elements[from.x, from.y])}");
                return false;
            }
        }

        return true;
    }

    private bool IsWithinGridBounds(Vector2Int position, Entity?[,] elements)
    {
        return position.x >= 0 && position.x < elements.GetLength(0) &&
               position.y >= 0 && position.y < elements.GetLength(1);
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

    public void Dispose() { }
}