using UnityEngine;

namespace LeafAudio
{
    /// <summary>
    /// A single sound variant containing an audio clip and playback parameters.
    /// </summary>
    [System.Serializable]
    public class SoundVariant
    {
        [SerializeField] internal AudioClip clip;
        [SerializeField] internal float volume;
        [SerializeField] internal float volumeVariation;
        [SerializeField] internal float pitch;
        [SerializeField] internal float pitchVariation;

        public SoundVariant()
        {
            volume = Settings.instance.SoundDefaults.Volume;
            volumeVariation = Settings.instance.SoundDefaults.VolumeVariation;
            pitch = Settings.instance.SoundDefaults.Pitch;
            pitchVariation = Settings.instance.SoundDefaults.PitchVariation;
        }
    }
}