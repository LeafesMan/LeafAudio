using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace LeafAudio
{
    /// <summary>
    /// Handles the positioning and pooling of all played Sounds
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Vars
        [SerializeField] internal PooledAudioSource[] pooledSources = new PooledAudioSource[64]; // Contains all sources in the pool indices are never changed once assigned (Use a manually resized array for easier struct mutation)
        [SerializeField] List<int> usedIndices = new(); // Indices for used sources in PooledSources
        [SerializeField] List<int> freeIndices = new(); // Indices for free sources in PooledSources
        public static AudioManager Global { get; private set; }


#if UNITY_EDITOR
        internal static bool WarnOnPlayNull;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void SetupGlobalAudioManager()
        {
            Global = new GameObject("AudioManager").AddComponent<AudioManager>();
            DontDestroyOnLoad(Global.gameObject);
        }

        #endregion
        void Update()
        {
            for (int i = usedIndices.Count - 1; i >= 0; i--)
            {
                PooledAudioSource pooledSource = pooledSources[usedIndices[i]];
                if (pooledSource.origin != null) pooledSource.source.transform.position = pooledSource.origin.position + pooledSource.offset;

                // Free up the source and toggle it off
                if (pooledSource.IsDone) FreeSource(i);
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
        public PlaybackHandle Play(Sound sound, Vector3? position = null, Transform origin = null, float loops = 1) => Play(sound.GetPlaybackSettings(), position, origin, loops);
        /// <summary>
        /// Plays a Clip with the given parameters.<br/>Note that when either position or origin are set the sound will be played spatially.
        /// </summary>
        /// <param name="playbackSettings"> The PlaybackSettings to play</param>
        /// <param name="position"> The world-space position to play the Sound at.<br/>If an origin is provided,
        /// this is treated as an offset from the origin.<br/>If this value is non-null the sound will play spatially. </param>
        /// <param name="origin">
        /// The sound will follow origin as if it were parented.<br/>
        /// When this value is set position will be treated as an offset from this.<br/>
        /// If this value is non-null the sound will play spatially.</param>
        /// <param name="loops">
        /// The number of times to play the Sound. A value of 1 plays the Sound once, values greater than
        /// 1 repeat the Sound, fractional values play will play part of the sound, and values less than 0 loop infinitely. </param> 
        public PlaybackHandle Play(PlaybackSettings playbackSettings, Vector3? position = null, Transform origin = null, float loops = 1)
        {   // Early exit on playing null sound or clip
            if (playbackSettings.clip == null)
            {
#if UNITY_EDITOR
                if (WarnOnPlayNull) Debug.LogWarning("Failed to play! Sound or Clip passed in was null! This is an editor-only warning and may be disabled: ProjectSetting/LeafAudio/WarnOnPlayNull");
#endif
                return new PlaybackHandle(this, 0, 0);
            }


            // Grab a source, set it up, play it, and sort the sources
            int freeSourceIndex = AcquireSource();
            ref PooledAudioSource freeSource = ref pooledSources[freeSourceIndex];

            freeSource.playbackID = PooledAudioSource.PlaybackIDCounter++;

            freeSource.source.gameObject.SetActive(true);
            playbackSettings.ApplyToSource(freeSource.source);

#if UNITY_EDITOR
            freeSource.source.name = freeSource.source.clip.name; // Soley an editor convenience for easier debugging
#endif

            // Setup spatial settings
            freeSource.origin = origin;
            freeSource.offset = position ?? Vector3.zero;
            freeSource.source.transform.position = origin == null ? freeSource.offset : (origin.position + freeSource.offset);

            // Cache End Time stamp based on clip length and Loops value
            // negative loops results in infinite looping
            if (loops >= 0) freeSource.endTime = Time.time + playbackSettings.Duration * loops;
            else freeSource.endTime = Mathf.Infinity;

            freeSource.source.Play();

            return new PlaybackHandle(this, freeSourceIndex, freeSource.playbackID);
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
        /// <summary>
        /// Returns the index of a free source and makes it a used source (creates a new source if none are available)
        /// </summary>
        int AcquireSource()
        {
            int index; // The index of the free source in PooledSources

            if (freeIndices.Count > 0) // Pool has Free Source --> Return it
            {
                int lastFree = freeIndices.Count - 1;

                index = freeIndices[lastFree];

                // Update Used/Free Lists
                usedIndices.Add(freeIndices[lastFree]);
                freeIndices.RemoveAt(lastFree);
            }
            else // Pool has no Free Sources --> Return a new Source
            {
                index = usedIndices.Count;

                // Construct a Pooled Source
                PooledAudioSource newPooledSource = new PooledAudioSource();
                newPooledSource.source = new GameObject("PooledAudioSource").AddComponent<AudioSource>();
                newPooledSource.source.rolloffMode = AudioRolloffMode.Custom;
                newPooledSource.source.loop = true;
                newPooledSource.source.transform.SetParent(transform);

                // Add the new Source to PooledSources
                if (usedIndices.Count == pooledSources.Length) Array.Resize(ref pooledSources, pooledSources.Length * 2);
                pooledSources[index] = newPooledSource;


                // Update Used/Free Lists
                usedIndices.Add(index);
            }

            // Update used index
            pooledSources[index].usedIndex = usedIndices.Count - 1;

            return index;
        }
        /// <summary>
        /// Frees up a used source by its index. Note: freeing up a source will stop its playback.
        /// </summary>
        internal void FreeSource(int index)
        {
            PooledAudioSource pooledSource = pooledSources[index];
            pooledSource.source.gameObject.SetActive(false); // Stop playback

            pooledSource.playbackID = 0; // Set to sentinal value for free sources 

            freeIndices.Add(index);

            // Swap Remove from active indices
            (usedIndices[^1], usedIndices[pooledSource.usedIndex]) = (usedIndices[pooledSource.usedIndex], usedIndices[^1]);
            usedIndices.RemoveAt(usedIndices.Count - 1);
        }
    }
}