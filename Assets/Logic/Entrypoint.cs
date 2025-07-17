using Scellecs.Morpeh;
using UnityEngine;

public class Entrypoint : MonoBehaviour
{
    public Configuration Config;
    public Camera Сamera;

    private World world;

    void Start()
    {
        this.world = World.Default;

        var systems = this.world.CreateSystemsGroup();
        systems.AddInitializer(new InitSystem());

        systems.AddSystem(new LoadLevelSystem(Config, Сamera));
        systems.AddSystem(new BackgroundSystem());
        systems.AddSystem(new InitGridSystem());

        systems.AddSystem(new InputSystem());

        systems.AddSystem(new SpawnGridSystem());
        systems.AddSystem(new SpawnElementViewSystem());

        systems.AddSystem(new SwapSystem());

        systems.AddSystem(new GridAdaptationSystem());

        this.world.AddSystemsGroup(0, systems);
    }
}