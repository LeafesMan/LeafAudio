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
        public float maxDistance;
        public AnimationCurve attenuation;
        public AnimationCurve spatial;
        public AnimationCurve spread;
        public AnimationCurve reverb;

        public float startTime;
        public float duration;

        static readonly float DefaultMaxDistance = 50;
        static readonly AnimationCurve DefaultAttenuation = new AnimationCurve(new Keyframe(0, 1));
        static readonly AnimationCurve DefaultSpatial = new AnimationCurve(new Keyframe(0, 0));
        static readonly AnimationCurve DefaultSpread = new AnimationCurve(new Keyframe(0, 0));
        static readonly AnimationCurve DefaultReverb = new AnimationCurve(new Keyframe(0, 1));

        public PlaybackSettings(AudioClip clip, float volume, float pitch, float startTime, float duration, AudioMixerGroup mixerGroup, SpatialSettings spatialSettings)
        {
            this.mixerGroup = mixerGroup;
            this.clip = clip;
            this.volume = volume;
            this.pitch = pitch;

            this.startTime = startTime;
            this.duration = duration;


            // Copy SpatialSettings if provided
            // Otherwise apply defaults
            if (spatialSettings != null)
            {
                maxDistance = spatialSettings.maxDistance;
                attenuation = spatialSettings.attenuation;
                spatial = spatialSettings.spatial;
                spread = spatialSettings.spread;
                reverb = spatialSettings.reverb;
            }
            else
            {
                maxDistance = DefaultMaxDistance;
                attenuation = DefaultAttenuation;
                spatial = DefaultSpatial;
                spread = DefaultSpread;
                reverb = DefaultReverb;
            }
        }
        public PlaybackHandle Play(Vector3? position = null, Transform origin = null) => AudioManager.Global.Play(this, position, origin);
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

            source.time = startTime % clip.length;

            // Find largest distance
            source.maxDistance = maxDistance;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, attenuation);
            source.SetCustomCurve(AudioSourceCurveType.SpatialBlend, spatial);
            source.SetCustomCurve(AudioSourceCurveType.Spread, spread);
            source.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, reverb);
            if (source.pitch < 0) source.time = source.clip.length - 0.001f; // Flip the clip small subtraction stops from setting timestamp out-of-range causing an error
        }
        /// <summary>
        /// Returns the time it will take for these playback settings to play out.
        /// </summary>
        public float ClipDuration => Mathf.Abs(clip.length / pitch);
    }
}