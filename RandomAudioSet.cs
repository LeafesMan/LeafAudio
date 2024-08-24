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


[CreateAssetMenu(fileName ="NewAudioContainer", menuName ="Audio/Audio Container")]
public class RandomAudioSet : ScriptableObject, IAudioDataProvider
{
    [SerializeField] RandomAudio[] randomAudios;

    public AudioData GetAudioData() => LeafRand.I.Element(randomAudios).GetAudioData(); 
}
