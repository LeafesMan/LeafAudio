using UnityEngine;

namespace LeafAudio
{
    [CreateAssetMenu(fileName = "NewSpatialProfile", menuName = "Audio/Spatial Profile")]
    internal class SpatialProfile : DistanceProfile
    {
        internal override Vector2 CurveRange => Vector2.up;
        internal override bool GetCanShowAsValue() => true;
    }
}