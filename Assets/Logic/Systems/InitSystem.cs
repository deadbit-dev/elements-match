using Unity.IL2CPP.CompilerServices;
using Scellecs.Morpeh;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class InitSystem : IInitializer
{
    public World World { get; set; }

    private Configuration config;

    public InitSystem(Configuration config)
    {
        this.config = config;
    }

    public void OnAwake()
    {
        var levelIndex = config.startLevelIndex;
        if (Helpers.LoadData(out SaveData saveData))
            levelIndex = saveData.currentLevelIndex;

        var gameStateEntity = World.CreateEntity();
        ref var gameStateComponent = ref World.GetStash<GameStateComponent>().Add(gameStateEntity);
        gameStateComponent.currentLevelIndex = levelIndex;
        gameStateComponent.levelCounts = config.levels.Length;

        var loadLevelEvent = World.GetRequest<LoadLevelRequest>();
        loadLevelEvent.Publish(new LoadLevelRequest
        {
            levelIndex = levelIndex,
            gridData = saveData.gridData
        });
    }

    public void Dispose() { }
}