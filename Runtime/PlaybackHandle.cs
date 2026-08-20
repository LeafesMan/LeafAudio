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
        internal readonly PooledAudioSource pooledSource;
        internal PlaybackHandle(AudioManager manager, PooledAudioSource pooledAudioSource, uint playbackID)
        {
            this.manager = manager;
            this.playbackID = playbackID;
            this.pooledSource = pooledAudioSource;
        }



        /// <summary>
        /// Seeks by amount through the playback adjusting both the current Time and RemainingDuration accordingly. <br/>
        /// * Note: If amount would cause RemainingDuration to fall below 0, the seek will be limited to the current RemainingDuration.
        /// </summary>
        public readonly void Seek(float amount)
        {
            if (!IsAlive) return;

            // Ensure seek does not go past remaining duration
            amount = Mathf.Min(amount, pooledSource.remainingDuration);

            SetTimeInternal(pooledSource.source.time + amount);
            pooledSource.remainingDuration -= amount; // Update remaining duration accordingly
            UpdateAudioSourcePaused(pooledSource);
        }
        /// <summary>
        /// Permanently ends the playback of this sound.
        /// </summary>
        public readonly void Kill()
        {
            if (!IsAlive) return;
            manager.FreeSource(pooledSource);
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
                else return pooledSource.IsDone;
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
                else return pooledSource.playbackID == playbackID;
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
                return pooledSource.source.mute;
            }
            set
            {
                if (!IsAlive) return;
                pooledSource.source.mute = value;
            }
        }
        public readonly bool Paused
        {
            get
            {
                if (!IsAlive) return false;
                return pooledSource.paused;
            }
            set
            {
                if (!IsAlive) return;

                pooledSource.paused = value;

                UpdateAudioSourcePaused(pooledSource);
            }
        }
        public readonly Vector3 Position
        {
            get
            {
                if (!IsAlive) return new Vector3(float.NaN, float.NaN, float.NaN);
                else return pooledSource.position;
            }
            set
            {
                if (!IsAlive) return;
                pooledSource.position = value;
            }
        }
        public readonly Transform Origin
        {
            get
            {
                if (!IsAlive) return null;
                else return pooledSource.origin;
            }
            set
            {
                if (!IsAlive) return;
                pooledSource.origin = value;
            }
        }
        public readonly float Volume
        {
            get
            {
                if (!IsAlive) return float.NaN;
                else return pooledSource.source.volume;
            }
            set
            {
                if (!IsAlive) return;
                pooledSource.source.volume = value;
            }
        }
        public readonly float Pitch
        {
            get
            {
                if (!IsAlive) return float.NaN;
                else return pooledSource.source.pitch;
            }
            set
            {
                if (!IsAlive) return;
                pooledSource.source.pitch = value;
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
                return pooledSource.source.time;
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
            float clipLength = pooledSource.source.clip.length;

            // Keep time in clip space
            float time = newTime % clipLength + (newTime < 0 ? clipLength : 0);

            pooledSource.source.time = time;
        }
        /// <summary>
        /// Returns the current DurationMode. Note DurationMode may not be set through the property it may only be changed via a set of RealTimeRemainingDuration or ClipTimeRemainingDuration.
        /// </summary>
        public readonly DurationMode GetDurationMode => pooledSource.durationMode;
        /// <summary>
        /// Sets the remaining duration as a RealTime value, in seconds, and switches DurationMode to RealTime.
        /// </summary>
        public readonly float RealTimeRemainingDuration
        {
            get
            {
                if (!IsAlive) return 0;
                switch (pooledSource.durationMode)
                {
                    case DurationMode.RealTime: return pooledSource.remainingDuration;
                    case DurationMode.ClipTime: return pooledSource.remainingDuration == 0 ? 0 : pooledSource.remainingDuration / Mathf.Abs(PitchInternal);
                    default: throw new System.Exception("Unknown Mode!");
                }
            }
            set
            {
                if (!IsAlive) return;
                pooledSource.remainingDuration = Mathf.Max(0, value);
                pooledSource.durationMode = DurationMode.RealTime;

                // Changing remainingDuration may result in change in Pause on the source
                UpdateAudioSourcePaused(pooledSource);
            }
        }
        /// <summary>
        /// Sets the remaining duration as a ClipTime value, in seconds, and switches DurationMode to ClipTime.
        /// </summary>
        public readonly float ClipTimeRemainingDuration
        {
            get
            {
                if (!IsAlive) return 0;
                switch (pooledSource.durationMode)
                {
                    case DurationMode.RealTime: return Mathf.Abs(PitchInternal) == 0 ? 0 : pooledSource.remainingDuration * Mathf.Abs(PitchInternal);
                    case DurationMode.ClipTime: return pooledSource.remainingDuration;
                    default: throw new System.Exception("Unknown Mode!");
                }
            }
            set
            {
                if (!IsAlive) return;
                pooledSource.remainingDuration = Mathf.Max(0, value);
                pooledSource.durationMode = DurationMode.ClipTime;

                // Changing remainingDuration may result in change in Pause on the source
                UpdateAudioSourcePaused(pooledSource);
            }
        }
        float PitchInternal => pooledSource.source.pitch;
        #endregion
    }
}