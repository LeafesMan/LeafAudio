using LeafAudio;
using PrimeTween;
using UnityEngine;

namespace LeafAudio
{
    /// <summary>
    /// Provides access to Playback properties and functions.
    /// </summary>
    public readonly struct PlaybackHandle
    {
        static internal uint NextPlaybackID = 1;
        internal readonly AudioManager manager;
        internal readonly uint playbackID;
        internal readonly int pooledSourceIndex;
        public PlaybackHandle(AudioManager manager, int pooledSourceIndex, uint playbackID)
        {
            this.manager = manager;
            this.playbackID = playbackID;
            this.pooledSourceIndex = pooledSourceIndex;
        }

        readonly PooledAudioSource PooledSource => manager.pooledSources[pooledSourceIndex];

        /// <summary>
        /// Seeks by amount through the playback adjusting both the current Time and RemainingDuration accordingly. <br/>
        /// * Note: If amount would cause RemainingDuration to fall below 0, the seek will be limited to the current RemainingDuration.
        /// </summary>
        public readonly void Seek(float amount)
        {
            if (!IsAlive) return;

            // Ensure seek does not go past remaining duration
            amount = Mathf.Min(amount, PooledSource.remainingDuration);

            SetTimeInternal(PooledSource.source.time + amount);
            PooledSource.remainingDuration -= amount; // Update remaining duration accordingly
            UpdateAudioSourcePaused(PooledSource);
        }
        /// <summary>
        /// Permanently ends the playback of this sound.
        /// </summary>
        public readonly void Kill()
        {
            if (!IsAlive) return;
            manager.FreeSource(pooledSourceIndex);
        }

        /// <summary>
        /// A handle IsDone when remaining duration is up. <br/>
        /// At this point audio playback has stopped and the handle will be dead if KillOnDone is true.
        /// </summary>
        public readonly bool IsDone
        {
            get
            {
                if (!IsAlive) return true;
                else return PooledSource.IsDone;
            }
        }
        /// <summary>
        /// When a handle isAlive all functions and properties are accessible.<br/>
        /// When a handle is not alive functions and properties will be no-ops or return NaN values.
        /// </summary>
        public readonly bool IsAlive
        {
            get
            {
                if (manager == null) return false;
                else return PooledSource.playbackID == playbackID;
            }
        }
        readonly void UpdateAudioSourcePaused(in PooledAudioSource pooledSource)
        {
            if (pooledSource.IsDone || pooledSource.paused) pooledSource.source.Pause();
            else pooledSource.source.UnPause();
        }
        #region Setters
        public readonly bool Muted
        {
            get
            {
                if (!IsAlive) return false;
                return PooledSource.source.mute;
            }
            set
            {
                if (!IsAlive) return;
                PooledSource.source.mute = value;
            }
        }
        public readonly bool Paused
        {
            get
            {
                if (!IsAlive) return false;
                return PooledSource.paused;
            }
            set
            {
                if (!IsAlive) return;

                PooledSource.paused = value;

                UpdateAudioSourcePaused(PooledSource);
            }
        }
        public readonly Vector3 Position
        {
            get
            {
                if (!IsAlive) return new Vector3(float.NaN, float.NaN, float.NaN);
                else return PooledSource.position;
            }
            set
            {
                if (!IsAlive) return;
                PooledSource.position = value;
            }
        }
        public readonly Transform Origin
        {
            get
            {
                if (!IsAlive) return null;
                else return PooledSource.origin;
            }
            set
            {
                if (!IsAlive) return;
                PooledSource.origin = value;
            }
        }
        public readonly float Volume
        {
            get
            {
                if (!IsAlive) return float.NaN;
                else return PooledSource.source.volume;
            }
            set
            {
                if (!IsAlive) return;
                PooledSource.source.volume = value;
            }
        }
        public readonly float Pitch
        {
            get
            {
                if (!IsAlive) return float.NaN;
                else return PooledSource.source.pitch;
            }
            set
            {
                if (!IsAlive) return;
                PooledSource.remainingDuration *= PooledSource.source.pitch / value; // Update endtime to account for change in pitch
                PooledSource.source.pitch = value;
            }
        }
        /// <summary>
        /// The current playback position within the clip. When time Adjusting time proportionally adjusts the Reamining Duration. <br/>
        /// * Note: Setting the time below 0 or above the clip length wraps around within the clip.
        /// </summary>
        public readonly float Time
        {
            get
            {
                if (!IsAlive) return float.NaN;
                return PooledSource.source.time;
            }
            set
            {
                if (!IsAlive) return;
                SetTimeInternal(value);
            }
        }
        readonly void SetTimeInternal(float newTime)
        {
            // Grab source and clipLength
            float clipLength = PooledSource.source.clip.length;

            // Keep time in clip space
            float time = newTime % clipLength + (newTime < 0 ? clipLength : 0);

            PooledSource.source.time = time;
        }
        public readonly float RemainingDuration
        {
            get
            {
                if (!IsAlive) return 0;
                return PooledSource.remainingDuration;
            }
            set
            {
                if (!IsAlive) return;
                PooledSource.remainingDuration = Mathf.Max(0, value);

                // Changing remainingDuration may result in change in Pause on the source
                UpdateAudioSourcePaused(PooledSource);
            }
        }
        #endregion
        #region Tween Support
        public readonly Tween TweenVolume(TweenSettings<float> tweenSettings) => Tween.Custom(target: new PlaybackHandleBoxed(this), tweenSettings, (target, newValue) => { target.handle.Volume = newValue; });
        public readonly Tween TweenVolume(float endValue, float duration, Easing ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false) => TweenVolume(new TweenSettings<float>(Volume, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public readonly Tween TweenVolume(float startValue, float endValue, float duration, Easing ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false) => TweenVolume(new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public readonly Tween TweenPitch(TweenSettings<float> tweenSettings) => Tween.Custom(target: new PlaybackHandleBoxed(this), tweenSettings, (target, newValue) => { target.handle.Pitch = newValue; });
        public readonly Tween TweenPitch(float endValue, float duration, Easing ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false) => TweenPitch(new TweenSettings<float>(Pitch, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        public readonly Tween TweenPitch(float startValue, float endValue, float duration, Easing ease = default, int cycles = 1, CycleMode cycleMode = CycleMode.Restart, float startDelay = 0f, float endDelay = 0f, bool useUnscaledTime = false) => TweenPitch(new TweenSettings<float>(startValue, endValue, duration, ease, cycles, cycleMode, startDelay, endDelay, useUnscaledTime));
        #endregion
    }
}
internal class PlaybackHandleBoxed
{
    public PlaybackHandle handle;
    public PlaybackHandleBoxed(PlaybackHandle handle) => this.handle = handle;
}