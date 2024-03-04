/*
 *  Name: Ian
 *
 *  Proj: Audio Library
 *
 *  Date: 7/27/23 
 *  
 *  Desc: A variable clip is the data structure for 1 to many varying audio clips that Contains ranges for volume and pitch.
 */
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Variable Clip", fileName = "NewVariableClip")]
public class VariableClip : PlayableClip
{
    [SerializeField] VariableClipSpecs[] clips;

    [System.Serializable]
    struct VariableClipSpecs
    {
        public AudioClip clip;
        [SerializeField, Slider(0, 1)] private Vector2 volumeRange;
        [SerializeField, Slider(0, 3)] private Vector2 pitchRange;

        public ClipSpecs GetRandomizedClipSpecs() => new ClipSpecs(clip, Random.Range(pitchRange.x, pitchRange.y), Random.Range(volumeRange.x, volumeRange.y));
    }

    public override ClipSpecs GetSpecs() => clips[Random.Range(0, clips.Length)].GetRandomizedClipSpecs();
}
