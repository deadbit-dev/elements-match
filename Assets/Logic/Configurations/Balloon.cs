using UnityEngine;

[CreateAssetMenu(fileName = "Balloon", menuName = "Balloons/New Balloon")]
public class Balloon : ScriptableObject
{
    public float flightDuration = 16f;
    public float flightAmplitude = 2f;
    public float flightFrequency = 1.5f;
    public float depth = 2f;
    public GameObject prefab;
}