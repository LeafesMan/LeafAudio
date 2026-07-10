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

        [SerializeField] float volume = DefaultVolume;
        [SerializeField] float volumeVariation = DefaultVolumeVariation;
        [SerializeField] float pitch = DefaultPitch;
        [SerializeField] float pitchVariation = DefaultPitchVariation;

        public const float DefaultVolume = 0.2f;
        public const float DefaultVolumeVariation = 0;
        public const float DefaultPitch = 1;
        public const float DefaultPitchVariation = 0.2f;

        public AudioClip GetClip() => clip;
        public float GetVolume() => volume + Rand.Float(-volumeVariation, volumeVariation);
        public float GetPitch() => pitch + Rand.Float(-pitchVariation, pitchVariation);
    }
}