using UnityEngine;

[CreateAssetMenu(fileName = "BalloonsPreset", menuName = "Balloons/New BalloonsPreset")]
public class BalloonsPreset : ScriptableObject
{
    public int maxOnScreen = 3;
    public float spawnInterval = 2f;
    public float spawnOffsetFromSide = 1f;
    public float spawnOffsetFromBottom = 5f;
    public float spawnHeightPercentage = 1.6f;
    public float targetOffsetFromSide = 1f;
    public float targetOffsetFromTop = 1f;
    public float targetHeightPercentage = 0.7f;
    public Balloon[] balloons;
}