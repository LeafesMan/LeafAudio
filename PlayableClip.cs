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
using Unity.VisualScripting;
using UnityEngine;




public abstract class PlayableClip : ScriptableObject
{
    /// <summary> Gets the specs for this clip</summary>
    public abstract ClipSpecs GetSpecs();


    // Play Methods
    public void Play() => Play(0);
    public void Play(float delay) => AudioHandler.Play(GetSpecs(), delay);

    public void Play3D(Vector3 pos) => Play3D(pos, 0);
    public void Play3D(Vector3 pos, float delay) => AudioHandler.PlayPositional(GetSpecs(), pos, delay);

    public void Play3D(Transform parent, Vector3 offset) => Play3D(parent, offset, 0);
    public void Play3D(Transform parent, Vector3 offset, float delay) => AudioHandler.PlayParented(GetSpecs(), parent, offset, delay);

    public void PlayLooping(float fadeInTime, uint slot) => PlayLooping(fadeInTime, slot, 0);
    public void PlayLooping(float fadeInTime, uint slot, float delay) => AudioHandler.PlayLooping(GetSpecs(), fadeInTime, slot, delay);



    /// <summary>
    /// This test method is a little scuffed, though not sure of a better way to do it.
    /// </summary>
    public virtual void Test()
    {
        // Create Temp Object and Components
        AudioSource source = new GameObject("Audio Test (DELETE ME)").AddComponent<AudioSource>();
        AudioHandler handler = source.AddComponent<AudioHandler>();

        // Setup Source
        ClipSpecs specs = GetSpecs();
        source.clip = specs.clip;
        source.volume = specs.volume;
        source.pitch = specs.pitch;

        source.Play();

        // Destroy temporary Object after the clips completion
        handler.StartCoroutine(DestroyAfterClip());
        IEnumerator DestroyAfterClip()
        {
            yield return new WaitForSecondsRealtime(specs.clip.length);
            DestroyImmediate(source.gameObject);
        }
    }
}
