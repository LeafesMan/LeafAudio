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
public class RandomAudio : IAudioDataProvider, IWeighted // Rename to RandomizedAudioData
{
    [SerializeField] AudioClip clip;

    [SerializeField] FieldType volumeType;
    [SerializeField] float volume = 0.5f;
    [SerializeField] Vector2 volumeRange = new Vector2(0.4f, 0.6f);
    [SerializeField] float[] volumeList = new float[] {0.4f, 0.5f, 0.6f};

    [SerializeField] FieldType pitchType;
    [SerializeField] float pitch = 1;
    [SerializeField] Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] float[] pitchList = new float[] { 0.9f, 1, 1.1f };
    public enum FieldType { Float, Range, List };


    [SerializeField] float weight;
    public float GetWeight() => weight;

    public AudioData GetAudioData()
    {
        float volume;
        if (volumeType == FieldType.Range)      volume = LeafRand.I.Range(volumeRange);
        else if (volumeType == FieldType.List)  volume = LeafRand.I.Element(volumeList);
        else volume = this.volume;

        float pitch;
        if      (pitchType == FieldType.Range) pitch = LeafRand.I.Range(pitchRange);
        else if  (pitchType == FieldType.List) pitch = LeafRand.I.Element(pitchList);
        else pitch = this.pitch;

        return new AudioData() { clip = clip, volume = volume, pitch = pitch };
    }
}
