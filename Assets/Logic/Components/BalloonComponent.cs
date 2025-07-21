using System;
using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Serializable]
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public struct BalloonComponent : IComponent
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public float flightDuration;
    public float amplitude;
    public float frequency;
}