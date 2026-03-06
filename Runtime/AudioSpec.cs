using UnityEngine;
using LeafRand;

/// <summary>
/// Definition variables concerning a single audio clip.
/// </summary>
[System.Serializable]
public class AudioSpec
{
    [SerializeField] float weight = 1;
    [SerializeField] AudioClip clip;

    [SerializeField] FieldType volumeType;
    [SerializeField] float volume = 0.5f;
    [SerializeField] Vector2 volumeRange = new Vector2(0.4f, 0.6f);
    [SerializeField] Weighted<float>[] volumeList;

    [SerializeField] FieldType pitchType;
    [SerializeField] float pitch = 1;
    [SerializeField] Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] Weighted<float>[] pitchList;
    public enum FieldType { Float, Range, List };


    public AudioClip GetClip() => clip;
    public float GetWeight() => weight;
    public float GetVolume() => GetValue(volumeType, volume, volumeRange, volumeList);
    public float GetPitch() => GetValue(pitchType, pitch, pitchRange, pitchList);

    /// <summary>
    /// Returns a randomized value based on the field type
    /// </summary>
    public static float GetValue(FieldType type, float floatVal, Vector2 rangeVal, Weighted<float>[] arrayVal)
    {
        if (type == FieldType.Range) return Rand.Float(rangeVal);
        else if (type == FieldType.List) return Rand.ItemWeighted(arrayVal);
        return floatVal;
    }
}