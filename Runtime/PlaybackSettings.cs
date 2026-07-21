using UnityEngine;
using UnityEngine.Audio;
namespace LeafAudio
{
    /// <summary>
    /// Required settings for playing a sound through an AudioManager
    /// </summary>
    public struct PlaybackSettings
    {
        public AudioMixerGroup mixerGroup;
        public AudioClip clip;
        public float volume;
        public float pitch;
        public float reverbMix;
        public PlaybackSettings(AudioClip clip, float volume, float pitch, AudioMixerGroup mixerGroup, float reverbMix)
        {
            this.mixerGroup = mixerGroup;
            this.clip = clip;
            this.volume = volume;
            this.pitch = pitch;
            this.reverbMix = reverbMix;
        }
    }
}