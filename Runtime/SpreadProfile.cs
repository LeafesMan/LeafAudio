using UnityEngine;
namespace LeafAudio
{
    [CreateAssetMenu(fileName = "NewSpreadProfile", menuName = "Audio/Spread Profile")]
    public class SpreadProfile : DistanceProfile
    {
#if UNITY_EDITOR
        internal override bool GetCanShowAsValue() => true;
        internal override Vector2 CurveRange => Vector2.up;
#endif
    }
}