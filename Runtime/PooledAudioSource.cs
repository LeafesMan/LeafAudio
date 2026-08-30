using System;
using UnityEngine;
namespace LeafAudio
{
    /// <summary>
    /// Struct for data stored about every source in the pool.
    /// </summary>
    [Serializable]
    internal class PooledAudioSource
    {
        internal static uint PlaybackIDCounter = 1; // Value of 0 is reserved for free sources

        [SerializeField] internal bool killOnDone; // Whether the source should be killed when it is done
        [SerializeField] internal AudioSource source; // Unchanging for the life of a pooled audio source

        [SerializeField] internal uint playbackID;
        [SerializeField] internal int usedIndex; // The index of this source in usedIndices
        [SerializeField] internal DurationMode durationMode;
        [SerializeField] internal float remainingDuration;
        [SerializeField] internal bool paused;

        // We maintain this outside the Unity AudioSource to avoid losing the curve
        // Upon nullifying both position+origin the UnitySource Spatial Blend Curve is set to 0 to stop all spatial playback. Thus we store this separately incase position/origin are set again we dont want to lose the curve 
        [SerializeField] internal AnimationCurve spatialBlendCurve;
        static readonly AnimationCurve NoSpatialCurve = new AnimationCurve(new Keyframe(0, 0));
        [SerializeField] Transform origin = null;
        [SerializeField] Vector3? position = null;

        [SerializeField] private float pitch; // The user-set pitch
        [SerializeField] private bool ignoreTimescale;

        public void SetOriginWithoutNotify(Transform newOrigin) => origin = newOrigin;
        public void SetPositionWithoutNotify(Vector3? newPos) => position = newPos;
        public Transform Origin
        {
            get => origin;
            set
            {
                if (value == origin) return;
                origin = value;
                UpdateWorldPosition();
                UpdateSpatialCurve();
            }
        }
        public Vector3? Position
        {
            get => position;
            set
            {
                if (value == position) return;
                position = value;
                UpdateWorldPosition();
                UpdateSpatialCurve();
            }
        }
        public void UpdateSpatialCurve()
        {
            if (position != null || Origin != null) source.SetCustomCurve(AudioSourceCurveType.SpatialBlend, spatialBlendCurve);
            else source.SetCustomCurve(AudioSourceCurveType.SpatialBlend, NoSpatialCurve);
        }
        /// <summary>
        /// Updates the sources world position based on Position & Origin
        /// </summary>
        public void UpdateWorldPosition() => source.transform.position = position ?? Vector3.zero + (origin == null ? Vector3.zero : origin.position);
        public void UpdateWorldPositionOriginNull() => source.transform.position = position ?? Vector3.zero + origin.position;

        public float Pitch
        {
            get => pitch;
            set
            {
                pitch = value;
                UpdateSourcePitch();
            }
        }
        public bool IgnoreTimeScale
        {
            get => ignoreTimescale;
            set
            {
                ignoreTimescale = value;
                UpdateSourcePitch();
            }
        }


        /// <summary>
        /// Whether the pooled audio source has completed playback
        /// </summary>
        public bool IsDone => remainingDuration == 0;

        /// <summary>
        /// Updates the pitch on the source using the user-set pitch and the timescale if ignoreTimeScale is false
        /// </summary>
        public void UpdateSourcePitch() => source.pitch = pitch * (ignoreTimescale ? 1 : Time.timeScale);
    }
}