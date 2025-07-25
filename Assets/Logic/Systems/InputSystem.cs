using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class InputSystem : ISystem
{
    public World World { get; set; }

    private Filter levelFilter;
    private Stash<LevelComponent> levelComponents;

    private Entity inputStateEntity;
    private Stash<InputStateComponent> inputStateComponents;

    private Event<SwapEvent> swapElementsEvent;

    public void OnAwake()
    {
        levelFilter = World.Filter.With<LevelComponent>().Build();
        levelComponents = World.GetStash<LevelComponent>();
        inputStateComponents = World.GetStash<InputStateComponent>();
        swapElementsEvent = World.GetEvent<SwapEvent>();

        inputStateEntity = World.CreateEntity();
        ref var inputState = ref inputStateComponents.Add(inputStateEntity);
        inputState.isDown = false;
        inputState.position = Vector2Int.zero;
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var entity in this.levelFilter)
        {
            ref var levelComponent = ref levelComponents.Get(entity);

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Input.mousePosition;
                Vector3 worldPosition = levelComponent.camera.ScreenToWorldPoint(mousePosition);
                worldPosition.z = 0;

                Vector2Int gridPosition = Helpers.WorldToGridPosition(worldPosition, levelComponent);

                bool inputCondition = gridPosition.x >= 0 && gridPosition.x < levelComponent.width &&
                    gridPosition.y >= 0 && gridPosition.y < levelComponent.height;
                if (!inputCondition) continue;

                ref var inputState = ref inputStateComponents.Get(inputStateEntity);
                inputState.position = gridPosition;
                inputState.isDown = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                ref var inputState = ref inputStateComponents.Get(inputStateEntity);

                if (!inputState.isDown) continue;

                Vector3 mousePosition = Input.mousePosition;
                Vector3 worldPosition = levelComponent.camera.ScreenToWorldPoint(mousePosition);
                worldPosition.z = 0;

                Vector2Int gridPosition = Helpers.WorldToGridPosition(worldPosition, levelComponent);

                Vector2Int firstPosition = inputState.position;
                Vector2Int direction = DetermineSwipeDirection(firstPosition, gridPosition);
                if (IsValidDirection(direction))
                {
                    Vector2Int neighborPosition = firstPosition + direction;
                    if (neighborPosition.x >= 0 && neighborPosition.x < levelComponent.width &&
                        neighborPosition.y >= 0 && neighborPosition.y < levelComponent.height)
                    {
                        swapElementsEvent.NextFrame(new SwapEvent
                        {
                            from = firstPosition,
                            to = neighborPosition
                        });
                    }
                }

                inputState.isDown = false;
            }
        }
    }

    private Vector2Int DetermineSwipeDirection(Vector2Int from, Vector2Int to)
    {
        Vector2Int delta = to - from;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return new Vector2Int(delta.x > 0 ? 1 : -1, 0);
        else
            return new Vector2Int(0, delta.y > 0 ? 1 : -1);
    }

    private bool IsValidDirection(Vector2Int direction)
    {
        return (direction == Vector2Int.up) ||
               (direction == Vector2Int.down) ||
               (direction == Vector2Int.left) ||
               (direction == Vector2Int.right);
    }

    public void Dispose() { }
}