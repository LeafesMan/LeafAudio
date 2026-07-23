using System.Collections.Generic;
using LeafRand.Collections;
using LeafRand.Global;
using UnityEngine;
using UnityEngine.Audio;
namespace LeafAudio
{
    /// <summary>
    /// A reusable sound asset containing multiple variants that will be randomly selected from for playback.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSound", menuName = "Audio/Sound", order = -2)]
    public class Sound : ScriptableObject
    {
        [SerializeField] internal AudioMixerGroup mixerGroup = null;
        [SerializeField] internal AttenuationProfile attenuation = null;
        [SerializeField] internal SpreadProfile spread = null;
        [SerializeField] internal ReverbProfile reverb = null;
        [SerializeField] internal SelectionMode selectionMode = SelectionMode.UniformRandom;
        [SerializeField] internal List<Weighted<SoundVariant>> weightedVariants = new() { new() };
        [SerializeField] internal Vector2 pitchRange = new Vector2(0, 2);

        readonly AnimationCurve DefaultAttenuationCurve = new AnimationCurve(new Keyframe(0, 1));
        readonly AnimationCurve DefaultReverbCurve = new AnimationCurve(new Keyframe(0, 1));
        readonly AnimationCurve DefaultSpreadCurve = new AnimationCurve(new Keyframe(0, 0));

        /// <summary>
        /// Selects a variant from WeightedVariants using the specified SelectionMode.
        /// </summary>
        SoundVariant SelectVariant(List<Weighted<SoundVariant>> weightedVariants)
        {
            switch (selectionMode)
            {
                case SelectionMode.UniformRandom: return Rand.Item(weightedVariants).Item;
                case SelectionMode.WeightedRandom: return Rand.ItemWeighted(weightedVariants);
            }
            throw new System.Exception("Undefined selection mode!");
        }
        public AudioMixerGroup Group => mixerGroup;
        public SelectionMode Mode => selectionMode;
        public int VariantCount => weightedVariants.Count;

        public enum SelectionMode { UniformRandom, WeightedRandom }

        /// <summary>
        /// Gets PlaybackSettings from this sound using this sound's selection mode and a variant's variation properties.
        /// </summary>
        public PlaybackSettings GetPlaybackSettings() => GetPlaybackSettingsFromVariants(weightedVariants);

        /// <summary>
        /// Gets PlaybackSettings from this sound but uses the specified weightedVariants
        /// </summary>
        internal PlaybackSettings GetPlaybackSettingsFromVariants(List<Weighted<SoundVariant>> weightedVariants)
        {
            var variant = SelectVariant(weightedVariants);

            // Clamp Volume/Pitch so that they aren't outside of their respective ranges
            // * Clamping the variation amount rather than then final value ensures we get uniform distribution across our range
            Vector2 volumeVariationRange = new Vector2(-variant.volume, 1 - variant.volume); // Volume has a static range of 0,1
            float volume = variant.volume + Rand.Float(Mathf.Max(-variant.volumeVariation, volumeVariationRange.x), Mathf.Min(variant.volumeVariation, volumeVariationRange.y));

            Vector2 pitchVariationRange = new Vector2(pitchRange.x - variant.pitch, pitchRange.y - variant.pitch);
            float pitch = variant.pitch + Rand.Float(Mathf.Max(-variant.pitchVariation, pitchVariationRange.x), Mathf.Min(variant.pitchVariation, pitchVariationRange.y));

            AnimationCurve attenuationCurve = attenuation == null ? DefaultAttenuationCurve : attenuation.curve;
            AnimationCurve spreadCurve = spread == null ? DefaultSpreadCurve : attenuation.curve;
            AnimationCurve reverbCurve = reverb == null ? DefaultReverbCurve : reverb.curve;

            return new PlaybackSettings(variant.clip, volume, pitch, mixerGroup, attenuationCurve, spreadCurve, reverbCurve);
        }

#if UNITY_EDITOR
        // These values are all used in the SoundEditor

        // Whether values are shared between variants
        [SerializeField] internal bool shareClip = false;
        [SerializeField] internal bool shareVolume = false;
        [SerializeField] internal bool sharePitch = false;

        // Whether the following fields will be shown and used 
        [SerializeField] internal bool useAttenuation = false;
        [SerializeField] internal bool useReverb = false;
        [SerializeField] internal bool useSpread = false;

        public enum VariationMode { Unique, Shared, None }
        [SerializeField] internal VariationMode volumeVariationMode = VariationMode.None;
        [SerializeField] internal VariationMode pitchVariationMode = VariationMode.Unique;

        void OnValidate()
        {
            // Ensure there is a variant
            if (weightedVariants == null) weightedVariants = new();
            if (weightedVariants.Count == 0)
            {
                SoundVariant defaultVariant = new SoundVariant();

                weightedVariants.Add(new(defaultVariant));
            }
            // Ensure pitch range is valid
            if (pitchRange.x > pitchRange.y) pitchRange = Vector2.one * pitchRange.x;

            // Ensure pitches are in pitch range
            foreach (var variant in weightedVariants) variant.Item.pitch = Mathf.Clamp(variant.Item.pitch, pitchRange.x, pitchRange.y);

            // Ensure Spatial Fields are nullified if not using
            if (!useAttenuation) attenuation = null;
            if (!useReverb) reverb = null;
            if (!useSpread) spread = null;

            // Ensure shared values are shared
            SoundVariant firstVariant = weightedVariants[0].Item;
            foreach (var weightedVariant in weightedVariants)
            {
                SoundVariant variant = weightedVariant.Item;

                // Update Shared Fields
                if (shareClip) variant.clip = firstVariant.clip;
                if (shareVolume) variant.volume = firstVariant.volume;
                if (volumeVariationMode == VariationMode.Shared) variant.volumeVariation = firstVariant.volumeVariation;
                if (sharePitch) variant.pitch = firstVariant.pitch;
                if (pitchVariationMode == VariationMode.Shared) variant.pitchVariation = firstVariant.pitchVariation;
            }
        }
#endif
    }
}