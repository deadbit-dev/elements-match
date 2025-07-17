using Unity.IL2CPP.CompilerServices;
using Scellecs.Morpeh;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class InitSystem : IInitializer
{
    public World World { get; set; }

    public void OnAwake()
    {
        var loadLevelEvent = this.World.GetRequest<LoadLevelRequest>();
        loadLevelEvent.Publish(new LoadLevelRequest
        {
            levelIndex = 0
        });
    }

    public void Dispose() { }
}