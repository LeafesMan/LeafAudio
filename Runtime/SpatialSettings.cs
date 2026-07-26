using UnityEngine;

namespace LeafAudio
{
    public class SpatialSettings : ScriptableObject
    {
        public float MaxDistance => maxDistance;
        internal float maxDistance;

        // Curves are stored 0-1 relative to maxDistance just like Unity's Curve
        internal AnimationCurve attenuation;
        internal AnimationCurve spatial;
        internal AnimationCurve reverb;
        internal AnimationCurve spread;
    }
}