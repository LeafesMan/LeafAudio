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

        public static void Play(Sound sound, SpatialRolloff spatialSpecs = null) => GlobalManager.Play(sound, spatialSpecs);
        public static void PlayLooping(Sound sound, float fadeDuration, uint slot) => GlobalManager.PlayLooping(sound, fadeDuration, slot);

        /// <summary>
        /// Applies Playbacksettings to an AudioSource
        /// </summary>
        /// <param name="source"></param>
        /// <param name="playbackSettings"></param>
        public static void ApplyPlaybackSettings(AudioSource source, PlaybackSettings playbackSettings)
        {
            // Setup Source
            source.clip = playbackSettings.clip;
            source.volume = playbackSettings.volume;
            source.pitch = playbackSettings.pitch;
            source.outputAudioMixerGroup = playbackSettings.mixerGroup;
            source.reverbZoneMix = playbackSettings.reverbMix;
            if (source.pitch < 0) source.time = source.clip.length - 0.001f; // Flip the clip small subtraction stops from setting timestamp out-of-range causing an error
        }
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