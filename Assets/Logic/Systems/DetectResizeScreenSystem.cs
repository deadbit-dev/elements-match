using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public class DetectResizeScreenSystem : ISystem
{
    public World World { get; set; }

    private Filter gameStateFilter;
    private Stash<GameStateComponent> gameStateComponents;

    private Event<ResizeScreenEvent> resizeScreenEvent;

    public void OnAwake()
    {
        gameStateFilter = World.Filter.With<GameStateComponent>().Build();
        gameStateComponents = World.GetStash<GameStateComponent>();

        resizeScreenEvent = World.GetEvent<ResizeScreenEvent>();
    }

    public void OnUpdate(float deltaTime)
    {
        if (gameStateFilter.IsEmpty())
            return;

        var gameStateEntity = gameStateFilter.First();
        ref var gameStateComponent = ref gameStateComponents.Get(gameStateEntity);

        if (gameStateComponent.screenWidth != Screen.width || gameStateComponent.screenHeight != Screen.height)
        {
            gameStateComponent.screenWidth = Screen.width;
            gameStateComponent.screenHeight = Screen.height;

            resizeScreenEvent.NextFrame(new ResizeScreenEvent
            {
                screenWidth = Screen.width,
                screenHeight = Screen.height
            });
        }
    }

    public void Dispose() { }
}