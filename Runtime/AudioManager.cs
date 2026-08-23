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
        [SerializeField] internal List<PooledAudioSource> pooledSources = new(); // Contains all sources in the pool indices are never changed once assigned (Use a manually resized array for easier struct mutation)
        [SerializeField] List<PooledAudioSource> usedSources = new(); // Indices for used sources in PooledSources
        [SerializeField] List<PooledAudioSource> freeSources = new(); // Indices for free sources in PooledSources
        public static AudioManager Global { get; private set; }

        float previousTimeScale; // Cache the previous timescale to detect change

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
            // Update Pooled Sources
            // - Update positions
            // - Update remaining duration
            // - Free/Pause when Done
            for (int i = usedSources.Count - 1; i >= 0; i--)
            {
                PooledAudioSource pooledSource = usedSources[i];
                if (pooledSource.origin != null) pooledSource.source.transform.position = pooledSource.origin.position + pooledSource.position;

                if (!pooledSource.paused)
                {   // Reduces the remaining duration while not paused
                    float pitchFactor = pooledSource.durationMode == DurationMode.ClipTime ? pooledSource.source.pitch : 1; // Compute pitch effect
                    float deltaTime = pooledSource.IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime; // Allow clip to ignore timescale
                    pooledSource.remainingDuration -= deltaTime * pitchFactor; // Reduce remaining duration
                    pooledSource.remainingDuration = Mathf.Max(0, pooledSource.remainingDuration); // Ensure >= 0
                }
                if (pooledSource.IsDone)
                {
                    // Free up the source and toggle it off
                    if (pooledSource.killOnDone) FreeSource(usedSources[i]);
                    else pooledSource.source.Pause();
                }
            }

            HandleTimeScaleChange();
        }
        /// <summary>
        /// Detects change in timescale and updates all pitches accordingly
        /// </summary>
        void HandleTimeScaleChange()
        {
            // Timescale changed -> Update pitches
            if (Time.timeScale != previousTimeScale)
                foreach (PooledAudioSource pooledSource in usedSources)
                    pooledSource.UpdateSourcePitch();

            previousTimeScale = Time.timeScale;
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
        public PlaybackHandle Play(Sound sound) => Play(sound.GetPlaybackSettings());
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
        public PlaybackHandle Play(in PlaybackSettings playbackSettings)
        {   // Early exit on playing null sound or clip
            if (playbackSettings.Clip == null)
            {
#if UNITY_EDITOR
                if (WarnOnPlayNull) Debug.LogWarning("Failed to play! Sound or Clip passed in was null! This is an editor-only warning and may be disabled: ProjectSetting/LeafAudio/WarnOnPlayNull");
#endif
                return new PlaybackHandle(this, null, 0);
            }


            // Grab a source, set it up, play it, and sort the sources
            PooledAudioSource pooledSource = RentSource();

            pooledSource.playbackID = PooledAudioSource.PlaybackIDCounter++;

            pooledSource.killOnDone = playbackSettings.KillOnDone;
            pooledSource.IgnoreTimeScale = playbackSettings.IgnoreTimeScale;

            pooledSource.source.gameObject.SetActive(true);
            playbackSettings.ApplyToUnityAudioSource(pooledSource.source);

#if UNITY_EDITOR
            pooledSource.source.name = pooledSource.source.clip.name; // Soley an editor convenience for easier debugging
#endif

            // Setup spatial settings
            pooledSource.origin = playbackSettings.Origin;
            pooledSource.position = playbackSettings.Position ?? Vector3.zero;
            pooledSource.source.transform.position = playbackSettings.Origin == null ? pooledSource.position : (playbackSettings.Origin.position + pooledSource.position);


            if (playbackSettings.durationMode == PlaybackSettings.DurationMode.RealTime)
            {
                pooledSource.durationMode = DurationMode.RealTime;
                pooledSource.remainingDuration = playbackSettings.duration;
            }
            else
            {
                pooledSource.durationMode = DurationMode.ClipTime;
                pooledSource.remainingDuration = playbackSettings.ClipTimeDuration;
            }

            pooledSource.source.Play();

            return new PlaybackHandle(this, pooledSource, pooledSource.playbackID);
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
        PooledAudioSource RentSource()
        {
            PooledAudioSource toRent; // The index of the free source in PooledSources

            if (freeSources.Count > 0) // Pool has Free Source --> Return it
            {
                toRent = freeSources[freeSources.Count - 1];

                // Update Used/Free Lists
                usedSources.Add(toRent);
                freeSources.RemoveAt(freeSources.Count - 1);
            }
            else // Pool has no Free Sources --> Return a new Source
            {

                // Construct a Pooled Source
                PooledAudioSource newPooledSource = new PooledAudioSource
                {
                    source = new GameObject("PooledAudioSource").AddComponent<AudioSource>()
                };
                newPooledSource.source.rolloffMode = AudioRolloffMode.Custom;
                newPooledSource.source.loop = true;
                newPooledSource.source.transform.SetParent(transform);

                toRent = newPooledSource;

                pooledSources.Add(newPooledSource);

                // Update Used/Free Lists
                usedSources.Add(newPooledSource);

            }

            // Update used index
            toRent.usedIndex = usedSources.Count - 1;

            return toRent;
        }
        /// <summary>
        /// Frees up a used source by its index. Note: freeing up a source will stop its playback.
        /// </summary>
        internal void FreeSource(PooledAudioSource pooledSource)
        {
            pooledSource.source.gameObject.SetActive(false); // Stop playback

            pooledSource.playbackID = 0; // Set to sentinal value for free sources 


            // Swap Remove from active indices
            int toFreeSlot = pooledSource.usedIndex;
            int lastSlot = usedSources.Count - 1;
            usedSources[toFreeSlot] = usedSources[lastSlot]; // Swap
            usedSources.RemoveAt(lastSlot); // Remove

            pooledSource.usedIndex = toFreeSlot; // Update the cached usedIndex on the pooledSource whose usedIndex entry was moved

            // Add as a free source
            freeSources.Add(pooledSource);
        }
    }
}