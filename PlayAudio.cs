/*
 * Auth: Ian
 * 
 * Proj: Audio
 * 
 * Date: 10/1/24
 * 
 * Desc: Attach to an object to play Audio through UnityEvents
 *  - This is neccesary as Unity events only accept a single parameter in the inspector
 *  - This component stores those parameters 
 *  - Call the Play() method through a UnityEvent to play the Audio with all desired parameters 
 */

using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] new AudioAsset audio;

    [Header("Delay")]
    [SerializeField] float delay;

    [Header("Position")]
    [SerializeField, Tooltip("Leave null to play without spatial rolloff")] Rolloff rolloff;
    [SerializeField] Transform origin;
    [SerializeField] Vector3 offset;


    public void Play()
    {
        SpatialRolloff spatialRolloff = rolloff ? new SpatialRolloff(rolloff, offset, origin) : null;

        AudioManager.Play(audio, spatialRolloff, delay);
    }
}
