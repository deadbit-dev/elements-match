
using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Configuration", menuName = "Configuration/New Configuration")]
public class Configuration : ScriptableObject
{
    public Level[] levels;
}