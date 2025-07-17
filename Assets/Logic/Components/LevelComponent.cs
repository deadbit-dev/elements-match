using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[System.Serializable]
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public struct LevelComponent : IComponent
{
    public Camera camera;
    public Sprite background;
    public int width;
    public int height;
    public int[] grid;
    public float maxGridSize;
    public Vector3 gridOffset;
    public float screenPadding;
    public Element[] elements;
    public float elementSize;
}