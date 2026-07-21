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
        [SerializeField] AudioMixerGroup mixerGroup;
        [SerializeField] SelectionMode selectionMode;
        [SerializeField] List<Weighted<SoundVariant>> weightedVariants;
        [SerializeField] Vector2 pitchRange;
        [SerializeField] float reverbMix;

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

            return new PlaybackSettings(variant.clip, volume, pitch, mixerGroup, reverbMix);
        }

#if UNITY_EDITOR
        // These values are all used in the SoundEditor
        public enum ValueMode { Unique, Shared }
        [SerializeField] ValueMode clipMode;
        [SerializeField] ValueMode volumeMode;
        [SerializeField] ValueMode pitchMode;

        [SerializeField] bool useReverbMix;

        public enum VariationMode { Unique, Shared, None }
        [SerializeField] VariationMode volumeVariationMode;
        [SerializeField] VariationMode pitchVariationMode;

        void OnValidate()
        {
            // Ensure there is a variant
            if (weightedVariants == null) weightedVariants = new();
            if (weightedVariants.Count == 0)
            {
                SoundVariant defaultVariant = new SoundVariant();
                defaultVariant.Reset();

                weightedVariants.Add(new(defaultVariant));
            }
            // Ensure pitch range is valid
            if (pitchRange.x > pitchRange.y) pitchRange = Vector2.one * pitchRange.x;

            // Ensure pitches are in pitch range
            foreach (var variant in weightedVariants) variant.Item.pitch = Mathf.Clamp(variant.Item.pitch, pitchRange.x, pitchRange.y);

            // Ensure reverb zone mix is accurate
            if (!useReverbMix) reverbMix = 0;
            else reverbMix = Mathf.Clamp(reverbMix, 0, 1.1f);

            // Ensure shared values are shared
            SoundVariant firstVariant = weightedVariants[0].Item;
            foreach (var weightedVariant in weightedVariants)
            {
                SoundVariant variant = weightedVariant.Item;

                // Update Shared Fields
                if (clipMode == ValueMode.Shared) variant.clip = firstVariant.clip;
                if (volumeMode == ValueMode.Shared) variant.volume = firstVariant.volume;
                if (volumeVariationMode == VariationMode.Shared) variant.volumeVariation = firstVariant.volumeVariation;
                if (pitchMode == ValueMode.Shared) variant.pitch = firstVariant.pitch;
                if (pitchVariationMode == VariationMode.Shared) variant.pitchVariation = firstVariant.pitchVariation;
            }
        }
        void Reset()
        {
            mixerGroup = Settings.instance.SoundDefaults.AudioMixerGroup;
            selectionMode = Settings.instance.SoundDefaults.SelectionMode;
            weightedVariants = new() { new Weighted<SoundVariant>(new SoundVariant(), 1) };
            pitchRange = Settings.instance.SoundDefaults.PitchRange;

            clipMode = Settings.instance.SoundDefaults.ClipMode;
            volumeMode = Settings.instance.SoundDefaults.VolumeMode;
            pitchMode = Settings.instance.SoundDefaults.PitchMode;
            volumeVariationMode = Settings.instance.SoundDefaults.VolumeVariationMode;
            pitchVariationMode = Settings.instance.SoundDefaults.PitchVariationMode;
            useReverbMix = Settings.instance.SoundDefaults.UseReverbMix;

            // Set Pitch/Volume and Variations
            var mainVariant = weightedVariants[0].Item;
            mainVariant.volume = Settings.instance.SoundDefaults.Volume;
            mainVariant.pitch = Settings.instance.SoundDefaults.Pitch;
            // Only set Variation when mode is not none
            if (volumeVariationMode != VariationMode.None) mainVariant.volumeVariation = Settings.instance.SoundDefaults.VolumeVariation;
            if (pitchVariationMode != VariationMode.None) mainVariant.pitchVariation = Settings.instance.SoundDefaults.PitchVariation;
            if (useReverbMix) reverbMix = Settings.instance.SoundDefaults.ReverbMix;
        }
#endif
    }
}