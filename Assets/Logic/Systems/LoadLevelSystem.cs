using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class LoadLevelSystem : ISystem
{
    public World World { get; set; }

    private Request<LoadLevelRequest> loadLevelRequest;

    private readonly Configuration config;
    private readonly Camera camera;

    public LoadLevelSystem(Configuration config, Camera camera)
    {
        this.config = config;
        this.camera = camera;
    }

    public void OnAwake()
    {
        loadLevelRequest = World.Default.GetRequest<LoadLevelRequest>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var request in loadLevelRequest.Consume())
        {
            var level = config.levels[request.levelIndex];
            var levelEntity = World.CreateEntity();
            var levelComponents = World.GetStash<LevelComponent>();
            ref var levelComponent = ref levelComponents.Add(levelEntity);
            levelComponent.camera = camera;
            levelComponent.background = level.background;
            levelComponent.backgroundScale = level.backgroundScale;
            levelComponent.balloonsPreset = level.balloonsPreset;
            levelComponent.screenPadding = level.screenPadding;
            levelComponent.gridOffset = level.gridOffset;
            levelComponent.maxGridSizeInUnits = level.maxGridSizeInUnits;
            levelComponent.cellSize = level.cellSize;
            levelComponent.elementSize = level.elementSize;
            levelComponent.swapEasing = level.swapEasing;
            levelComponent.swapDuration = level.swapDuration;
            levelComponent.fallEasing = level.fallEasing;
            levelComponent.fallDuration = level.fallDuration;
            levelComponent.width = level.width;
            levelComponent.height = level.height;
            levelComponent.grid = request.gridData ?? level.grid;
            levelComponent.elements = level.elementDataBase.elements;

            var loadedLevelEvent = World.GetEvent<LoadedLevelEvent>();
            loadedLevelEvent.NextFrame(new LoadedLevelEvent
            {
                levelEntity = levelEntity
            });
        }
    }

    public void Dispose() { }
}