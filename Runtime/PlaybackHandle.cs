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
        internal readonly int pooledSourceIndex; // Index of this source in the usedSourcesList
        public PlaybackHandle(AudioManager manager, int pooledSourceIndex, uint playbackID)
        {
            this.manager = manager;
            this.playbackID = playbackID;
            this.pooledSourceIndex = pooledSourceIndex;
        }

        /// <summary>
        /// Returns whether this handle is stale. A handle becomes stale when the playback it is referencing ends because the playback ran it's course or Stop was called on a handle pointing to that playback. 
        /// </summary>
        public bool IsStale => manager.pooledSources[pooledSourceIndex].playbackID != playbackID;
        void ThrowIfStale()
        {
            if (IsStale) throw new System.InvalidOperationException("Failed to call method on this handle because it is stale!");
        }
        /// <summary>
        /// Permanently ends the playback of this sound.
        /// </summary>
        public void Stop()
        {
            ThrowIfStale();
            manager.FreeSource(pooledSourceIndex);
        }
        /// <summary>
        /// Stops the playback of this sound returning its full PlaybackSettings allowing you to start the sound later from where it started via PlaybackSettings.Play();
        /// </summary>
        public PlaybackSettings StopAndGetPlaybackSettings()
        {
            return new PlaybackSettings();
        }
    }
}