using UnityEngine;


[CreateAssetMenu(menuName = "Rolloff")]
public class Rolloff : ScriptableObject
{
    [SerializeField] Vector2 range = new Vector2(5, 30);
    [SerializeField, Range(0,4)] int power = 1;

    public Vector2 Range => range;
    public int Power => power;
}
