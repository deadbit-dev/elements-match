using Scellecs.Morpeh;
using System;
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

    public void OnAwake()
    {
        gridFilter = this.World.Filter.With<GridComponent>().Without<ViewRefComponent>().Build();
        viewRefComponents = this.World.GetStash<ViewRefComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var gridEntity in this.gridFilter)
        {
            ref var viewRefComponent = ref viewRefComponents.Add(gridEntity);
            viewRefComponent.viewRef = new GameObject("Grid");
        }
    }

    public void Dispose() { }
}