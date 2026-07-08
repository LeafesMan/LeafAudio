using UnityEngine;
using LeafRand.Global;

/// <summary>
/// Definition variables concerning a single audio clip.
/// </summary>
[System.Serializable]
public class AudioSpec
{
    [SerializeField] AudioClip clip;

    [SerializeField] float volume = 0.5f;
    [SerializeField] float volumeVariation = 0;
    [SerializeField] float pitch = 1;
    [SerializeField] float pitchVariation = 0.2f;


    public AudioClip GetClip() => clip;
    public float GetVolume() => volume + Rand.Float(-volumeVariation, volumeVariation);
    public float GetPitch() => pitch + Rand.Float(-pitchVariation, pitchVariation);
}