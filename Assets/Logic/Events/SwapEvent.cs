using Scellecs.Morpeh;
using UnityEngine;

public struct SwapEvent : IEventData
{
    public Vector2Int from;
    public Vector2Int to;
}