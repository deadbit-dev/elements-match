using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class SpawnElementViewSystem : ISystem
{
    public World World { get; set; }

    private Filter levelFilter;
    private Stash<LevelComponent> levelComponents;

    private Filter gridFilter;
    private Stash<GridComponent> gridComponents;

    private Stash<ViewRefComponent> viewRefComponents;

    private Filter elementFilter;
    private Stash<ElementComponent> elementComponents;

    public void OnAwake()
    {
        levelFilter = World.Filter.With<LevelComponent>().Build();
        levelComponents = World.GetStash<LevelComponent>();

        gridFilter = World.Filter.With<GridComponent>().Build();
        gridComponents = World.GetStash<GridComponent>();

        viewRefComponents = World.GetStash<ViewRefComponent>();

        elementFilter = World.Filter.With<ElementComponent>().Without<ViewRefComponent>().Build();
        elementComponents = World.GetStash<ElementComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        if (levelFilter.IsEmpty() || gridFilter.IsEmpty())
            return;

        var levelEntity = levelFilter.First();
        ref var levelComponent = ref levelComponents.Get(levelEntity);

        var gridEntity = gridFilter.First();
        ref var gridComponent = ref gridComponents.Get(gridEntity);

        ref var gridViewRefComponent = ref viewRefComponents.Get(gridEntity);

        foreach (var elementEntity in elementFilter)
        {
            ref var elementComponent = ref elementComponents.Get(elementEntity);
            var elementData = levelComponent.elements[elementComponent.type];

            GameObject view = GameObject.Instantiate(elementData.prefab, gridViewRefComponent.viewRef.transform);
            view.GetComponent<ElementView>().SetIdleWithDelay(Random.Range(0f, 3f));

            ref var viewRefComponent = ref viewRefComponents.Add(elementEntity);
            viewRefComponent.viewRef = view.gameObject;

            Vector2Int position = Helpers.FindElementPositionInGrid(gridComponent.elements, elementEntity);
            if (position.x == -1 || position.y == -1)
                break;

            Helpers.ApplyElementTransform(view.transform, position, levelComponent);
        }
    }

    public void Dispose() { }
}