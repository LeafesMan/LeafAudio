using UnityEngine;

namespace LeafAudio
{
    /// <summary>
    /// A single sound variant containing an audio clip and playback parameters.
    /// </summary>
    [System.Serializable]
    internal class SoundVariant
    {
        [SerializeField] internal AudioClip clip;
        [SerializeField] internal float volume;
        [SerializeField] internal float volumeVariation;
        [SerializeField] internal float pitch;
        [SerializeField] internal float pitchVariation;

        //  Don't use a default constructor as Unity will call it before scriptable singletons setup resulting in a null ref error
        /// <summary>
        /// Sets values to defaults.
        /// </summary>
        internal void Reset()
        {
            volume = Settings.instance.SoundDefaults.Volume;
            volumeVariation = Settings.instance.SoundDefaults.VolumeVariation;
            pitch = Settings.instance.SoundDefaults.Pitch;
            pitchVariation = Settings.instance.SoundDefaults.PitchVariation;
        }
    }
}