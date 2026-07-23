using UnityEngine;

namespace LeafAudio
{
    public abstract class DistanceProfile : ScriptableObject
    {
        [SerializeField] internal AnimationCurve curve;

#if UNITY_EDITOR
        [SerializeField] internal Vector2 curveDomain = new Vector2(0, 100);
        const float RequiredDomainSize = 0.01f;
        [SerializeField] internal bool useCurve = true;
        internal abstract bool CanShowAsValue { get; }
        internal abstract Vector2 CurveRange { get; }

        void OnValidate()
        {
            curveDomain = ValidateCurveDomain(curveDomain);

            // When showing as value
            // - Enforce a single keyframe
            // - Ensure that keyframe is at time 0 with no tangents set
            if (CanShowAsValue && !useCurve)
            {
                if (curve.keys.Length == 1) curve.SetKeys(new Keyframe[] { new(0, curve.keys[0].value) });
                else curve.SetKeys(new Keyframe[] { new(0, 0) });
            }
        }
        /// <summary>
        /// Returns a Validated CurveDomain ensuring y >= x and both >= 0
        /// </summary>
        static internal Vector2 ValidateCurveDomain(Vector2 toValidate)
        {
            if (toValidate.x < 0) toValidate.x = 0;
            if (toValidate.y <= toValidate.x + RequiredDomainSize) toValidate.y = toValidate.x + RequiredDomainSize;

            return toValidate;
        }
#endif
    }
}