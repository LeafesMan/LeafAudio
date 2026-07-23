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
        /// <summary>
        /// Each Looping audio source element has two audio sources for fading in a new looping clip
        /// </summary>
        Dictionary<uint, (AudioSource, AudioSource)> loopingPool = new();
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
        /// Plays a Clip with the given parameters
        /// </summary>
        public void Play(Sound sound, Vector3? position = null, Transform origin = null)
        {
            if (sound == null)
            {
#if UNITY_EDITOR
                if (Settings.instance.WarnOnPlayNullSound) Debug.LogWarning("Failed to play null sound! This is an editor-only warning and may be disabled: ProjectSetting/LeafAudio/WarnOnPlayNullSound");
#endif
                return;
            }

            // Exit early if clip is null
            PlaybackSettings playbackSettings = sound.GetPlaybackSettings();
            if (playbackSettings.clip == null) return;

            // Grab a source, set it up, play it, and sort the sources
            PooledAudioSource pooledSource = GetAudioSource();
            pooledSource.Setup(playbackSettings, position, origin);
            pooledSource.Play();
        }
        public void PlayLooping(Sound sound, float fadeDuration, uint slot)
        {   // If Audio Source pair hasnt been created for this slot create it
            if (!loopingPool.ContainsKey(slot))
            {
                loopingPool.Add(slot, new(gameObject.AddComponent<AudioSource>(), gameObject.AddComponent<AudioSource>()));
                loopingPool[slot].Item1.loop = true;
                loopingPool[slot].Item2.loop = true;
            }



            //Start fading out the faded in AudioSource
            StartCoroutine(FadeVolume(loopingPool[slot].Item2, loopingPool[slot].Item2.volume, 0, fadeDuration));

            //Fade in faded out Audio Source, replace it's clip with clip to fade in, and set volume to 0
            var playbackSettings = sound.GetPlaybackSettings();
            loopingPool[slot].Item2.clip = playbackSettings.clip;
            loopingPool[slot].Item2.pitch = playbackSettings.pitch;
            loopingPool[slot].Item2.outputAudioMixerGroup = sound.Group;
            StartCoroutine(FadeVolume(loopingPool[slot].Item2, 0, playbackSettings.volume, fadeDuration));

            //Swap faded in AudioSource with the faded out AudioSource in the audioSourcePairs tuple
            loopingPool[slot] = new(loopingPool[slot].Item2, loopingPool[slot].Item1);

            loopingPool[slot].Item1.Play();
            loopingPool[slot].Item2.Play();
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
            public void Setup(PlaybackSettings playbackSettings, Vector3? position, Transform origin)
            {
                playbackSettings.ApplyToSource(source);

#if UNITY_EDITOR
                source.name = source.clip.name; // Soley an editor convenience for easier debugging
#endif

                // Setup spatial settings
                if (position != null)
                {
                    source.spatialBlend = 1;

                    this.origin = origin;
                    offset = position.Value;
                    source.transform.position = origin == null ? offset : origin.position + offset;
                }
                else source.spatialBlend = 0;

                // Cache End Time stamp based on clip length
                endTime = Time.time + Audio.GetDuration(playbackSettings); ;
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