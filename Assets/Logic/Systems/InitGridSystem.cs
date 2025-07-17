using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class InitGridSystem : ISystem
{
    public World World { get; set; }

    private Event<LoadedLevelEvent> loadedLevelEvent;

    public void OnAwake()
    {
        loadedLevelEvent = this.World.GetEvent<LoadedLevelEvent>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var eventData in loadedLevelEvent.publishedChanges)
        {
            var levelComponents = this.World.GetStash<LevelComponent>();
            ref var levelComponent = ref levelComponents.Get(eventData.levelEntity);

            var gridEntity = this.World.CreateEntity();
            var gridComponents = this.World.GetStash<GridComponent>();
            ref var gridComponent = ref gridComponents.Add(gridEntity);
            gridComponent.elements = new Entity?[levelComponent.width, levelComponent.height];
            gridComponent.state = new bool[levelComponent.width, levelComponent.height];

            for (int y = 0; y < levelComponent.height; y++)
            {
                for (int x = 0; x < levelComponent.width; x++)
                {
                    int elementType = levelComponent.grid[y * levelComponent.width + x];

                    if (elementType == -1)
                        continue;

                    var elementEntity = this.World.CreateEntity();
                    var elementComponents = this.World.GetStash<ElementComponent>();
                    ref var elementComponent = ref elementComponents.Add(elementEntity);
                    elementComponent.type = elementType;

                    gridComponent.state[x, y] = false;
                    gridComponent.elements[x, y] = elementEntity;
                }
            }
        }
    }

    public void Dispose() { }
}