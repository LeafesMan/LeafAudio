/*
 * Auth: Ian
 * 
 * Proj: Audio Library
 * 
 * Date: 2/29/24
 * 
 * Desc: Specs for a clip
 */
using UnityEngine;


public struct ClipSpecs
{
    public AudioClip clip;
    public float pitch;
    public float volume;

    public ClipSpecs(AudioClip clip, float pitch, float volume)
    {
        this.clip = clip;
        this.pitch = pitch;
        this.volume = volume;
    }
}
