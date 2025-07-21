using UnityEngine;

[CreateAssetMenu(fileName = "Element", menuName = "Element/New Element")]
public class Element : ScriptableObject
{
    public string title;
    public Color color = new Color(1, 1, 1, 1);

    public GameObject prefab;
}