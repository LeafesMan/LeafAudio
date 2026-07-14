using UnityEngine;
using LeafRand.Global;

namespace LeafAudio
{
    /// <summary>
    /// A single sound variant containing an audio clip and playback parameters.
    /// </summary>
    [System.Serializable]
    public class SoundVariant
    {
        [SerializeField] AudioClip clip;

        [SerializeField] internal float volume;
        [SerializeField] internal float volumeVariation;
        [SerializeField] internal float pitch;
        [SerializeField] internal float pitchVariation;

        public AudioClip GetClip() => clip;
        public float GetVolume() => volume + Rand.Float(-volumeVariation, volumeVariation);
        public float GetPitch() => pitch + Rand.Float(-pitchVariation, pitchVariation);
    }
}