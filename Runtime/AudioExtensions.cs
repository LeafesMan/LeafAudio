using UnityEngine;
namespace LeafAudio
{
    /// <summary>
    /// Allow Playing Sounds via sound.Play() with the same exact behaviour as playing via Audio.Play(sound). 
    /// * Using extensions ensures no null exception in the case of sound.Play() when sound is null (Note this is not neccesary for PlaybackSettings.Play() as it is a struct)
    /// </summary>
    public static class AudioExtensions
    {
        /// <summary>
        /// * Note calling this method on a null sound will result in an early out and a warning rather than a null ref.
        /// </summary>
        public static PlaybackHandle Play(this Sound sound) => AudioManager.Global.Play(sound);
        /// <summary>
        /// Gets PlaybackSettings from this sound using this sound's selection mode and a variant's variation properties.<br/>
        /// * Note calling this method on a null sound will result in an early out and a warning rather than a null ref.
        /// </summary>
        public static PlaybackSettings GetPlaybackSettings(this Sound sound) => sound.GetPlaybackSettingsInternal(sound.weightedVariants);
    }
}