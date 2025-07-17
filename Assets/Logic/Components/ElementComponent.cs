using System;
using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;


[Serializable]
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public struct ElementComponent : IComponent
{
    public int type;
}