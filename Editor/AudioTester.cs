using UnityEditor;
using UnityEngine;

namespace LeafAudio.Editor
{
    public class AudioTester
    {
        /// <summary>
        /// Tests the clip by creating a temporary gameobject with an audio source on it then destroying it.
        /// </summary>
        public static void Test(AudioSpec spec)
        {
            // Create Temp Object and Components
            AudioSource source = new GameObject("Audio Test (DELETE ME)").AddComponent<AudioSource>();

            // Setup Source
            source.clip = spec.GetClip();
            source.volume = spec.GetVolume();
            source.pitch = spec.GetPitch();

            source.Play();

            // Destroy temporary Object after the clips completion
            if (Application.isPlaying) Object.Destroy(source.gameObject, source.clip.length);
            else
            {
                double destroyTime = EditorApplication.timeSinceStartup + source.clip.length;
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