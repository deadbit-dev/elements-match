using Scellecs.Morpeh;
using UnityEngine;

public struct FallTweenCompleteRequest : IRequestData
{
    public Entity gridEntity;
    public Entity elementEntity;
    public Vector2Int fromPos;
    public Vector2Int toPos;
}