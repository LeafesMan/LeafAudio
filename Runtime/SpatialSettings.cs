using UnityEngine;

namespace LeafAudio
{
    [CreateAssetMenu(fileName = "NewSpatialSettings", menuName = "Audio/Spatial Settings", order = -1)]
    public class SpatialSettings : ScriptableObject
    {
        [SerializeField] internal float maxDistance;
        [SerializeField] internal float doppler;

        // Curves are stored 0-1 relative to maxDistance just like Unity's Curve
        [SerializeField] internal AnimationCurve attenuation;
        [SerializeField] internal AnimationCurve spatial;
        [SerializeField] internal AnimationCurve reverb;
        [SerializeField] internal AnimationCurve spread;



        internal const float MinMaxDistance = 1; // The minnimusm value for MaxDistance
        internal static readonly Vector2 DopplerRange = new(0, 5);

#if UNITY_EDITOR
        // The possible ways a Unity's curve values may be driven
        internal enum CurveValueType { Curve, Value, None }
        internal bool useAttenuation = true;
        internal bool useSpatial = false;
        internal CurveValueType reverbType = CurveValueType.None;
        internal CurveValueType spreadType = CurveValueType.None;


        public static float ValidateMaxDistance(float maxDistance) => Mathf.Max(1, maxDistance);
        public static float ValidateDoppler(float doppler) => Mathf.Clamp(doppler, DopplerRange.x, DopplerRange.y);

        void OnValidate()
        {
            maxDistance = ValidateMaxDistance(maxDistance);
            doppler = ValidateDoppler(doppler);
        }
#endif
    }
}