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
    }
}