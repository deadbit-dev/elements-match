using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class SpawnBackgroundSystem : ISystem
{
    public World World { get; set; }

    private Event<LoadedLevelEvent> loadedLevelEvent;

    public void OnAwake()
    {
        loadedLevelEvent = World.GetEvent<LoadedLevelEvent>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var eventData in loadedLevelEvent.publishedChanges)
        {
            var levelComponents = World.GetStash<LevelComponent>();
            ref var levelComponent = ref levelComponents.Get(eventData.levelEntity);

            var backgroundGameObject = new GameObject(levelComponent.background.name);
            backgroundGameObject.transform.localScale = levelComponent.backgroundScale;
            var spriteRenderer = backgroundGameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = levelComponent.background;
            spriteRenderer.sortingOrder = -1;
        }
    }

    public void Dispose() { }
}