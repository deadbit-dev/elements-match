using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ElementDataBase", menuName = "ElementDataBase/New ElementDataBase")]
public class ElementDataBase : ScriptableObject
{
    public Element[] elements;
}