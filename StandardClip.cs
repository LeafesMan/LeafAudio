/*
 * Auth: Ian
 * 
 * Proj: Untitled Moster Game
 * 
 * Date: 10/19/23
 * 
 * Desc: A standard clip plays an audioclip with a set volume and pitch
 */

using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Standard Clip", fileName = "NewStandardClip")]
public class StandardClip : PlayableClip
{
    [SerializeField] AudioClip clip;
    [SerializeField] float volume;
    [SerializeField] float pitch;

    public override ClipSpecs GetSpecs() => new ClipSpecs(clip, pitch, volume);
}
