using System;
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

        ref PooledAudioSource GetPooledSource()
        {
            if (manager == null) throw new InvalidOperationException("This handle is invalid! It was not returned from a Play call or it's AudioManager was Destroyed!");
            return ref manager.pooledSources[pooledSourceIndex];
        }

        /// <summary>
        /// A handle IsDone when playback has run it's course or Stop() was called.<br/>
        /// At this point all local functions will result in no-ops. 
        /// </summary>
        public bool IsDone => IsDoneInternal(GetPooledSource());
        public bool IsPaused => IsPausedInternal(GetPooledSource());
        // Making these take the PooledAudioSource allows us to cache PooledSource once and pass it around to all the methods that need it rather than grabbing it several times
        bool IsDoneInternal(in PooledAudioSource pooledSource) => pooledSource.playbackID != playbackID;
        bool IsPausedInternal(in PooledAudioSource pooledSource) => float.IsNaN(pooledSource.endTime);


        /// <summary>
        /// Resumes playback
        /// </summary>
        public void Resume()
        {
            ref PooledAudioSource pooledSource = ref GetPooledSource();
            if (!IsDoneInternal(pooledSource) && IsPausedInternal(pooledSource))
                ResumeInternal(ref pooledSource);
        }
        /// <summary>
        /// Pauses playback
        /// </summary>
        public void Pause()
        {
            ref PooledAudioSource pooledSource = ref GetPooledSource();
            if (!IsDoneInternal(pooledSource) && !IsPausedInternal(pooledSource))
                PauseInternal(ref pooledSource);
        }
        /// <summary>
        /// Resumes if paused and Pauses if not paused 
        /// </summary>
        public void TogglePause()
        {
            ref PooledAudioSource pooledSource = ref GetPooledSource();
            if (IsDoneInternal(pooledSource)) return;
            if (IsPausedInternal(pooledSource)) ResumeInternal(ref pooledSource);
            else PauseInternal(ref pooledSource);
        }
        // These internal allow no repeated paused checks
        void PauseInternal(ref PooledAudioSource pooledSource)
        {   // Calling this method while paused will cause issues
            pooledSource.source.Pause();

            pooledSource.pausedTimeRemaining = pooledSource.endTime - Time.time;
            pooledSource.endTime = float.NaN; // Sentinal for Paused
        }
        void ResumeInternal(ref PooledAudioSource pooledSource)
        {   // Calling this method while not paused will cause issues
            pooledSource.source.UnPause();

            pooledSource.endTime = pooledSource.pausedTimeRemaining + Time.time;
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
            set
            {
                if (IsDone) return;
                GetPooledSource().position = value;
            }
        }
        public Transform Origin
        {
            set
            {
                if (IsDone) return;
                GetPooledSource().origin = value;
            }
        }
        #endregion

    }
}