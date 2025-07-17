using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class BackgroundSystem : ISystem
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

            var backgroundGameObject = new GameObject(levelComponent.background.name);
            backgroundGameObject.transform.localScale = new Vector3(0.45f, 0.45f, 1);
            var spriteRenderer = backgroundGameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = levelComponent.background;
            spriteRenderer.sortingOrder = -1;
        }
    }

    public void Dispose() { }
}