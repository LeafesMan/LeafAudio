using UnityEngine;

namespace LeafAudio
{
    [CreateAssetMenu(fileName = "NewSpatialSettings", menuName = "Audio/SpatialSettings")]
    public class SpatialSettings : ScriptableObject
    {
        public float dopplerLevel;
        public AnimationCurve volume;
        public AnimationCurve spatialBlend;
        public AnimationCurve spread;
        public AnimationCurve reverbZoneMix;
    }
}