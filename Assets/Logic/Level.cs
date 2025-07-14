using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Level", menuName = "Level/New Level")]
public class Level : ScriptableObject
{
    public string title;
    [Range(2, 10)]
    public int width = 5;
    [Range(2, 10)]
    public int height = 5;

    [HideInInspector]
    public ElementDataBase elementDataBase;

    [HideInInspector]
    public Element[] elements;
}