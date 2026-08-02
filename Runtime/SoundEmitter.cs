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
        public void PlayFrom(float timestamp) => PlayInternal();
        public void PlayFromStart() => PlayFrom(0);
        public void Pause() => handle.Pause();
        public void Stop() => handle.Stop();

        void PlayInternal()
        {
            if (spatial) handle = sound.Play(transform.localPosition, transform);
            else handle = sound.Play();
        }

        // Keep offset up-to-date
        void Update() => handle.Offset = transform.localPosition;
        void OnDestroy() => handle.Stop();
    }
}
