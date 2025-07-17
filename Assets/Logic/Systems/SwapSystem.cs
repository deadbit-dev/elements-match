using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class SwapSystem : ISystem
{
    public World World { get; set; }

    private Event<SwapEvent> swapElementsEvent;

    public void OnAwake()
    {
        swapElementsEvent = World.GetEvent<SwapEvent>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var eventData in swapElementsEvent.publishedChanges)
        {
            Debug.Log($"Swap event: from {eventData.from} to {eventData.to}");
            // TODO: Swap logic
        }
    }

    public void Dispose() { }
}