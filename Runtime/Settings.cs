using UnityEditor;

namespace LeafAudio
{
    [FilePath("ProjectSettings/LeafAudioSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class Settings : ScriptableSingleton<Settings>
    {
        // Runtime
        public int GlobalAudioManagerPoolSize = 30;

#if UNITY_EDITOR

#endif

        public void SaveSettings() => Save(true);
    }
}