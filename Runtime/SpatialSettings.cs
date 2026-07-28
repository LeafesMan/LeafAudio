using UnityEngine;

namespace LeafAudio
{
    [CreateAssetMenu(fileName = "NewSpatialSettings", menuName = "Audio/Spatial Settings", order = -1)]
    public class SpatialSettings : ScriptableObject
    {
        [SerializeField] internal float maxDistance = DefaultMaxDistance;
        [SerializeField] internal float doppler = DefaultDoppler;

        // Curves are stored 0-1 relative to maxDistance just like Unity's Curve
        [SerializeField] internal AnimationCurve attenuation = new AnimationCurve(new Keyframe(0, DefaultAttenuation));
        [SerializeField] internal AnimationCurve spatial = new AnimationCurve(new Keyframe(0, DefaultSpatial));
        [SerializeField] internal AnimationCurve reverb = new AnimationCurve(new Keyframe(0, DefaultReverb));
        [SerializeField] internal AnimationCurve spread = new AnimationCurve(new Keyframe(0, DefaultSpread));




        internal const float DefaultMaxDistance = 100;
        internal const float DefaultDoppler = 1;
        internal const float DefaultAttenuation = 1;
        internal const float DefaultSpatial = 1;
        internal const float DefaultReverb = 1;
        internal const float DefaultSpread = 0;
        internal static readonly Vector2 DopplerRange = new(0, 5);

#if UNITY_EDITOR
        [SerializeField] internal bool useAttenuation = true;
        [SerializeField] internal bool useSpatial = false;
        [SerializeField] internal bool useDoppler = false;
        [SerializeField] internal CurveValueType reverbType = CurveValueType.Value;
        [SerializeField] internal CurveValueType spreadType = CurveValueType.None;
        internal enum CurveValueType { Curve, Value, None }// The possible ways a Unity's curve values may be driven

        readonly AnimationCurve DefaultAttenuationCurve = new AnimationCurve(new Keyframe(0, 1, 0, -3f), new Keyframe(1, 0));


        public static float ValidateMaxDistance(float maxDistance) => Mathf.Max(1, maxDistance);
        public static float ValidateDoppler(float doppler) => Mathf.Clamp(doppler, DopplerRange.x, DopplerRange.y);
        void ValidateCurve(AnimationCurve curve, float range = 1)
        {
            var keys = curve.keys;


            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].time = Mathf.Clamp01(keys[i].time);
                keys[i].value = Mathf.Clamp(keys[i].value, 0f, range);
            }
            curve.keys = keys;
        }
        void ValidateValueCurve(AnimationCurve curve, float range = 1)
        {   // Ensure only one keyframe with timestamp 0

            Keyframe firstKeyFrame = curve.keys[0];
            if (curve.keys[0].time != 0) firstKeyFrame.time = 0; // Ensure Keyframe t = 0

            // Apply and Ensure 1 Keyframe
            curve.ClearKeys();
            curve.AddKey(firstKeyFrame);

            ValidateCurve(curve, range);
        }
        void MakeCurveFlat(AnimationCurve curve, float value) { curve.ClearKeys(); curve.AddKey(new Keyframe(0, value)); }

        void OnValidate()
        {
            maxDistance = ValidateMaxDistance(maxDistance);
            doppler = useDoppler ? ValidateDoppler(doppler) : 1;

            if (useAttenuation) ValidateCurve(attenuation);
            else MakeCurveFlat(attenuation, DefaultAttenuation);

            if (useSpatial) ValidateCurve(spatial);
            else MakeCurveFlat(spatial, DefaultSpatial);
            if (reverbType == CurveValueType.Curve) ValidateCurve(reverb, 1.1f);
            else if (reverbType == CurveValueType.Value) ValidateValueCurve(reverb, 1.1f);
            else MakeCurveFlat(reverb, 0);

            if (spreadType == CurveValueType.Curve) ValidateCurve(spread);
            else if (spreadType == CurveValueType.Value) ValidateValueCurve(spread);
            else MakeCurveFlat(spread, 0);
        }
        void Reset()
        {
            attenuation = DefaultAttenuationCurve;
            OnValidate();
        }
#endif
    }
}