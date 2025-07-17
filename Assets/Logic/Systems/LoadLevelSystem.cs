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
            var levelEntity = this.World.CreateEntity();
            var levelComponents = this.World.GetStash<LevelComponent>();
            ref var levelComponent = ref levelComponents.Add(levelEntity);
            levelComponent.camera = camera;
            levelComponent.background = level.background;
            levelComponent.width = level.width;
            levelComponent.height = level.height;
            levelComponent.grid = level.grid;
            levelComponent.elementSize = level.elementSize;
            levelComponent.maxGridSize = level.maxGridSize;
            levelComponent.screenPadding = level.screenPadding;
            levelComponent.gridOffset = level.gridOffset;
            levelComponent.elements = level.elementDataBase.elements;

            var loadedLevelEvent = this.World.GetEvent<LoadedLevelEvent>();
            loadedLevelEvent.NextFrame(new LoadedLevelEvent
            {
                levelEntity = levelEntity
            });
        }
    }

    public void Dispose() { }
}