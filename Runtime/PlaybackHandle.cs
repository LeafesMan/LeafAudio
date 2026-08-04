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

        /// <summary>
        /// A handle IsDone when playback has run it's course or Stop() was called.<br/>
        /// At this point all local functions will result in no-ops. 
        /// </summary>
        public bool IsDone => PooledSource.playbackID != playbackID;
        public bool IsPaused => float.IsNaN(PooledSource.endTime);
        ref PooledAudioSource PooledSource => ref manager.pooledSources[pooledSourceIndex];


        /// <summary>
        /// Resumes playback
        /// </summary>
        public void Resume() { if (!IsDone && IsPaused) ResumeInternal(); }
        /// <summary>
        /// Pauses playback
        /// </summary>
        public void Pause() { if (!IsDone && !IsPaused) PauseInternal(); }
        /// <summary>
        /// Resumes if paused and Pauses if not paused 
        /// </summary>
        public void TogglePause()
        {
            if (IsDone) return;
            if (IsPaused) ResumeInternal();
            else PauseInternal();
        }
        void PauseInternal()
        {   // Calling this method while paused will cause issues
            PooledSource.source.Pause();

            PooledSource.pausedTimeRemaining = PooledSource.endTime - Time.time;
            PooledSource.endTime = float.NaN; // Sentinal for Paused
        }
        void ResumeInternal()
        {   // Calling this method while not paused will cause issues
            PooledSource.source.UnPause();

            PooledSource.endTime = PooledSource.pausedTimeRemaining + Time.time;
        }
        /// <summary>
        /// Permanently ends the playback of this sound.
        /// </summary>
        public void Stop()
        {
            if (IsDone) return;
            manager.FreeSource(pooledSourceIndex);
        }

        #region Setters
        public Vector3 Position
        {
            set
            {
                if (IsDone) return;
                PooledSource.position = value;
            }
        }
        public Transform Origin
        {
            set
            {
                if (IsDone) return;
                PooledSource.origin = value;
            }
        }
        #endregion
    }
}