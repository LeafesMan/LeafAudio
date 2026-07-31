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
        public void Resume() { if (IsPaused) ResumeInternal(); }
        /// <summary>
        /// Pauses playback
        /// </summary>
        public void Pause() { if (!IsPaused) PauseInternal(); }
        /// <summary>
        /// Resumes if paused and Pauses if not paused 
        /// </summary>
        public void TogglePause()
        {
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
            if (IsDone) throw new System.InvalidOperationException("Failed to call method on this handle because it is stale!");
            manager.FreeSource(pooledSourceIndex);
        }
    }
}