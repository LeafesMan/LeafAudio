using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace LeafAudio
{
    /// <summary>
    /// Script for playing and pooling audio. <br></br>
    /// - Attach this component to one object in your scene to listen for and handle audio events. <br></br>
    /// - Updated to automatically initialize a pool <br></br>
    /// - Instance is required for updating positions and running Coroutines 
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Vars
        [SerializeField, Tooltip("How many audio sources may be pooled.\nThis number has no bearing on looping audio sources.\nFeel free to edit this value")]
        int poolSize = Settings.instance.GlobalAudioManagerPoolSize;
        /// <summary>
        /// Audio Source Pool. Sorted in ascending order by End Time
        /// </summary>
        [SerializeField]
        List<PooledAudioSource> pool = new();
        /// <summary>
        /// Each Looping audio source element has two audio sources for fading in a new looping clip
        /// </summary>
        Dictionary<uint, (AudioSource, AudioSource)> loopingPool = new();
        #endregion
        void Update()
        {
            foreach (PooledAudioSource pooledSource in pool)
            {
                bool isDone = pooledSource.IsDone;
                if (!isDone) pooledSource.UpdatePosition();
#if UNITY_EDITOR
                pooledSource.ToggleSourceGameObject(!isDone); // This is purely visual for editor debugging can see what all is playing in inspector
#endif
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
            Sort(pooledSource);
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
        {   // Grabs a Pooled audio source to use for playing a sound
            // Uses free sources when possible
            // When there are no free sources creates a new one
            // OR   uses the oldest used source if the pool is full
            PooledAudioSource toReturn;

            // Pool has Available Source --> Return it
            if (pool.Count != 0 && pool[0].IsDone)
                toReturn = pool[0];
            // Pool Full --> Return Source that is closest to complete
            else if (pool.Count >= poolSize)
                toReturn = pool[0];
            // Pool Not Full --> Create new Source
            else
            {   // Create and  reparent an audio source
                AudioSource audioSource = new GameObject("PooledAudioSource").AddComponent<AudioSource>();
                audioSource.transform.SetParent(transform);

                toReturn = new PooledAudioSource(audioSource);
            }

            return toReturn;
        }
        /// <summary>
        /// Sorts the pool by ascending end time.<br></br>
        /// ***Assumes the PooledSource passed in is the only one that has changed
        /// </summary>
        void Sort(PooledAudioSource toInsert)
        {
            pool.Remove(toInsert);

            int i = 0;
            for (; i < pool.Count; i++)
                if (pool[i].EndTime > toInsert.EndTime)
                    break;

            pool.Insert(i, toInsert);
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
            {   // If no origin assume origin is (0, 0, 0)
                // Position is origin position + offset
                if (origin == null) return;
                source.transform.position = origin.position + offset;
            }
#if UNITY_EDITOR
            public void ToggleSourceGameObject(bool on) => source.gameObject.SetActive(on);
#endif
        }
        #endregion
    }
}