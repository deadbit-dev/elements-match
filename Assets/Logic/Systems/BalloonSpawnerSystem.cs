using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using DG.Tweening;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public class BalloonSpawnerSystem : ISystem
{
    public World World { get; set; }

    private Filter gameStateFilter;
    private Stash<GameStateComponent> gameStateComponents;

    private Filter levelFilter;
    private Stash<LevelComponent> levelComponents;

    private Filter balloonFilter;
    private Stash<BalloonComponent> balloonComponents;

    private Stash<ViewRefComponent> viewRefComponents;

    public void OnAwake()
    {
        levelFilter = World.Filter.With<LevelComponent>().Build();
        levelComponents = World.GetStash<LevelComponent>();

        balloonFilter = World.Filter.With<BalloonComponent>().Build();
        balloonComponents = World.GetStash<BalloonComponent>();

        viewRefComponents = World.GetStash<ViewRefComponent>();

        gameStateFilter = World.Filter.With<GameStateComponent>().Build();
        gameStateComponents = World.GetStash<GameStateComponent>();

        if (gameStateFilter.IsEmpty())
        {
            var gameStateEntity = World.CreateEntity();
            ref var gameStateComponent = ref gameStateComponents.Add(gameStateEntity);
            gameStateComponent.balloonsSpawnTimer = 0f;
        }
    }

    public void OnUpdate(float deltaTime)
    {
        if (gameStateFilter.IsEmpty() || levelFilter.IsEmpty())
            return;

        var gameStateEntity = gameStateFilter.First();
        ref var gameStateComponent = ref gameStateComponents.Get(gameStateEntity);

        var levelEntity = levelFilter.First();
        ref var levelComponent = ref levelComponents.Get(levelEntity);

        gameStateComponent.balloonsSpawnTimer += deltaTime;

        if (gameStateComponent.balloonsSpawnTimer >= levelComponent.balloonsPreset.spawnInterval)
        {
            int currentBalloonCount = balloonFilter.GetLengthSlow();

            if (currentBalloonCount < levelComponent.balloonsPreset.maxOnScreen)
                Spawn(levelComponent);

            gameStateComponent.balloonsSpawnTimer = 0f;
        }
    }

    private void Spawn(LevelComponent levelComponent)
    {
        BalloonsPreset balloonsPreset = levelComponent.balloonsPreset;

        if (balloonsPreset.balloons == null || balloonsPreset.balloons.Length == 0)
            return;

        int randomIndex = Random.Range(0, balloonsPreset.balloons.Length);
        Balloon balloon = balloonsPreset.balloons[randomIndex];
        GameObject balloonPrefab = balloon.prefab;

        if (balloonPrefab == null)
            return;

        GameObject balloonObject = GameObject.Instantiate(balloonPrefab);

        Entity balloonEntity = World.CreateEntity();
        ref var balloonComponent = ref balloonComponents.Add(balloonEntity);

        float spawnSide = Random.value;

        balloonComponent.startPosition = Helpers.GetRandomBalloonSpawnPosition(levelComponent, spawnSide);
        balloonComponent.startPosition.z = balloon.depth;

        balloonComponent.endPosition = Helpers.GetRandomBalloonTargetPosition(levelComponent, 1 - spawnSide);
        balloonComponent.endPosition.z = balloon.depth;

        balloonComponent.flightDuration = balloon.flightDuration;
        balloonComponent.amplitude = balloon.flightAmplitude;
        balloonComponent.frequency = balloon.flightFrequency;

        balloonObject.transform.position = balloonComponent.startPosition;

        ref var viewRefComponent = ref viewRefComponents.Add(balloonEntity);
        viewRefComponent.viewRef = balloonObject;

        Flight(balloonEntity, balloonComponent, balloonObject.transform);
    }

    private void Flight(Entity balloonEntity, BalloonComponent balloonComponent, Transform balloonTransform)
    {
        DOTween.To(
            () => 0f,
            (progress) =>
            {
                Vector3 newPosition = Helpers.CalculateBalloonPosition(
                    balloonComponent.startPosition,
                    balloonComponent.endPosition,
                    progress,
                    balloonComponent.amplitude,
                    balloonComponent.frequency
                );
                balloonTransform.position = newPosition;
            },
            1f,
            balloonComponent.flightDuration
        )
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            ref var viewRefComponent = ref viewRefComponents.Get(balloonEntity);
            GameObject.Destroy(viewRefComponent.viewRef);
            World.RemoveEntity(balloonEntity);
        });
    }

    public void Dispose() { }
}