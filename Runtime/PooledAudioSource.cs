using System;
using UnityEngine;
namespace LeafAudio
{
    /// <summary>
    /// Struct for data stored about every source in the pool.
    /// </summary>
    [Serializable]
    internal struct PooledAudioSource
    {
        internal static uint PlaybackIDCounter = 1; // Value of 0 is reserved for free sources

        [SerializeField] internal AudioSource source; // Unchanging for the life of a pooled audio source

        [SerializeField] internal uint playbackID;
        [SerializeField] internal int usedIndex; // The index of this source in usedIndices
        [SerializeField] internal Transform origin;
        [SerializeField] internal Vector3 position;
        [SerializeField] internal float endTime; // End time may be NaN representing paused or Infinity representing an infinitely looping sound
        [SerializeField] internal float pausedRemainingDuration; // Where remaining time is stored while this pooled source is paused

        /// <summary>
        /// Whether the pooled audio source has completed playback
        /// </summary>
        public bool IsDone => Time.time > endTime;
    }
}