namespace LeafAudio
{
    /// <summary>
    /// Allow Playing Sounds via sound.Play() with the same exact behaviour as playing via Audio.Play(sound). Using extensions ensures no null exception in the case of sound.Play() when sound is null.
    /// </summary>
    public static class AudioExtensions
    {
        public static void Play(this Sound sound, SpatialRolloff spatialSpecs = null) => Audio.Play(sound, spatialSpecs);
        public static void PlayLooping(this Sound sound, float fadeDuration, uint slot) => Audio.PlayLooping(sound, fadeDuration, slot);
    }
}