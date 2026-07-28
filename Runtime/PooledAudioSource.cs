using System;
using UnityEngine;

namespace LeafAudio
{
    /// <summary>
    /// Struct for data stored about every source in the pool.
    /// </summary>
    [Serializable]
    class PooledAudioSource
    {
        // A value of 0 is sentinal for not playing
        static uint PlaybackIDCounter = 1;
        internal uint playbackID;
        internal int usedIndex; // The index of this source in usedIndices

        readonly AudioSource source;
        [SerializeField] Transform origin;
        [SerializeField] Vector3 offset;
        [SerializeField] float endTime;

        public PooledAudioSource(Transform parent)
        {
            source = new GameObject("PooledAudioSource").AddComponent<AudioSource>();
            source.rolloffMode = AudioRolloffMode.Custom;
            source.loop = true;
            source.transform.SetParent(parent);
        }
        /// <summary>
        /// Setups a pooled audio source with a new set of parameters
        /// </summary>
        public void Setup(PlaybackSettings playbackSettings, Vector3? position, Transform origin, float loops)
        {
            playbackID = PlaybackIDCounter++;

            ToggleSourceGameObject(true);
            playbackSettings.ApplyToSource(source);

#if UNITY_EDITOR
            source.name = source.clip.name; // Soley an editor convenience for easier debugging
#endif

            // Setup spatial settings
            this.origin = origin;
            offset = position ?? Vector3.zero;
            source.transform.position = origin == null ? offset : (origin.position + offset);

            // Cache End Time stamp based on clip length and Loops value
            // negative loops results in infinite looping
            if (loops >= 0) endTime = Time.time + Audio.GetDuration(playbackSettings) * loops;
            else endTime = Mathf.Infinity;
        }
        /// <summary>
        /// Plays the pooled audio source.
        /// </summary>
        public void Play() => source.Play();
        /// <summary>
        /// Whether the pooled audio source has finished its clip
        /// </summary>
        public bool IsDone => Time.time > endTime;
        public float EndTime => endTime;
        public void UpdatePosition()
        {   // Final position is origin + offset
            if (origin != null) source.transform.position = origin.position + offset;
        }
        public void ToggleSourceGameObject(bool on) => source.gameObject.SetActive(on);
    }
}