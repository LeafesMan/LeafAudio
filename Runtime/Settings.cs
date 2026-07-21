using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace LeafAudio
{
    [FilePath("ProjectSettings/LeafAudioSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class Settings : ScriptableSingleton<Settings>
    {
        // Runtime
        public int GlobalAudioManagerPoolSize = 30;

#if UNITY_EDITOR

        [System.Serializable]
        public class SoundTemplate
        {
            public AudioMixerGroup AudioMixerGroup = null;
            public float Volume = 0.5f;
            public float VolumeVariation = 0;
            public float Pitch = 1;
            public float PitchVariation = 0.1f;
            public float ReverbMix = 0;
            public Vector2 PitchRange = new Vector2(0, 2);
            public Sound.SelectionMode SelectionMode = Sound.SelectionMode.UniformRandom;
            public Sound.ValueMode ClipMode = Sound.ValueMode.Unique;
            public Sound.ValueMode VolumeMode = Sound.ValueMode.Unique;
            public Sound.ValueMode PitchMode = Sound.ValueMode.Unique;
            public Sound.VariationMode VolumeVariationMode = Sound.VariationMode.None;
            public Sound.VariationMode PitchVariationMode = Sound.VariationMode.Shared;
            public bool UseReverbMix = false;
        }
        public SoundTemplate SoundDefaults;
        public bool WarnOnPlayNullSound = true;
        public Color SliderVariationColor = new Color(68, 136, 202);

        public void SaveSettings() => Save(true);
#endif
    }
}