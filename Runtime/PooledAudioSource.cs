using System;
using UnityEngine;
namespace LeafAudio
{
    /// <summary>
    /// Struct for data stored about every source in the pool.
    /// </summary>
    [Serializable]
    internal class PooledAudioSource
    {
        internal static uint PlaybackIDCounter = 1; // Value of 0 is reserved for free sources

        [SerializeField] internal bool killOnDone; // Whether the source should be killed when it is done
        [SerializeField] internal AudioSource source; // Unchanging for the life of a pooled audio source

        [SerializeField] internal uint playbackID;
        [SerializeField] internal int usedIndex; // The index of this source in usedIndices
        [SerializeField] private float pitch; // The user-set pitch
        [SerializeField] internal Transform origin;
        [SerializeField] internal Vector3 position;
        [SerializeField] internal DurationMode durationMode;
        [SerializeField] internal float remainingDuration;
        [SerializeField] internal bool paused;
        [SerializeField] private bool ignoreTimescale;


        public float Pitch
        {
            get => pitch;
            set
            {
                pitch = value;
                UpdateSourcePitch();
            }
        }
        public bool IgnoreTimeScale
        {
            get => ignoreTimescale;
            set
            {
                ignoreTimescale = value;
                UpdateSourcePitch();
            }
        }


        /// <summary>
        /// Whether the pooled audio source has completed playback
        /// </summary>
        public bool IsDone => remainingDuration == 0;

        /// <summary>
        /// Updates the pitch on the source using the user-set pitch and the timescale if ignoreTimeScale is false
        /// </summary>
        public void UpdateSourcePitch() => source.pitch = pitch * (ignoreTimescale ? 1 : Time.timeScale);
    }
}