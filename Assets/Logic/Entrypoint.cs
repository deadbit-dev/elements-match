using Scellecs.Morpeh;
using UnityEngine;

public class Entrypoint : MonoBehaviour
{
    [SerializeField] private Configuration config;
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private GameObject gui;
    [SerializeField] private GridDebugRenderer gridDebugRenderer;

    private World world;

    void Awake()
    {
        if (World.Default == null)
            WorldExtensions.InitializationDefaultWorld();

        this.world = World.Default;
        this.world.UpdateByUnity = true;

        var systems = this.world.CreateSystemsGroup();

        systems.AddInitializer(new InitSystem(config));

        systems.AddSystem(new LoadLevelSystem(config, gameplayCamera));

        systems.AddSystem(new InitGridSystem());

        systems.AddSystem(new SpawnBackgroundSystem());
        systems.AddSystem(new SpawnGridSystem(gridDebugRenderer));
        systems.AddSystem(new SpawnElementViewSystem());

        systems.AddSystem(new BalloonSpawnerSystem());

        systems.AddSystem(new InputSystem());

        systems.AddSystem(new SwapSystem());
        systems.AddSystem(new FallSystem());
        systems.AddSystem(new DestroyAreaSystem());

        systems.AddSystem(new WinSystem());

        systems.AddSystem(new UISystem(gui));

        systems.AddSystem(new DetectResizeScreenSystem());
        systems.AddSystem(new CleanUpOnResizeScreenSystem());

        systems.AddSystem(new GridAdaptationSystem());

        systems.AddSystem(new SaveSystem());

        systems.AddSystem(new ReloadSceneSystem());

        this.world.AddSystemsGroup(0, systems);
    }

    void OnDestroy()
    {
        DG.Tweening.DOTween.Clear();
        this.world.Dispose();
    }
}