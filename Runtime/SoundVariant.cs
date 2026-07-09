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

        [SerializeField] float volume = 0.5f;
        [SerializeField] float volumeVariation = 0;
        [SerializeField] float pitch = 1;
        [SerializeField] float pitchVariation = 0.2f;

        public AudioClip GetClip() => clip;
        public float GetVolume() => volume + Rand.Float(-volumeVariation, volumeVariation);
        public float GetPitch() => pitch + Rand.Float(-pitchVariation, pitchVariation);
    }
}