using UnityEngine;

namespace LeafAudio
{
    public abstract class DistanceProfile : ScriptableObject
    {
        [SerializeField] internal AnimationCurve curve;

#if UNITY_EDITOR
        [SerializeField] internal Vector2 curveDomain = new Vector2(0, 100);
        const float RequiredDomainSize = 0.01f;
        internal abstract bool GetCanShowAsValue();
        internal abstract Vector2 CurveRange { get; }

        void OnValidate()
        {
            curveDomain = ValidateCurveDomain(curveDomain);
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