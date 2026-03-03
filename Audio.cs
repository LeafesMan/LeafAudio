/*
 * Auth: Ian
 * 
 * Proj: Audio
 * 
 * Desc: Audio consists of a mixer group and an array of randomized audio clips
 * 
 * Date: 8/23/24
 */

using UnityEngine;
using UnityEngine.Audio;
using LeafRand;
using System.Collections.Generic;

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
