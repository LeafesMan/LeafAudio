/*
 * Auth: Ian
 * 
 * Proj: Audio Audio Audio
 * 
 * Desc: Variables regarding a single clip within a LeafAudioClip
 * 
 * Date: 8/23/24
 */
using UnityEngine;


[System.Serializable]
public class AudioSpec :  IWeighted // Rename to RandomizedAudioData
{
    [SerializeField] float weight = 1;
    [SerializeField] AudioClip clip;

    [SerializeField] FieldType volumeType;
    [SerializeField] float volume = 0.5f;
    [SerializeField] Vector2 volumeRange = new Vector2(0.4f, 0.6f);
    [SerializeField] Weighted<float>[] volumeList = new Weighted<float>[] { new(0.9f, 0), new(1, 0), new(1.1f, 0) };

    [SerializeField] FieldType pitchType;
    [SerializeField] float pitch = 1;
    [SerializeField] Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] Weighted<float>[] pitchList = new Weighted<float>[] { new(0.9f, 0), new(1, 0), new(1.1f, 0) };
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
        if (type == FieldType.Range) return SRand.Range(rangeVal);
        else if (type == FieldType.List) return SRand.Weighted(arrayVal);
        return floatVal;
    }
}