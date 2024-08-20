/*
 *  Name: Ian
 *
 *  Proj: Audio Library
 *
 *  Date: 7/26/23 
 *  
 *  Desc: Interface for playable clip classes forces a Play and Positional Play method.
 */

using System.Collections;
using UnityEngine;




public abstract class PlayableClip : ScriptableObject
{

    /// <summary> Gets the specs for this clip</summary>
    public abstract ClipSpecs GetSpecs();

    /// <summary>
    /// This test method is a little scuffed still not sure of a good way to do it.
    /// </summary>
    public virtual void Test(ClipSpecs specs)
    {
        // Create Temp Object and Components
        AudioSource source = new GameObject("Audio Test (DELETE ME)").AddComponent<AudioSource>();
        AudioManager manager = source.gameObject.AddComponent<AudioManager>();

        // Setup Source
        source.clip = specs.clip;
        source.volume = specs.volume;
        source.pitch = specs.pitch;

        source.Play();

        // Destroy temporary Object after the clips completion
        manager.StartCoroutine(DestroyAfterClip());
        IEnumerator DestroyAfterClip()
        {
            yield return new WaitForSecondsRealtime(specs.clip.length);
            DestroyImmediate(source.gameObject);
        }
    }



    // Play Methods
    public void Play(float delay = 0)
    {
        if (AudioManagerStarted())
            AudioManager.Play(GetSpecs(), delay);
    }

    public void Play(Vector3 pos, float delay = 0)
    {
        if (AudioManagerStarted())
            AudioManager.PlayPositional(GetSpecs(), pos, delay);
    }

    public void Play(Transform parent, Vector3 offset, float delay = 0)
    {
        if (AudioManagerStarted())
            AudioManager.PlayParented(GetSpecs(), parent, offset, delay);
    }

    public void PlayLooping(float fadeInTime, uint slot, float delay = 0)
    {
        if(AudioManagerStarted())
            AudioManager.PlayLooping(GetSpecs(), fadeInTime, slot, delay);
    }


    /// <summary>
    /// Returns whether an Audio Manager is loaded and prints a warning to console if one is not.
    /// </summary>
    /// <returns></returns>
    bool AudioManagerStarted()
    {
        if (AudioManager.Play == null) 
        {
            Debug.Log("Audio failed to play. No AudioManager loaded in the scene!");
            return false; 
        }
        return true;
    }
}
