using UnityEngine;

namespace LeafAudio
{
    /// <summary>
    /// A single sound variant containing an audio clip and playback parameters.
    /// </summary>
    [System.Serializable]
    internal class SoundVariant
    {
        [SerializeField] internal AudioClip clip = null;
        [SerializeField] internal float volume = 0.5f;
        [SerializeField] internal float volumeVariation = 0;
        [SerializeField] internal float pitch = 1;
        [SerializeField] internal float pitchVariation = 0.05f;
    }
}