using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class WinSystem : ISystem
{
    public World World { get; set; }

    private Filter gridFilter;
    private Stash<GridComponent> gridComponents;

    private Filter gameStateFilter;
    private Stash<GameStateComponent> gameStateComponents;

    public void OnAwake()
    {
        gridFilter = World.Filter.With<GridComponent>().Build();
        gridComponents = World.GetStash<GridComponent>();

        gameStateFilter = World.Filter.With<GameStateComponent>().Build();
        gameStateComponents = World.GetStash<GameStateComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        if (gameStateFilter.IsEmpty() || gridFilter.IsEmpty())
            return;

        var gridEntity = gridFilter.First();
        ref var gridComponent = ref gridComponents.Get(gridEntity);

        if (!Helpers.IsGridEmpty(gridComponent))
            return;

        if (!Helpers.IsAllCellsUnlocked(gridComponent))
            return;

        TransitionToNextLevel();
    }

    private void TransitionToNextLevel()
    {
        var gameStateEntity = gameStateFilter.First();
        ref var gameStateComponent = ref gameStateComponents.Get(gameStateEntity);

        var currentLevelIndex = gameStateComponent.currentLevelIndex;
        var nextLevelIndex = (currentLevelIndex + 1) % gameStateComponent.levelCounts;
        gameStateComponent.currentLevelIndex = nextLevelIndex;

        Helpers.EmmitLevelIndexSaveRequest(World);
        Helpers.EmmitClearGridSaveRequest(World);
        Helpers.EmmitReloadSceneRequest(World);
    }

    public void Dispose() { }
}
