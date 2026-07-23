using UnityEngine;
namespace LeafAudio
{
    /// <summary>
    /// Global Wrapper for an AudioManager
    /// </summary>
    public static class Audio
    {
        public static AudioManager GlobalManager { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void SetupGlobalAudioManager()
        {
            GlobalManager = new GameObject("AudioManager").AddComponent<AudioManager>();
            Object.DontDestroyOnLoad(GlobalManager.gameObject);
        }

        public static void Play(Sound sound, Vector3? position = null, Transform origin = null) => Audio.GlobalManager.Play(sound, position, origin);


        /// <summary>
        /// Gets Duration of the given clip with the given pitch.
        /// </summary>
        public static float GetDuration(AudioClip clip, float pitch) => Mathf.Abs(clip.length / pitch);
        /// <summary>
        /// Gets Duration of the given playback settings.
        /// </summary>
        public static float GetDuration(PlaybackSettings playbackSettings) => GetDuration(playbackSettings.clip, playbackSettings.pitch);
    }
}