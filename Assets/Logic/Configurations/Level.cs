using UnityEngine;
using System;
using DG.Tweening;

[CreateAssetMenu(fileName = "Level", menuName = "Level/New Level")]
public class Level : ScriptableObject
{
    public string title;

    [Header("Background")]
    public Sprite background;
    public Vector3 backgroundScale = new Vector3(0.45f, 0.45f, 1);

    [Space(10)]
    public BalloonsPreset balloonsPreset;

    [Header("Motions")]
    public Ease swapEasing = Ease.OutQuad;
    public float swapDuration = 0.3f;
    public Ease fallEasing = Ease.InCubic;
    public float fallDuration = 0.3f;

    [Header("Grid")]
    public float screenPadding = 0.2f;
    public Vector3 gridOffset = new Vector3(0, 1.7f, 1);
    public float maxGridSizeInUnits = 5f;
    public float cellSize = 0.5f;
    public float elementSize = 2f;

    [Range(2, 10)]
    public int width = 5;
    [Range(2, 10)]
    public int height = 5;

    [HideInInspector]
    public ElementDataBase elementDataBase;

    [HideInInspector]
    public int[] grid;
}