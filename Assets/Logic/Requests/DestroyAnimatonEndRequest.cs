using Scellecs.Morpeh;
using UnityEngine;

public struct DestroyAnimationEndRequest : IRequestData
{
    public Entity elementEntity;
    public Vector2Int position;
}