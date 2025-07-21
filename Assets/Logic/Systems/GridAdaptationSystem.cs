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
        levelFilter = World.Filter.With<LevelComponent>().Build();
        levelComponents = World.GetStash<LevelComponent>();

        gridFilter = World.Filter.With<GridComponent>().Build();
        gridComponents = World.GetStash<GridComponent>();

        elementFilter = World.Filter.With<ElementComponent>().With<ViewRefComponent>().Without<TweenComponent>().Build();
        viewRefComponents = World.GetStash<ViewRefComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        if (levelFilter.IsEmpty() || gridFilter.IsEmpty())
            return;

        var levelEntity = levelFilter.First();
        ref var levelComponent = ref levelComponents.Get(levelEntity);

        var gridEntity = gridFilter.First();
        ref var gridComponent = ref gridComponents.Get(gridEntity);

        foreach (var elementEntity in elementFilter)
        {
            ref var viewRefComponent = ref viewRefComponents.Get(elementEntity);

            Vector2Int position = Helpers.FindElementPositionInGrid(gridComponent.elements, elementEntity);

            if (position.x == -1 || position.y == -1)
                break;

            Helpers.ApplyElementTransform(viewRefComponent.viewRef.transform, position, levelComponent);
        }
    }

    public void Dispose() { }
}