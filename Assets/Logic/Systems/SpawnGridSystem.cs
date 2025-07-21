using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class SpawnGridSystem : ISystem
{
    public World World { get; set; }

    private Filter gridFilter;
    private Stash<ViewRefComponent> viewRefComponents;

    private readonly GridDebugRenderer gridDebugRenderer;

    public SpawnGridSystem(GridDebugRenderer gridDebugRenderer)
    {
        this.gridDebugRenderer = gridDebugRenderer;
    }

    public void OnAwake()
    {
        gridFilter = World.Filter.With<GridComponent>().Without<ViewRefComponent>().Build();
        viewRefComponents = World.GetStash<ViewRefComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        if (gridFilter.IsEmpty())
            return;

#if UNITY_EDITOR
        if (gridDebugRenderer != null)
            GameObject.Instantiate(gridDebugRenderer.gameObject);
#endif

        var gridEntity = gridFilter.First();
        ref var viewRefComponent = ref viewRefComponents.Add(gridEntity);
        viewRefComponent.viewRef = new GameObject("Grid");
    }

    public void Dispose() { }
}