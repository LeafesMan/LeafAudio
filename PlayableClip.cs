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
    public virtual void Test()
    {
        // Create Temp Object and Components
        AudioSource source = new GameObject("Audio Test (DELETE ME)").AddComponent<AudioSource>();
        LeafAudioManager manager = source.gameObject.AddComponent<LeafAudioManager>();

        // Setup Source
        ClipSpecs specs = GetSpecs();
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
    public void Play(float delay = 0) => LeafAudioManager.Play(GetSpecs(), delay);

    public void Play(Vector3 pos, float delay = 0) => LeafAudioManager.PlayPositional(GetSpecs(), pos, delay);

    public void Play(Transform parent, Vector3 offset, float delay = 0) => LeafAudioManager.PlayParented(GetSpecs(), parent, offset, delay);

    public void PlayLooping(float fadeInTime, uint slot, float delay = 0) => LeafAudioManager.PlayLooping(GetSpecs(), fadeInTime, slot, delay);
}
