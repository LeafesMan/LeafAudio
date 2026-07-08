using UnityEngine;

namespace LeafAudio
{
    /// <summary>
    /// Desc: Attach to an object to play Audio through UnityEvents <br></br>
    /// - This is neccesary as Unity events only accept a single parameter in the inspector<br></br>
    /// - This component stores those parameters <br></br>
    /// - Call the Play() method through a UnityEvent to play the Audio with all desired parameters 
    /// </summary>
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
}