/*
 * Auth: Ian
 * 
 * Proj: Audio
 * 
 * Desc: Container for a set of RandomAudio
 * 
 * Date: 8/23/24
 */
using UnityEngine;
using UnityEngine.Audio;


[CreateAssetMenu(fileName ="NewAudioContainer", menuName ="Audio/Audio Container")]
public class RandomAudioSet : ScriptableObject, IAudioDataProvider
{
    [Tooltip("The mixer group the produced audio will be a part of")]
    [SerializeField] AudioMixerGroup mixerGroup;
    [SerializeField] RandomAudio[] randomAudios;

    public AudioData GetAudioData() 
    {
        AudioData toReturn = SRand.Element(randomAudios).GetAudioData();

        toReturn.mixerGroup = mixerGroup;

        return toReturn;
    } 
}
