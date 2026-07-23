using UnityEngine;
namespace LeafAudio
{
    /// <summary>
    /// Allow Playing Sounds via sound.Play() with the same exact behaviour as playing via Audio.Play(sound). Using extensions ensures no null exception in the case of sound.Play() when sound is null.
    /// </summary>
    public static class AudioExtensions
    {
        public static void Play(this Sound sound, Vector3? position = null, Transform origin = null) => Audio.GlobalManager.Play(sound, position, origin);
    }
}