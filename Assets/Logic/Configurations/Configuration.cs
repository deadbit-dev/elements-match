using Scellecs.Morpeh;
using UnityEngine;

[CreateAssetMenu(fileName = "Configuration", menuName = "Configuration/New Configuration")]
public class Configuration : ScriptableObject
{
    public int startLevelIndex;
    public Level[] levels;
}