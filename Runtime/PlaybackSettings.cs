using UnityEngine;
using UnityEngine.Audio;
namespace LeafAudio
{
    /// <summary>
    /// Required settings for playing a sound through an AudioManager
    /// </summary>
    public struct PlaybackSettings
    {
        // Main Vars
        public AudioMixerGroup MixerGroup;
        public AudioClip Clip;
        public float Volume;
        public float Pitch;
        public bool KillOnDone;
        public bool IgnoreTimeScale;

        // Positional Vars
        public Vector3? Position;
        public Transform Origin;
        public float MaxDistance;
        public AnimationCurve Attenuation;
        public AnimationCurve Spread;
        public AnimationCurve Reverb;
        public AnimationCurve Spatial;

        // Time Vars
        public float StartTime;
        internal DurationMode durationMode;
        internal float duration; // A value with units defined in DurationMode
        /// <summary> 
        /// Sets the playback duration in seconds.<br/> 
        /// Overrides any previous duration specification.
        /// </summary>
        public float RealTimeDuration
        {
            get
            {
                if (durationMode == DurationMode.RealTime) return duration;
                float unpitchedDuration = ClipTimeDuration;
                return unpitchedDuration == 0 ? 0 : unpitchedDuration / Mathf.Abs(Pitch);
            }
            set
            {
                durationMode = DurationMode.RealTime;
                duration = Mathf.Max(0, value);
            }
        }
        public float ClipTimeDuration
        {
            get
            {
                // Account for pitch on modes that arent specifying a certain Time duration
                if (durationMode == DurationMode.ClipLoops) return ClipLength == 0 ? 0 : ClipLength - ClipSpaceStartTime + ClipLength * duration;
                else if (durationMode == DurationMode.ClipTraversals) return ClipLength == 0 ? 0 : duration * ClipLength;
                else if (durationMode == DurationMode.RealTime) return Mathf.Abs(Pitch) == 0 ? 0 : duration * Mathf.Abs(Pitch);
                return duration;
            }
            set
            {
                durationMode = DurationMode.ClipTime;
                duration = Mathf.Max(0, value);
            }
        }
        /// <summary> 
        /// Sets the playback duration as a number of traversals of the clip length. <br/> 
        /// Overrides any previous duration specification. <br/>
        /// * Note same as Loops when StartTime = 0
        /// </summary>
        public float Traversals
        {
            get
            {
                if (ClipLength == 0) return 0;
                return ClipTimeDuration / ClipLength;
            }

            set
            {
                durationMode = DurationMode.ClipTraversals;
                duration = Mathf.Max(0, value);
            }
        }
        /// <summary> 
        /// Sets the playback duration as a number of loops after the initial traversal of the clip. <br/> 
        /// Overrides any previous duration specification. <br/>
        /// * Note same as Traversals when StartTime = 0
        /// </summary>
        public float Loops
        {
            get
            {
                if (ClipLength == 0) return 0;
                return Mathf.Max(0, (ClipTimeDuration - (ClipLength - ClipSpaceStartTime)) / ClipLength);
            }
            set
            {
                durationMode = DurationMode.ClipLoops;
                duration = Mathf.Max(0, value);
            }
        }
        public float ClipLength => Clip == null ? 0 : Clip.length;
        float ClipSpaceStartTime => Mathf.Repeat(StartTime, ClipLength); // Starttime converted into the clips length space so: ClipLength: 1.5 StartTime 0 -> 0, 1.5 -> 1.5, 2 -> 0.5, -1, 0.5
        /// <summary>
        /// How the duration field should be interpreted
        /// </summary>
        internal enum DurationMode { RealTime, ClipTime, ClipLoops, ClipTraversals }



        // Defaults
        internal static readonly float DefaultMaxDistance = 50;
        internal static readonly AnimationCurve DefaultAttenuationCurve = new AnimationCurve(new Keyframe(0, SpatialProfile.DefaultAttenuation));
        internal static readonly AnimationCurve DefaultSpreadCurve = new AnimationCurve(new Keyframe(0, SpatialProfile.DefaultSpread));
        internal static readonly AnimationCurve DefaultReverbCurve = new AnimationCurve(new Keyframe(0, SpatialProfile.DefaultReverb));
        internal static readonly AnimationCurve DefaultSpatialCurve = new AnimationCurve(new Keyframe(0, SpatialProfile.DefaultSpatial));



