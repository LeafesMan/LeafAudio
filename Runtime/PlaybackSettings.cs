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
        public float startTime;
        public float duration;
        public float volume;
        public float pitch;
        public Vector3? position;
        public Transform origin;
        public float maxDistance;
        public AnimationCurve attenuation;
        public AnimationCurve spread;
        public AnimationCurve reverb;
        public AnimationCurve spatial;


        // Defaults
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

            position = null;
            origin = null;

            // Set these to satisfy compiler
            // Actually set below in ApplySpatialSettings Helper
            maxDistance = default;
            attenuation = null;
            spatial = null;
            spread = null;
            reverb = null;

            ApplySpatialSettings(spatialSettings);
        }
        /// <summary>
        /// Applies the provided SpatialSettings to this PlaybackSettings
        /// </summary>
        void ApplySpatialSettings(SpatialSettings spatialSettings)
        {
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
        /// <summary>
        /// Applies this Playbacksettings to an AudioSource
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

        public PlaybackHandle Play() => AudioManager.Global.Play(this);

        public PlaybackSettings WithMixerGroup(AudioMixerGroup mixerGroup)
        {
            var newSettings = this;
            newSettings.mixerGroup = mixerGroup;
            return newSettings;
        }
        public PlaybackSettings WithClip(AudioClip clip)
        {
            var newSettings = this;
            newSettings.clip = clip;
            return newSettings;
        }
        public PlaybackSettings WithStartTime(float startTime)
        {
            var newSettings = this;
            newSettings.startTime = startTime;
            return newSettings;
        }
        public PlaybackSettings WithDuration(float duration)
        {
            var newSettings = this;
            newSettings.duration = duration;
            return newSettings;
        }
        public PlaybackSettings WithVolume(float volume)
        {
            var newSettings = this;
            newSettings.volume = volume;
            return newSettings;
        }
        public PlaybackSettings WithPitch(float pitch)
        {
            var newSettings = this;
            newSettings.pitch = pitch;
            return newSettings;
        }
        public PlaybackSettings WithPosition(Vector3? position)
        {
            var newSettings = this;
            newSettings.position = position;
            return newSettings;
        }
        public PlaybackSettings WithOrigin(Transform origin)
        {
            var newSettings = this;
            newSettings.origin = origin;
            return newSettings;
        }
        public PlaybackSettings WithSpatialSettings(SpatialSettings settings)
        {
            var newSettings = this;
            newSettings.ApplySpatialSettings(settings);
            return newSettings;
        }
    }
}