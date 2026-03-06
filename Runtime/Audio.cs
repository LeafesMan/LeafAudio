using UnityEngine;
using UnityEngine.Audio;
using LeafRand;
using System.Collections.Generic;

/// <summary>
/// Audio consists of a mixer group and an array of randomized audio clips
/// </summary>
[System.Serializable]
public class Audio
{
    [Tooltip("The mixer group the produced audio will be a part of")]
    [SerializeField] AudioMixerGroup mixerGroup;
    [SerializeField] bool useWeights;
    [SerializeReference] List<Weighted<AudioSpec>> audioSpecs;

    public AudioMixerGroup Group => mixerGroup;
    public AudioSpec RandomAudioSpec => useWeights ? Rand.ItemWeighted(audioSpecs) : Rand.Item(audioSpecs).Item;
    public int AudioSpecCount => audioSpecs.Count;
}
