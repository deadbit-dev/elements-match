using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Level", menuName = "Level/New Level")]
public class Level : ScriptableObject
{
    public string title;
    public Sprite background;

    public float maxGridSize = 0.7f;
    public float screenPadding = 0.1f;
    public float elementSize = 2f;
    public Vector3 gridOffset = new Vector3(0, -2, 1);

    [Range(2, 10)]
    public int width = 5;
    [Range(2, 10)]
    public int height = 5;

    [HideInInspector]
    public ElementDataBase elementDataBase;

    [HideInInspector]
    public int[] grid;
}