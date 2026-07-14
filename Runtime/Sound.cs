using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        [SerializeField] AudioMixerGroup mixerGroup = null;
        [SerializeField] SelectionMode selectionMode = SelectionMode.UniformRandom;
        [SerializeField] List<Weighted<SoundVariant>> weightedVariants = new() { new Weighted<SoundVariant>(new SoundVariant(), 1) };

        public enum SelectionMode { UniformRandom, WeightedRandom }

#if UNITY_EDITOR
        public enum ValueType { Unique, Shared }
        [SerializeField] ValueType clipType;
        [SerializeField] ValueType volumeType;
        [SerializeField] ValueType pitchType;

        public enum VariationType { Unique, Shared, None }
        [SerializeField] VariationType volumeVariationType;
        [SerializeField] VariationType pitchVariationType;
#endif
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
    }
}