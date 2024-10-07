/*
 *  Name: Ian
 *
 *  Proj: Audio Library
 *
 *  Date: 7/26/23 
 *  
 *  Desc: Script for playing & pooling audio.
 *      Attach this component to one object in your scene to listen for and handle audio events.
 *      * Updated to automatically initialize a pool
 *      * Instance is required for updating positions and running Coroutines
 */

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    #region Vars
    [SerializeField, Tooltip("How many audio sources may be pooled.\nThis number has no bearing on looping audio sources.\nFeel free to edit this value")] 
    const int poolSize = 30;
    /// <summary>
    /// Audio Source Pool. Sorted in ascending order by End Time
    /// </summary>
    static List<PooledAudioSource> pool = new();
    /// <summary>
    /// Each Looping audio source element has two audio sources for fading in a new looping clip
    /// </summary>
    static Dictionary<uint, (AudioSource, AudioSource)> loopingPool = new();
    #endregion
    #region Pooled Audio Source Class
    /// <summary>
    /// Struct for data stored about every source in the pool.
    /// </summary>
    class PooledAudioSource
    {
        public readonly AudioSource source;
        Transform origin;
        Vector3 offset;
        public float endTime;


        public PooledAudioSource(AudioSource source) => this.source = source;

        /// <summary>
        /// Setups a pooled audio source with a new set of parameters
        /// </summary>
        public void Setup(AudioSpec audioSpec, AudioMixerGroup mixerGroup, SpatialRolloff spatialRolloff)
        {   // Audio Data
            source.outputAudioMixerGroup = mixerGroup;


            source.clip = audioSpec.GetClip();
            source.volume = audioSpec.GetVolume();
            source.pitch = audioSpec.GetPitch();

            // Setup spatial settings
            if(spatialRolloff != null)
            {
                origin = spatialRolloff.origin;
                offset = spatialRolloff.offset;

                source.spatialBlend = 1;
                source.rolloffMode = AudioRolloffMode.Custom;
                source.maxDistance = spatialRolloff.range.y;
                source.dopplerLevel = 0;
                float curveLength = source.maxDistance - source.minDistance;
                var rolloffCurve = new AnimationCurve(new Keyframe(spatialRolloff.range.x, 1, 0, -spatialRolloff.power / curveLength), new Keyframe(spatialRolloff.range.y, 0));
                source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, rolloffCurve);
            }
            else
                source.spatialBlend = 0;
            

            // Init position
            source.transform.position = origin == null ? offset : origin.position + offset;

            // Cache End Time stamp based on clip length
            endTime = audioSpec.GetClip().length + Time.time;
        }

        /// <summary>
        /// Plays the pooled audio source.
        /// </summary>
        public void Play() => source.Play();

        /// <summary>
        /// Whether the pooled audio source has finished its clip
        /// </summary>
        public bool IsActive => Time.time > endTime;
        public void UpdatePosition()
        {   // If no origin assume origin is (0, 0, 0)
            // Position is origin position + offset
            if(origin == null) return;
            source.transform.position = origin.position + offset;
        }
    }
    #endregion
   
    static AudioManager instance;


    [RuntimeInitializeOnLoadMethod]
    static void Setup()
    {
        GameObject audioManager = new GameObject("AudioManager");
        DontDestroyOnLoad(audioManager);
        audioManager.AddComponent<AudioManager>();

        // Create new Pool instance
        // Static variables are not refreshed by default
        // (Since I have disabled it for faster reloads + this will be default in future Unity versions)
        pool = new();
    }
    private void OnEnable()
    {   // Ensure Single Instance
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
            instance = this;;
    }
    private void Update()
    {
        foreach (PooledAudioSource pooledSource in pool)
            pooledSource.UpdatePosition();
    }
    /// <summary>
    /// Plays a Clip with the given parameters
    /// </summary>
    public static void Play(Audio audio, SpatialRolloff spatialSpecs = null)
    {   /* Null Audio Check Description
         * There are three cases where Audio may be null:
         * - Audio = null
         * - Audio.AudioSpec[].count = 0
         * - Audio.AudioSpec[x].clip = null
         * In all cases we quitely fail to play the audio
         * - I think this is ideal as we don't want to interrupt program flow with an error
         * - Theres an argument that the user should be informed of no audio playing
         *      but we don't want to blow up the console if it is intentional
         * - It is easy to determine why audio isnt playing dont need a console warning
         * (Perhaps turning on debug mode could print a warning)
         */
        // First and Second Null Audio Check
        if (audio == null || audio.AudioSpecCount == 0) return;

        // Get Spec to Play
        AudioSpec toPlay = audio.RandomAudioSpec;
        if (toPlay.GetClip() == null) return; // Third Null Audio Check

        // Get Audio source
        PooledAudioSource pooledSource = GetAudioSource();

        // Setup audio source
        pooledSource.Setup(toPlay, audio.Group, spatialSpecs);

        // Play Audio Source
        pooledSource.Play();

        // Add source to used audio sources
        Sort(pooledSource);
    }
    public static void Play(Audio audio, float delay) => Play(audio, null, delay);
    public static void Play(Audio audio, SpatialRolloff spatialSpecs, float delay) => ExecuteCallback(() => Play(audio, spatialSpecs), delay);
    public static void PlayLooping(Audio audio, float fadeDuration, uint slot, float delay = 0) => ExecuteCallback(() => PlayLooping(audio, fadeDuration, slot), delay);
    public static void PlayLooping(Audio audio, float fadeDuration, uint slot)
    {   // If Audio Source pair hasnt been created for this slot create it
        if (!loopingPool.ContainsKey(slot))
        {
            loopingPool.Add(slot, new(instance.gameObject.AddComponent<AudioSource>(), instance.gameObject.AddComponent<AudioSource>()));
            loopingPool[slot].Item1.loop = true;
            loopingPool[slot].Item2.loop = true;
        }



        //Start fading out the faded in AudioSource
        instance.StartCoroutine(FadeVolume(loopingPool[slot].Item2, loopingPool[slot].Item2.volume, 0, fadeDuration));

        //Fade in faded out Audio Source, replace it's clip with clip to fade in, and set volume to 0
        var audioSpec = audio.RandomAudioSpec;
        loopingPool[slot].Item2.clip = audioSpec.GetClip();
        loopingPool[slot].Item2.pitch = audioSpec.GetPitch();
        loopingPool[slot].Item2.outputAudioMixerGroup = audio.Group;
        instance.StartCoroutine(FadeVolume(loopingPool[slot].Item2, 0, audioSpec.GetVolume(), fadeDuration));

        //Swap faded in AudioSource with the faded out AudioSource in the audioSourcePairs tuple
        loopingPool[slot] = new(loopingPool[slot].Item2, loopingPool[slot].Item1);

        loopingPool[slot].Item1.Play();
        loopingPool[slot].Item2.Play();
    }
    /// <summary>
    /// Fades volume from current value to targetVolume over duration.
    /// </summary>
    static IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
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
    static PooledAudioSource GetAudioSource()
    {   // Grabs a Pooled audio source to use for playing a sound
        // Uses free sources when possible
        // When there are no free sources creates a new one
        // OR   uses the oldest used source if the pool is full
        PooledAudioSource toReturn;


        // Pool has Available Source --> Return it
        if (pool.Count != 0 && pool[0].IsActive)
            toReturn = pool[0];
        // Pool Full --> Return Source that is closest to complete
        else if (pool.Count >= poolSize)
            toReturn = pool[0];
        // Pool Not Full --> Create new Source
        else
        {   // Create and  reparent an audio source
            AudioSource audioSource = new GameObject("PooledAudioSource").AddComponent<AudioSource>();
            audioSource.transform.SetParent(instance.transform);

            //print($"Creating pooled source with source: {audioSource}");

            toReturn = new PooledAudioSource(audioSource);

            //print($"Created pooled source with source: {toReturn.source}");
        }

        return toReturn;
    }
    /// <summary>
    /// Sorts the pool by ascending end time.<br></br>
    /// ***Assumes the PooledSource passed in is the only one that has changed
    /// </summary>
    static void Sort(PooledAudioSource toInsert)
    {
        pool.Remove(toInsert);

        int i = 0;
        for (; i < pool.Count; i++)
            if (pool[i].endTime > toInsert.endTime)
                break;

        pool.Insert(i, toInsert);
    }
    /// <summary>
    /// Executes the callback after delay.
    /// Does not spin up a coroutine if delay is <= 0.
    /// </summary>
    static void ExecuteCallback(Action callback, float delay)
    {
        if (delay > 0) instance.StartCoroutine(ExecuteCallbackCoroutine(callback, delay));
        else callback.Invoke();
    }
    static IEnumerator ExecuteCallbackCoroutine(Action callback, float delay)
    {
        yield return new WaitForSeconds(delay);

        callback.Invoke();
    }
    /// <summary>
    /// Tests the clip by creating a temporary gameobject with an audio source on it then destroying it.
    /// </summary>
    public static void Test(AudioClip clip, float volume, float pitch)
    {
        if (clip == null) return;

        // Create Temp Object and Components
        AudioSource source = new GameObject("Audio Test (DELETE ME)").AddComponent<AudioSource>();
        AudioManager manager = source.gameObject.AddComponent<AudioManager>();

        // Setup Source
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;

        source.Play();

        // Destroy temporary Object after the clips completion
        manager.StartCoroutine(DestroyAfterClip());
        IEnumerator DestroyAfterClip()
        {
            yield return new WaitForSecondsRealtime(clip.length);
            DestroyImmediate(source.gameObject);
        }
    }
}