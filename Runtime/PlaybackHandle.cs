using UnityEngine;

namespace LeafAudio
{
    /// <summary>
    /// Provides access to Playback properties and functions.
    /// </summary>
    public struct PlaybackHandle
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


        ref PooledAudioSource PooledSource => ref manager.pooledSources[pooledSourceIndex];
        /// <summary>
        /// A handle IsDone when playback has run it is no longer playing audio!<br/>
        /// At this point all local functions will result in no-ops and return NaN values.
        /// </summary>
        public bool IsDone
        {
            get
            {
                if (manager == null) return true;
                else return PooledSource.playbackID != playbackID;
            }
        }
        public bool IsPaused => !IsDone && IsPausedInternal;
        bool IsPausedInternal => float.IsNaN(PooledSource.endTime);

        /// <summary>
        /// Resumes playback
        /// </summary>
        public void Resume()
        {
            if (IsDone) return;
            if (IsPausedInternal) ResumeInternal();
        }
        /// <summary>
        /// Pauses playback
        /// </summary>
        public void Pause()
        {
            if (IsDone) return;
            if (!IsPausedInternal) PauseInternal();
        }
        /// <summary>
        /// Resumes if paused and Pauses if not paused 
        /// </summary>
        public void TogglePause()
        {
            if (IsDone) return;
            if (IsPausedInternal) ResumeInternal();
            else PauseInternal();
        }
        // These internal allow no repeated paused checks
        void PauseInternal()
        {   // Calling this method while paused will cause issues
            ref var pooledSource = ref PooledSource;
            pooledSource.source.Pause();

            pooledSource.pausedRemainingDuration = pooledSource.endTime - UnityEngine.Time.time;
            pooledSource.endTime = float.NaN; // Sentinal for Paused
        }
        void ResumeInternal()
        {   // Calling this method while not paused will cause issues
            ref var pooledSource = ref PooledSource;
            pooledSource.source.UnPause();

            pooledSource.endTime = pooledSource.pausedRemainingDuration + UnityEngine.Time.time;
        }


        /// <summary>
        /// Permanently ends the playback of this sound.
        /// </summary>
        public void Stop()
        {
            if (IsDone) return;
            manager.FreeSource(pooledSourceIndex);
        }
        #region Internals
        #endregion
        #region Setters
        public Vector3 Position
        {
            get
            {
                if (IsDone) return new Vector3(float.NaN, float.NaN, float.NaN);
                else return PooledSource.position;
            }
            set
            {
                if (IsDone) return;
                PooledSource.position = value;
            }
        }
        public Transform Origin
        {
            get
            {
                if (IsDone) return null;
                else return PooledSource.origin;
            }
            set
            {
                if (IsDone) return;
                PooledSource.origin = value;
            }
        }
        public float Volume
        {
            get
            {
                if (IsDone) return float.NaN;
                else return PooledSource.source.volume;
            }
            set
            {
                if (IsDone) return;
                PooledSource.source.volume = value;
            }
        }
        public float Pitch
        {
            get
            {
                if (IsDone) return float.NaN;
                else return PooledSource.source.pitch;
            }
            set
            {
                if (IsDone) return;
                ref var pooledSource = ref PooledSource;
                pooledSource.endTime = RemainingDurationInternal * pooledSource.source.pitch / value; // Update endtime to account for change in pitch
                pooledSource.source.pitch = value;
            }
        }
        /// <summary>
        /// The current playback position within the clip. When time Adjusting time proportionally adjusts the Reamining Duration. <br/>
        /// * Note: Setting the time below 0 or above the clip length wraps around within the clip.
        /// </summary>
        public float Time
        {
            get => PooledSource.source.time;
            set
            {
                // Grab source and clipLength
                ref var pooledSource = ref PooledSource;
                float clipLength = pooledSource.source.clip.length;

                float time = value % clipLength; // Calculate new time

                if (time < 0) time += clipLength; // Account for negative time

                pooledSource.source.time = time; // Apply
            }
        }
        public float RemainingDuration
        {
            get
            {
                if (IsDone) return 0;
                return RemainingDurationInternal;
            }
            set
            {
                if (IsDone) return;
                if (IsPaused) PooledSource.pausedRemainingDuration = value;
                else PooledSource.endTime = UnityEngine.Time.time + value;
            }
        }
        float RemainingDurationInternal => IsPaused ? PooledSource.pausedRemainingDuration : PooledSource.endTime - UnityEngine.Time.time;
        #endregion
    }
}