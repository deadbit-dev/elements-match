using Scellecs.Morpeh;
using UnityEngine;

public struct GameStateComponent : IComponent
{
    public int currentLevelIndex;
    public int levelCounts;

    public int screenWidth;
    public int screenHeight;

    public float balloonsSpawnTimer;
}