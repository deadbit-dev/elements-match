using Scellecs.Morpeh;
using UnityEngine;

public struct SwapTweenCompleteRequest : IRequestData
{
    public Entity gridEntity;
    public Entity elementEntity;
    public Vector2Int pos;
}