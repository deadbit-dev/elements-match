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
        levelFilter = this.World.Filter.With<LevelComponent>().Build();
        levelComponents = this.World.GetStash<LevelComponent>();

        gridFilter = this.World.Filter.With<GridComponent>().Build();
        gridComponents = this.World.GetStash<GridComponent>();

        viewRefComponents = this.World.GetStash<ViewRefComponent>();

        elementFilter = this.World.Filter.With<ElementComponent>().Without<ViewRefComponent>().Build();
        elementComponents = this.World.GetStash<ElementComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var levelEntity in this.levelFilter)
        {
            ref var levelComponent = ref levelComponents.Get(levelEntity);

            foreach (var gridEntity in this.gridFilter)
            {
                ref var gridComponent = ref gridComponents.Get(gridEntity);
                ref var gridViewRefComponent = ref viewRefComponents.Get(gridEntity);

                foreach (var elementEntity in this.elementFilter)
                {
                    ref var elementComponent = ref elementComponents.Get(elementEntity);
                    var elementData = levelComponent.elements[elementComponent.type];

                    GameObject view = GameObject.Instantiate(elementData.view, gridViewRefComponent.viewRef.transform);

                    ref var viewRefComponent = ref viewRefComponents.Add(elementEntity);
                    viewRefComponent.viewRef = view;

                    Vector2Int position = Helpers.FindElementPositionInGrid(gridComponent.elements, elementEntity);
                    if (position.x == -1 || position.y == -1)
                        break;

                    Helpers.ApplyElementTransform(view.transform, position, levelComponent);
                }
            }
        }
    }

    public void Dispose() { }
}