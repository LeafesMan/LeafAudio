using UnityEngine;
namespace LeafAudio
{
    [CreateAssetMenu(fileName = "NewAttenuationProfile", menuName = "Audio/Attenuation Profile", order = -1)]
    public class AttenuationProfile : DistanceProfile
    {
#if UNITY_EDITOR
        internal override bool GetCanShowAsValue() => false;
        internal override Vector2 CurveRange => Vector2.up;
#endif
    }
}