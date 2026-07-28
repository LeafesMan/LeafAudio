using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace LeafAudio
{
    /// <summary>
    /// Handles the positioning and pooling of all played Sounds
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Vars
        [SerializeField] internal List<PooledAudioSource> pooledSources = new(); // Contains all sources in the pool indices are never changed once assigned
        [SerializeField] List<int> usedIndices = new(); // Indices for used sources in PooledSources
        [SerializeField] List<int> freeIndices = new(); // Indices for free sources in PooledSources

# if UNITY_EDITOR
        internal static bool WarnOnPlayNullSound;
#endif
        #endregion
        void Update()
        {
            for (int i = usedIndices.Count - 1; i >= 0; i--)
            {
                PooledAudioSource sourceToUpdate = pooledSources[usedIndices[i]];
                sourceToUpdate.UpdatePosition();

                // Free up the source and toggle it off
                if (pooledSources[usedIndices[i]].IsDone) FreeSource(i);
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
        public PlaybackHandle Play(Sound sound, Vector3? position = null, Transform origin = null, float loops = 1)
        {
            if (sound == null)
            {
#if UNITY_EDITOR
                if (WarnOnPlayNullSound) Debug.LogWarning("Failed to play null sound! This is an editor-only warning and may be disabled: ProjectSetting/LeafAudio/WarnOnPlayNullSound");
#endif
                return new PlaybackHandle(this, 0, 0);
            }

            // Exit early if clip is null
            PlaybackSettings playbackSettings = sound.GetPlaybackSettings();
            if (playbackSettings.clip == null) return new PlaybackHandle(this, 0, 0);

            // Grab a source, set it up, play it, and sort the sources
            int freeSourceIndex = GetFreeSource();
            PooledAudioSource freeSource = pooledSources[freeSourceIndex];
            freeSource.Setup(playbackSettings, position, origin, loops);
            freeSource.Play();

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
        int GetFreeSource()
        {
            int index;

            if (freeIndices.Count > 0) // Pool has Free Source --> Return it
            {
                int lastFree = freeIndices.Count - 1;

                index = freeIndices[lastFree];

                // Update Pool Lists
                usedIndices.Add(freeIndices[lastFree]);
                freeIndices.RemoveAt(lastFree);
            }
            else // Pool has no Free Sources --> Return a new Source
            {
                PooledAudioSource newPooledSource = new PooledAudioSource(transform);

                // Update Pool Lists
                int newSourceIndex = pooledSources.Count;
                usedIndices.Add(newSourceIndex);
                pooledSources.Add(newPooledSource);


                index = newSourceIndex;
            }

            pooledSources[index].usedIndex = usedIndices.Count - 1;

            return index;
        }
        /// <summary>
        /// Frees up a used source by its index. Note: freeing up a source will stop its playback.
        /// </summary>
        internal void FreeSource(int index)
        {
            PooledAudioSource pooledSource = pooledSources[index];
            pooledSource.ToggleSourceGameObject(false); // Stop playback

            pooledSource.playbackID = 0; // Set to sentinal value for not playing 

            freeIndices.Add(index);

            // Swap Remove from active indices
            (usedIndices[^1], usedIndices[pooledSource.usedIndex]) = (usedIndices[pooledSource.usedIndex], usedIndices[^1]);
            usedIndices.RemoveAt(usedIndices.Count - 1);
        }
    }
}