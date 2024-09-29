/*
 * Auth: Ian
 * 
 * Proj: Audio
 * 
 * Desc: A Scriptable Object Wrapper for Audio
 * 
 * Date: 8/23/24
 */
using UnityEngine;


[CreateAssetMenu(fileName ="NewAudioContainer", menuName ="Audio/Audio Container")]
public class AudioAsset : ScriptableObject
{
    [SerializeField] Audio audio;


    public static implicit operator Audio(AudioAsset audioAsset) => audioAsset.audio;
}
