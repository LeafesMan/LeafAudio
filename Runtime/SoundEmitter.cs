using System;
using UnityEngine;

namespace LeafAudio
{
    /// <summary>
    /// Emits a single sound using the given variables. AudioSource alternative that is routed through AudioManager and uses Sound assets instead of fields. 
    /// </summary>
    public class SoundEmitter : MonoBehaviour
    {
        [SerializeField] public Sound sound;
        [SerializeField] public bool playOnStart = true;
        [SerializeField] public bool spatial = true;
        [SerializeField] public bool loop = true;

        PlaybackHandle handle;

        void Start()
        {
            if (playOnStart) PlayInternal();
        }

        /// <summary>
        /// Plays the sound from the start if its not playing or resumes it if paused
        /// </summary>
        public void Play()
        {
            if (handle.IsPaused) handle.Resume();
            else if (!handle.IsDone) PlayInternal();
        }
        public void PlayFrom(float timestamp) { throw new NotImplementedException(); }
        public void PlayFromStart() => PlayFrom(0);
        public void Pause() => handle.Pause();
        public void Stop() => handle.Stop();

        void PlayInternal(float timestamp = 0)
        {
            int numLoops = loop ? -1 : 1;
            if (spatial) handle = sound.Play(transform.localPosition, transform);
            else handle = sound.Play();
        }

        void Update()
        {   // Keep offset up-to-date
            // handle.offset = transform.localPosition;    
        }

        void OnDestroy() => handle.Stop();
    }
}
