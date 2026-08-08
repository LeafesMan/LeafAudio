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
        /// Permanently ends the playback of this sound.
        /// </summary>
        public void Kill()
        {
            if (!IsAlive) return;
            manager.FreeSource(pooledSourceIndex);
        }

        /// <summary>
        /// A handle IsDone when remaining duration is up. <br/>
        /// At this point audio playback has stopped and the handle will be dead if KillOnDone is true.
        /// </summary>
        public bool IsDone
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
        public bool IsAlive
        {
            get
            {
                if (manager == null) return false;
                else return PooledSource.playbackID == playbackID;
            }
        }
        void UpdateAudioSourcePaused(in PooledAudioSource pooledSource)
        {
            if (pooledSource.IsDone || pooledSource.paused) pooledSource.source.Pause();
            else pooledSource.source.UnPause();
        }
        #region Setters
        public bool Paused
        {
            get
            {
                if (!IsAlive) return false;
                return PooledSource.paused;
            }
            set
            {   // Early Outs
                // 1) If !Alive can't pause
                // 2) If Done can't pause
                if (!IsAlive) return;
                ref var pooledSource = ref PooledSource;
                if (pooledSource.IsDone) return;

                // Update pause value and Un/Pause
                pooledSource.paused = value;
                UpdateAudioSourcePaused(pooledSource);
            }
        }
        public Vector3 Position
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
        public Transform Origin
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
        public float Volume
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
        public float Pitch
        {
            get
            {
                if (!IsAlive) return float.NaN;
                else return PooledSource.source.pitch;
            }
            set
            {
                if (!IsAlive) return;
                ref var pooledSource = ref PooledSource;
                pooledSource.remainingDuration *= pooledSource.source.pitch / value; // Update endtime to account for change in pitch
                pooledSource.source.pitch = value;
            }
        }
        /// <summary>
        /// The current playback position within the clip. When time Adjusting time proportionally adjusts the Reamining Duration. <br/>
        /// * Note: Setting the time below 0 or above the clip length wraps around within the clip.
        /// </summary>
        public float Time
        {
            get
            {
                if (!IsAlive) return float.NaN;
                return PooledSource.source.time;
            }
            set
            {
                if (!IsAlive) return;
                // Grab source and clipLength
                ref var pooledSource = ref PooledSource;
                float clipLength = pooledSource.source.clip.length;


                pooledSource.remainingDuration += pooledSource.source.time - value; // Update remaining duration accordingly

                // Keep time in clip space
                float time = value % clipLength + (pooledSource.source.time < 0 ? clipLength : 0);

                pooledSource.source.time = time; // Apply

                UpdateAudioSourcePaused(pooledSource);
            }
        }
        public float RemainingDuration
        {
            get
            {
                if (!IsAlive) return 0;
                return PooledSource.remainingDuration;
            }
            set
            {
                if (!IsAlive) return;
                ref var pooledSource = ref PooledSource;
                pooledSource.remainingDuration = Mathf.Max(0, value);

                // Changing remainingDuration may result in change in Pause on the source
                UpdateAudioSourcePaused(pooledSource);
            }
        }
        #endregion
    }
}