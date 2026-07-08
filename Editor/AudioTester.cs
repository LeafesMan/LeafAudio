using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
namespace LeafAudio.Editor
{
    public class AudioTester
    {
        const float DEFAULT_VOLUME = 0.1f;

        /// <summary>
        /// Test Audio Clips & Assets on double click 
        /// </summary>
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID)
        {
            Object asset = EditorUtility.EntityIdToObject(instanceID);

            Debug.Log($"Double-clicked: {asset.name} ({asset.GetType().Name})");

            // Handle AudioClip
            if (asset.GetType() == typeof(AudioClip))
            {
                Test(asset as AudioClip, DEFAULT_VOLUME, 1);
                return true;
            }
            if (asset.GetType() == typeof(AudioAsset))
            {
                AudioAsset audioAsset = asset as AudioAsset;
                AudioSpec spec = audioAsset.Audio.RandomAudioSpec;
                Test(spec.GetClip(), spec.GetVolume(), spec.GetPitch());
                return true;
            }

            return false;
        }


        /// <summary>
        /// Tests the clip by creating a temporary gameobject with an audio source on it then destroying it.
        /// </summary>
        public static void Test(AudioClip clip, float volume, float pitch)
        {
            // Create Temp Object and Components
            AudioSource source = new GameObject("Audio Test (DELETE ME)").AddComponent<AudioSource>();

            // Setup Source
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;

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