using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace LeafAudio
{
    /// <summary>
    /// Handles the positioning and pooling of all played Sounds
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Vars
        [SerializeField]
        List<PooledAudioSource> activeSources = new();
        Stack<PooledAudioSource> freeSources = new();

# if UNITY_EDITOR
        internal static bool WarnOnPlayNullSound;
#endif
        #endregion
        void Update()
        {
            for (int i = activeSources.Count - 1; i >= 0; i--)
            {
                PooledAudioSource sourceToUpdate = activeSources[i];
                sourceToUpdate.UpdatePosition();

                // Free up the source and toggle it off
                if (activeSources[i].IsDone)
                {
                    sourceToUpdate.ToggleSourceGameObject(false);

                    (activeSources[^1], activeSources[i]) = (activeSources[i], activeSources[^1]);
                    activeSources.RemoveAt(activeSources.Count - 1);

                    freeSources.Push(sourceToUpdate);
                }
            }
        }
        /// <summary>
        /// Plays a Clip with the given parameters.<br/>Note that when either position or origin are set the sound will be played spatially.
        /// </summary>
        /// <param name="sound"> The Sound asset to play</param>
        /// <param name="position"> The world-space position to play the Sound at.<br/>If an origin is provided,
        /// this is treated as an offset from the origin.<br/>If this value is non-null the sound will play spatially. </param>
        /// <param name="origin">
        /// The sound will follow origin as if it were parented.<br/>
        /// When this value is set position will be treated as an offset from this.<br/>
        /// If this value is non-null the sound will play spatially.</param>
        /// <param name="loops">
        /// The number of times to play the Sound. A value of 1 plays the Sound once, values greater than
        /// 1 repeat the Sound, fractional values play will play part of the sound, and values less than 0 loop infinitely. </param> 
        public void Play(Sound sound, Vector3? position = null, Transform origin = null, float loops = 1)
        {
            if (sound == null)
            {
#if UNITY_EDITOR
                if (WarnOnPlayNullSound) Debug.LogWarning("Failed to play null sound! This is an editor-only warning and may be disabled: ProjectSetting/LeafAudio/WarnOnPlayNullSound");
#endif
                return;
            }

            // Exit early if clip is null
            PlaybackSettings playbackSettings = sound.GetPlaybackSettings();
            if (playbackSettings.clip == null) return;

            // Grab a source, set it up, play it, and sort the sources
            PooledAudioSource pooledSource = GetAudioSource();
            pooledSource.Setup(playbackSettings, position, origin, loops);
            pooledSource.Play();
        }
        /// <summary>
        /// Fades volume from current value to targetVolume over duration.
        /// </summary>
        IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
        {
            float startTime = Time.time;

            //Lerp from start Volume to target Volume over duration
            while (Time.time - startTime <= duration)
            {
                source.volume = Mathf.Lerp(from, to, (Time.time - startTime) / duration);
                yield return null;
            }

            source.volume = to;
        }
        PooledAudioSource GetAudioSource()
        {
            // Pool has Free Source --> Return it
            if (freeSources.Count > 0) return freeSources.Pop();
            // Pool has no Free Sources --> Create a new Source
            else
            {   // Create and  reparent an audio source
                AudioSource audioSource = new GameObject("PooledAudioSource").AddComponent<AudioSource>();
                audioSource.transform.SetParent(transform);

                return new PooledAudioSource(audioSource);
            }
        }
        #region Pooled Audio Source Class
        /// <summary>
        /// Struct for data stored about every source in the pool.
        /// </summary>
        [Serializable]
        class PooledAudioSource
        {
            readonly AudioSource source;
            [SerializeField] Transform origin;
            [SerializeField] Vector3 offset;
            [SerializeField] float endTime;

            public PooledAudioSource(AudioSource source) => this.source = source;
            /// <summary>
            /// Setups a pooled audio source with a new set of parameters
            /// </summary>
            public void Setup(PlaybackSettings playbackSettings, Vector3? position, Transform origin, float loops)
            {
                ToggleSourceGameObject(true);
                playbackSettings.ApplyToSource(source);

#if UNITY_EDITOR
                source.name = source.clip.name; // Soley an editor convenience for easier debugging
#endif

                // Setup spatial settings
                if (position != null || origin != null)
                {
                    source.spatialBlend = 1;

                    this.origin = origin;
                    offset = position ?? Vector3.zero;
                    source.transform.position = origin == null ? offset : origin.position + offset;
                }
                else source.spatialBlend = 0;

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
        #endregion
    }
}