        public PlaybackSettings(AudioClip clip, float volume, float pitch, AudioMixerGroup mixerGroup, SpatialProfile spatialSettings)
        {
            MixerGroup = mixerGroup;
            Clip = clip;
            Volume = volume;
            Pitch = pitch;
            KillOnDone = true;
            IgnoreTimeScale = false;

            // Default timings
            StartTime = 0;
            durationMode = DurationMode.ClipLoops;
            duration = 0;

            Position = null;
            Origin = null;

            // Set these to satisfy compiler
            // Actually set below in ApplySpatialSettings Helper
            MaxDistance = default;
            Attenuation = null;
            Spatial = null;
            Spread = null;
            Reverb = null;

            ApplySpatialSettings(spatialSettings);
        }
        /// <summary>
        /// Applies the provided SpatialSettings to this PlaybackSettings
        /// </summary>
        void ApplySpatialSettings(SpatialProfile spatialSettings)
        {
            // Copy SpatialSettings if provided
            // Otherwise apply defaults
            if (spatialSettings != null)
            {
                MaxDistance = spatialSettings.maxDistance;
                Attenuation = spatialSettings.attenuation;
                Spatial = spatialSettings.spatial;
                Spread = spatialSettings.spread;
                Reverb = spatialSettings.reverb;
            }
            else
            {   // When no spatial settings are provided
                // the sound defaults to playing with no attenuation, spread, or reverb with a spatial curve set to always full spatial
                // Note: The spatial curve only applies when a position or origin is set
                MaxDistance = DefaultMaxDistance;
                Attenuation = DefaultAttenuationCurve;
                Spatial = DefaultSpatialCurve;
                Spread = DefaultSpreadCurve;
                Reverb = DefaultReverbCurve;
            }
        }
        /// <summary>
        /// Applies this Playbacksettings to an AudioSource
        /// </summary>
        internal readonly void ApplyToUnityAudioSource(AudioSource source)
        {
            // Setup Source
            source.clip = Clip;
            source.volume = Volume;
            source.pitch = Pitch;
            source.outputAudioMixerGroup = MixerGroup;

            source.time = StartTime % Clip.length;



            if (source.pitch < 0) source.time = source.clip.length - 0.001f; // Flip the clip small subtraction stops from setting timestamp out-of-range causing an error
        }
        public PlaybackHandle Play() => AudioManager.Global.Play(this);
        public PlaybackSettings WithMixerGroup(AudioMixerGroup mixerGroup)
        {
            var newSettings = this;
            newSettings.MixerGroup = mixerGroup;
            return newSettings;
        }
        public PlaybackSettings WithClip(AudioClip clip)
        {
            var newSettings = this;
            newSettings.Clip = clip;
            return newSettings;
        }
        public PlaybackSettings WithStartTime(float startTime)
        {
            var newSettings = this;
            newSettings.StartTime = startTime;
            return newSettings;
        }
        /// <summary> 
        /// Sets the playback duration in seconds.<br/> 
        /// Overrides any previous duration specification.
        /// </summary>
        public PlaybackSettings WithRealTimeDuration(float duration)
        {
            var newSettings = this;
            newSettings.RealTimeDuration = duration;
            return newSettings;
        }
        public PlaybackSettings WithClipTimeDuration(float duration)
        {
            var newSettings = this;
            newSettings.ClipTimeDuration = duration;
            return newSettings;
        }
        /// <summary> 
        /// Sets the playback duration as a number of traversals of the clip length. <br/> 
        /// Overrides any previous duration specification.
        /// </summary>
        public PlaybackSettings WithTraversals(float traversals)
        {
            var newSettings = this;
            newSettings.Traversals = traversals;
            return newSettings;
        }
        /// <summary> 
        /// Sets the playback duration as a number of loops after the initial traversal of the clip. <br/> 
        /// Overrides any previous duration specification.
        /// </summary>
        public PlaybackSettings WithLoops(float loops)
        {
            var newSettings = this;
            newSettings.Loops = loops;
            return newSettings;
        }
        public PlaybackSettings WithVolume(float volume)
        {
            var newSettings = this;
            newSettings.Volume = volume;
            return newSettings;
        }
        public PlaybackSettings WithPitch(float pitch)
        {
            var newSettings = this;
            newSettings.Pitch = pitch;
            return newSettings;
        }
        public PlaybackSettings WithPosition(Vector3? position)
        {
            var newSettings = this;
            newSettings.Position = position;
            return newSettings;
        }
        public PlaybackSettings WithOrigin(Transform origin)
        {
            var newSettings = this;
            newSettings.Origin = origin;
            return newSettings;
        }
        public PlaybackSettings WithSpatialSettings(SpatialProfile settings)
        {
            var newSettings = this;
            newSettings.ApplySpatialSettings(settings);
            return newSettings;
        }
        public PlaybackSettings WithKillOnDone(bool value)
        {
            var newSettings = this;
            newSettings.KillOnDone = value;
            return newSettings;
        }
        public PlaybackSettings WithIgnoreTimeScale(bool value)
        {
            var newSettings = this;
            newSettings.IgnoreTimeScale = value;
            return newSettings;
        }
    }
}