using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class GridAdaptationSystem : ISystem
{
    public World World { get; set; }

    private Filter levelFilter;
    private Stash<LevelComponent> levelComponents;

    private Filter gridFilter;
    private Stash<GridComponent> gridComponents;

    private Filter elementFilter;
    private Stash<ViewRefComponent> viewRefComponents;

    public void OnAwake()
    {
        levelFilter = this.World.Filter.With<LevelComponent>().Build();
        levelComponents = this.World.GetStash<LevelComponent>();

        gridFilter = this.World.Filter.With<GridComponent>().Build();
        gridComponents = this.World.GetStash<GridComponent>();

        elementFilter = this.World.Filter.With<ElementComponent>().With<ViewRefComponent>().Build();
        viewRefComponents = this.World.GetStash<ViewRefComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var levelEntity in this.levelFilter)
        {
            ref var levelComponent = ref levelComponents.Get(levelEntity);
            foreach (var elementEntity in this.elementFilter)
            {
                ref var viewRefComponent = ref viewRefComponents.Get(elementEntity);

                foreach (var gridEntity in this.gridFilter)
                {
                    ref var gridComponent = ref gridComponents.Get(gridEntity);
                    Vector2Int position = Helpers.FindElementPositionInGrid(gridComponent.cells, elementEntity);

                    if (position.x == -1 || position.y == -1)
                        break;

                    Helpers.ApplyElementTransform(viewRefComponent.viewRef.transform, position, levelComponent);
                }
            }
        }
    }

    public void Dispose() { }
}