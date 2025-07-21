using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public struct LevelComponent : IComponent
{
    public Camera camera;
    public Sprite background;
    public Vector3 backgroundScale;

    public BalloonsPreset balloonsPreset;

    public float screenPadding;
    public Vector3 gridOffset;
    public float maxGridSizeInUnits;
    public float cellSize;
    public float elementSize;
    public Ease swapEasing;
    public float swapDuration;
    public Ease fallEasing;
    public float fallDuration;
    public int width;
    public int height;
    public int[] grid;
    public Element[] elements;
}