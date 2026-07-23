using UnityEditor;
namespace LeafAudio.Editor
{
    public class HideMenuButtons
    {
        [MenuItem("Assets/Create/Audio/Audio Random Container", true)]
        static bool ValidateCreateSound() => false;
        [MenuItem("GameObject/Audio/Audio Source", true)]
        static bool ValidateCreateAudioSource() => false;
    }
}