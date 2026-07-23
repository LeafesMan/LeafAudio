using UnityEditor;
using UnityEngine;

namespace LeafAudio.Editor
{
    [FilePath("ProjectSettings/LeafAudioSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class Settings : ScriptableSingleton<Settings>
    {
        void OnEnable() => ApplySettings();


        public bool WarnOnPlayNullSound = true;
        public Color SliderVariationColor = new Color(68, 136, 202);

        void ApplySettings()
        {
            AudioManager.WarnOnPlayNullSound = WarnOnPlayNullSound;
        }
        public void SaveSettings() { ApplySettings(); Save(true); }
    }
}