using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
namespace LeafAudio.Editor
{
    public class SoundTester
    {
        /// <summary>
        /// Test Audio Clips & Assets on double click 
        /// </summary>
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID)
        {
            // First only Test Audio in the Project window
            var focusedWindow = EditorWindow.focusedWindow;
            if (focusedWindow == null || focusedWindow.GetType().Name != "ProjectBrowser") return false;

            // Grab the Asset
            Object asset = EditorUtility.EntityIdToObject(instanceID);


            // Handle AudioClip
            if (asset.GetType() == typeof(AudioClip))
            {
                var clip = asset as AudioClip;
                Test(new PlaybackSettings(clip, 0.5f, 1, null, null));
                return true;
            }
            // Handle Sound
            if (asset.GetType() == typeof(Sound))
            {
                Sound sound = asset as Sound;
                Test(sound.GetPlaybackSettings());
                return true;
            }

            return false;
        }


        /// <summary>
        /// Tests the clip by creating a temporary gameobject with an audio source on it then destroying it.
        /// </summary>
        public static void Test(PlaybackSettings playbackSettings)
        {
            if (playbackSettings.Clip == null) { Debug.LogWarning("Sound Testing Skipped: Can't test null clip!"); return; }


            // Create Temp Object and Components
            AudioSource source = new GameObject("SoundTest").AddComponent<AudioSource>();
            source.gameObject.hideFlags = HideFlags.DontSave;

            // Setup Source
            playbackSettings.ApplyToUnityAudioSource(source);
            source.Play();

            // Destroy temporary Object after the clips completion            
            if (Application.isPlaying) Object.Destroy(source.gameObject, playbackSettings.RealTimeDuration);
            else
            {
                double destroyTime = EditorApplication.timeSinceStartup + playbackSettings.RealTimeDuration;
                EditorApplication.update += Cleanup;

                void Cleanup()
                {
                    if (EditorApplication.timeSinceStartup > destroyTime && source != null)
                    {
                        Object.DestroyImmediate(source.gameObject);

                        EditorApplication.update -= Cleanup;
                    }
                }
            }
        }
    }
}