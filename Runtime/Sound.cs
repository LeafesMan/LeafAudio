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

        /// <summary>
        /// Selects a variant from this sound using this sound's SelectionMode.
        /// </summary>
        public SoundVariant SelectVariant() => SelectVariant(weightedVariants, selectionMode);
        /// <summary>
        /// Selects a variant from WeightedVariants using the specified SelectionMode.
        /// </summary>
        public static SoundVariant SelectVariant(List<Weighted<SoundVariant>> weightedVariants, SelectionMode selectionMode)
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

#if UNITY_EDITOR
        // These values are all used in the SoundEditor
        public enum ValueMode { Unique, Shared }
        [SerializeField] ValueMode clipMode;
        [SerializeField] ValueMode volumeMode;
        [SerializeField] ValueMode pitchMode;

        public enum VariationMode { Unique, Shared, None }
        [SerializeField] VariationMode volumeVariationMode;
        [SerializeField] VariationMode pitchVariationMode;


        void Reset()
        {
            mixerGroup = Settings.instance.SoundDefaults.AudioMixerGroup;
            selectionMode = Settings.instance.SoundDefaults.SelectionMode;
            weightedVariants = new() { new Weighted<SoundVariant>(new SoundVariant(), 1) };

            clipMode = Settings.instance.SoundDefaults.ClipMode;
            volumeMode = Settings.instance.SoundDefaults.VolumeMode;
            pitchMode = Settings.instance.SoundDefaults.PitchMode;
            volumeVariationMode = Settings.instance.SoundDefaults.VolumeVariationMode;
            pitchVariationMode = Settings.instance.SoundDefaults.PitchVariationMode;

            // Set Pitch/Volume and Variations
            var mainVariant = weightedVariants[0].Item;
            mainVariant.volume = Settings.instance.SoundDefaults.Volume;
            mainVariant.pitch = Settings.instance.SoundDefaults.Pitch;
            // Only set Variation when mode is not none
            if (volumeVariationMode != VariationMode.None) mainVariant.volumeVariation = Settings.instance.SoundDefaults.VolumeVariation;
            if (pitchVariationMode != VariationMode.None) mainVariant.pitchVariation = Settings.instance.SoundDefaults.PitchVariation;
        }
#endif
    }
}