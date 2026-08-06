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
        public float Duration
        {
            get
            {
                // Account for pitch on modes that arent specifying a certain Time duration
                float finalDuration = 0;
                if (durationMode == DurationMode.Time) return duration;
                else if (durationMode == DurationMode.Loops) finalDuration = ClipLength - ClipSpaceStartTime + ClipLength * duration;
                else if (durationMode == DurationMode.FullDurations) finalDuration = duration * ClipLength;
                return finalDuration / Mathf.Abs(Pitch); // Account for pitch on pitch dependent modes
            }
            set
            {
                durationMode = DurationMode.Time;
                duration = Mathf.Max(0, value);
            }
        }
        /// <summary> 
        /// Sets the playback duration as a number of traversals of the clip length. <br/> 
        /// Overrides any previous duration specification.
        /// </summary>
        public float Traversals
        {
            get
            {
                if (ClipLength == 0) return 0;
                return Duration * Mathf.Abs(Pitch) / ClipLength;
            }

            set
            {
                durationMode = DurationMode.FullDurations;
                duration = Mathf.Max(0, value);
            }
        }
        /// <summary> 
        /// Sets the playback duration as a number of loops after the initial traversal of the clip. <br/> 
        /// Overrides any previous duration specification.
        /// </summary>
        public float Loops
        {
            get
            {
                if (ClipLength == 0) return 0;
                return Mathf.Max(0, (Duration - (ClipLength - ClipSpaceStartTime)) * Mathf.Abs(Pitch) / ClipLength);
            }
            set
            {
                durationMode = DurationMode.Loops;
                duration = Mathf.Max(0, value);
            }
        }
        public float ClipLength => Clip == null ? 0 : Clip.length;
        float ClipSpaceStartTime => Mathf.Repeat(StartTime, ClipLength); // Starttime converted into the clips length space so: ClipLength: 1.5 StartTime 0 -> 0, 1.5 -> 1.5, 2 -> 0.5, -1, 0.5
        /// <summary>
        /// How the duration field should be interpreted
        /// </summary>
        internal enum DurationMode { Time, Loops, FullDurations }



        // Defaults
        static readonly float DefaultMaxDistance = 50;
        static readonly AnimationCurve DefaultAttenuation = new AnimationCurve(new Keyframe(0, 1));
        static readonly AnimationCurve DefaultSpatial = new AnimationCurve(new Keyframe(0, 0));
        static readonly AnimationCurve DefaultSpread = new AnimationCurve(new Keyframe(0, 0));
        static readonly AnimationCurve DefaultReverb = new AnimationCurve(new Keyframe(0, 1));



        public PlaybackSettings(AudioClip clip, float volume, float pitch, AudioMixerGroup mixerGroup, SpatialSettings spatialSettings)
        {
            MixerGroup = mixerGroup;
            Clip = clip;
            Volume = volume;
            Pitch = pitch;

            // Default timings
            StartTime = 0;
            durationMode = DurationMode.Loops;
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
        void ApplySpatialSettings(SpatialSettings spatialSettings)
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
            {
                MaxDistance = DefaultMaxDistance;
                Attenuation = DefaultAttenuation;
                Spatial = DefaultSpatial;
                Spread = DefaultSpread;
                Reverb = DefaultReverb;
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


            // Find largest distance
            source.maxDistance = MaxDistance;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, Attenuation);
            source.SetCustomCurve(AudioSourceCurveType.SpatialBlend, Spatial);
            source.SetCustomCurve(AudioSourceCurveType.Spread, Spread);
            source.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, Reverb);
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
        public PlaybackSettings WithDuration(float duration)
        {
            var newSettings = this;
            newSettings.Duration = duration;
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
        public PlaybackSettings WithSpatialSettings(SpatialSettings settings)
        {
            var newSettings = this;
            newSettings.ApplySpatialSettings(settings);
            return newSettings;
        }
    }
}