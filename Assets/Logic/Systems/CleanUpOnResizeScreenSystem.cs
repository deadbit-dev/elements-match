using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using DG.Tweening;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public class CleanUpOnResizeScreenSystem : ISystem
{
    public World World { get; set; }

    private Event<ResizeScreenEvent> resizeScreenEvent;

    private Filter elementInTweenFilter;
    private Stash<TweenComponent> tweenComponents;

    private Filter elementInDestroyAnimationFilter;
    private Stash<DestroyAnimationTagComponent> destroyAnimationTagComponents;
    private Stash<ViewRefComponent> viewRefComponents;

    public void OnAwake()
    {
        resizeScreenEvent = World.GetEvent<ResizeScreenEvent>();

        elementInTweenFilter = World.Filter.With<ElementComponent>().With<TweenComponent>().Build();
        tweenComponents = World.GetStash<TweenComponent>();

        elementInDestroyAnimationFilter = World.Filter.With<ElementComponent>().With<DestroyAnimationTagComponent>().Build();

        viewRefComponents = World.GetStash<ViewRefComponent>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var _ in resizeScreenEvent.publishedChanges)
        {
            foreach (var elementEntity in elementInTweenFilter)
            {
                ref var tweenComponent = ref tweenComponents.Get(elementEntity);
                tweenComponent.tween.Complete(true);
                tweenComponent.tween.Kill();
                tweenComponents.Remove(elementEntity);
            }

            foreach (var elementEntity in elementInDestroyAnimationFilter)
            {
                if (!viewRefComponents.Has(elementEntity))
                    continue;

                ref var viewRefComponent = ref viewRefComponents.Get(elementEntity);
                viewRefComponent.viewRef.GetComponent<ElementView>().OnEndDestroy();
            }
        }
    }

    public void Dispose() { }
}