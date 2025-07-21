using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public class UISystem : ISystem
{
    public World World { get; set; }

    private Filter gameStateFilter;
    private Stash<GameStateComponent> gameStateComponents;

    private Filter buttonFilter;
    private Stash<ButtonComponent> buttonComponents;

    private readonly GameObject gui;

    public UISystem(GameObject gui)
    {
        this.gui = gui;
    }

    public void OnAwake()
    {
        gameStateFilter = World.Filter.With<GameStateComponent>().Build();
        gameStateComponents = World.GetStash<GameStateComponent>();

        buttonFilter = World.Filter.With<ButtonComponent>().Build();
        buttonComponents = World.GetStash<ButtonComponent>();

        if (gui != null)
            GameObject.Instantiate(gui);
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var entity in buttonFilter)
        {
            ref var buttonComponent = ref buttonComponents.Get(entity);
            if (!buttonComponent.isClicked) continue;

            switch (buttonComponent.name)
            {
                case "NextButton":
                    if (gameStateFilter.IsEmpty())
                        break;
                    ref var gameStateComponent = ref gameStateComponents.Get(gameStateFilter.First());
                    var currentLevelIndex = gameStateComponent.currentLevelIndex;
                    var nextLevelIndex = (currentLevelIndex + 1) % (gameStateComponent.levelCounts);
                    gameStateComponent.currentLevelIndex = nextLevelIndex;
                    Helpers.EmmitLevelIndexSaveRequest(World);
                    Helpers.EmmitClearGridSaveRequest(World);
                    Helpers.EmmitReloadSceneRequest(World);
                    break;
                case "RestartButton":
                    Helpers.EmmitClearGridSaveRequest(World);
                    Helpers.EmmitReloadSceneRequest(World);
                    break;
            }

            buttonComponent.isClicked = false;
        }
    }

    public void Dispose() { }
}