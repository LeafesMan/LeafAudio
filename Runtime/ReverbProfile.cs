using UnityEngine;
namespace LeafAudio
{
    [CreateAssetMenu(fileName = "NewReverbProfile", menuName = "Audio/Reverb Profile")]
    public class ReverbProfile : DistanceProfile
    {
#if UNITY_EDITOR
        internal override bool CanShowAsValue => true;
        internal override Vector2 CurveRange => Vector2.up * 1.1f;
#endif
    }
}