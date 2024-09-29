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

[System.Serializable]
public class Audio
{
    [Tooltip("The mixer group the produced audio will be a part of")]
    [SerializeField] AudioMixerGroup mixerGroup;
    [SerializeField] AudioSpec[] audioSpecs;

    public AudioMixerGroup Group => mixerGroup;
    public AudioSpec RandomAudioSpec => SRand.Weighted(audioSpecs);
}
