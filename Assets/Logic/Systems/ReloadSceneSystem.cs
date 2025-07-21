using Scellecs.Morpeh;
using UnityEngine.SceneManagement;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public class ReloadSceneSystem : ISystem
{
    public World World { get; set; }

    private Request<ReloadSceneRequest> reloadSceneRequest;

    public void OnAwake()
    {
        reloadSceneRequest = World.GetRequest<ReloadSceneRequest>();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var request in reloadSceneRequest.Consume())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void Dispose() { }
}