using UnityEngine;

/// <summary>
/// A Scriptable Object Wrapper for Audio
/// </summary>
[CreateAssetMenu(fileName ="NewAudioAsset", menuName ="FX/Audio Asset")]
public class AudioAsset : ScriptableObject
{
    [SerializeField] Audio audio;


    public static implicit operator Audio(AudioAsset audioAsset) => audioAsset == null ? null : audioAsset.audio;
}
