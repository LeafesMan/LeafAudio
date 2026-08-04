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
            else if (!handle.IsDone) PlayFromStart();
        }
        public void PlayFrom(float timestamp) => PlayInternal(timestamp);
        public void PlayFromStart() => PlayFrom(0);
        public void Pause() => handle.Pause();
        public void Stop() => handle.Stop();

        void PlayInternal(float timestamp = 0)
        {
            var settings = sound.WithStartTime(timestamp);

            // Set Pos and Origin if want to play spatially
            if (spatial) settings = settings.WithOrigin(transform).WithPosition(transform.localPosition); ;

            handle = settings.Play();
        }

        // Keep offset up-to-date
        void Update() => handle.Position = transform.localPosition;
        void OnDestroy() => handle.Stop();
    }
}
