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
    [CreateAssetMenu(fileName = "NewSound", menuName = "Audio/Sound")]
    public class Sound : ScriptableObject
    {
        [SerializeField] internal AudioMixerGroup mixerGroup;
        [SerializeField] internal AttenuationProfile attenuation;
        [SerializeField] internal SpreadProfile spread;
        [SerializeField] internal ReverbProfile reverb;
        [SerializeField] internal SelectionMode selectionMode;
        [SerializeField] internal List<Weighted<SoundVariant>> weightedVariants;
        [SerializeField] internal Vector2 pitchRange;

        readonly AnimationCurve DefaultAttenuationCurve = new AnimationCurve(new Keyframe(0, 1));
        readonly AnimationCurve DefaultReverbCurve = new AnimationCurve(new Keyframe(0, 1));
        readonly AnimationCurve DefaultSpreadCurve = new AnimationCurve(new Keyframe(0, 0));

        internal static Sound Template;

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
            AnimationCurve spreadCurve = spread == null ? DefaultAttenuationCurve : attenuation.curve;
            AnimationCurve reverbCurve = reverb == null ? DefaultReverbCurve : reverb.curve;

            return new PlaybackSettings(variant.clip, volume, pitch, mixerGroup, attenuationCurve, spreadCurve, reverbCurve);
        }

#if UNITY_EDITOR
        // These values are all used in the SoundEditor

        // Whether values are shared between variants
        [SerializeField] internal bool shareClip;
        [SerializeField] internal bool shareVolume;
        [SerializeField] internal bool sharePitch;

        // Whether ReverbMix will be non-zero and shown
        [SerializeField] internal bool useReverbMix;
        [SerializeField] internal bool useSpatialSettings;

        public enum VariationMode { Unique, Shared, None }
        [SerializeField] internal VariationMode volumeVariationMode;
        [SerializeField] internal VariationMode pitchVariationMode;

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

            // Ensure reverb zone mix is accurate
            //if (!useReverbMix) reverbMix = 0;
            //else reverbMix = Mathf.Clamp(reverbMix, 0, 1.1f);

            // Ensure spatial settings is accurate
            //if (!useSpatialSettings) spatialSettings = null;

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
        void Reset()
        {
            mixerGroup = Template.mixerGroup;
            selectionMode = Template.selectionMode;
            pitchRange = Template.pitchRange;
            shareClip = Template.shareClip;
            shareVolume = Template.shareVolume;
            sharePitch = Template.sharePitch;
            volumeVariationMode = Template.volumeVariationMode;
            pitchVariationMode = Template.pitchVariationMode;
            useReverbMix = Template.useReverbMix;
            useSpatialSettings = Template.useSpatialSettings;


            weightedVariants = new();
            for (int i = 0; i < Template.weightedVariants.Count; i++) AddVariantCopy(i);
            void AddVariantCopy(int i)
            {
                SoundVariant variant = new SoundVariant();
                variant.volume = Template.weightedVariants[i].Item.volume;
                variant.volumeVariation = Template.weightedVariants[i].Item.volumeVariation;
                variant.pitch = Template.weightedVariants[i].Item.pitch;
                variant.pitchVariation = Template.weightedVariants[i].Item.pitchVariation;

                weightedVariants.Add(new(variant));
            }
        }

#endif
    }
}