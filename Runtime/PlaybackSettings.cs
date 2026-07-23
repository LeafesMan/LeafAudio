using UnityEngine;
using UnityEngine.Audio;
namespace LeafAudio
{
    /// <summary>
    /// Required settings for playing a sound through an AudioManager
    /// </summary>
    public struct PlaybackSettings
    {
        public AudioMixerGroup mixerGroup;
        public AudioClip clip;
        public float volume;
        public float pitch;
        public AnimationCurve attenuation;
        public AnimationCurve spread;
        public AnimationCurve reverb;
        public PlaybackSettings(AudioClip clip, float volume, float pitch, AudioMixerGroup mixerGroup, AnimationCurve attenuation, AnimationCurve spread, AnimationCurve reverb)
        {
            this.mixerGroup = mixerGroup;
            this.clip = clip;
            this.volume = volume;
            this.pitch = pitch;
            this.attenuation = attenuation;
            this.spread = spread;
            this.reverb = reverb;
        }
        /// <summary>
        /// Applies Playbacksettings to an AudioSource
        /// </summary>
        public readonly void ApplyToSource(AudioSource source)
        {
            // Setup Source
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.outputAudioMixerGroup = mixerGroup;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, attenuation);
            source.SetCustomCurve(AudioSourceCurveType.Spread, spread);
            source.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, reverb);
            if (source.pitch < 0) source.time = source.clip.length - 0.001f; // Flip the clip small subtraction stops from setting timestamp out-of-range causing an error
        }
    }
}