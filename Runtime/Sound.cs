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
    [CreateAssetMenu(fileName = "NewSound", menuName = "FX/Sound")]
    public class Sound : ScriptableObject
    {
        [SerializeField] AudioMixerGroup mixerGroup;
        [SerializeField] SelectionMode selectionMode;
        [SerializeField] List<Weighted<SoundVariant>> weightedVariants;

        public enum SelectionMode { UniformRandom, WeightedRandom }


        /// <summary>
        /// Selects a variant using the specified Selection Mode.
        /// </summary>
        public SoundVariant SelectVariant() => selectionMode == SelectionMode.WeightedRandom ? Rand.ItemWeighted(weightedVariants) : Rand.Item(weightedVariants).Item;
        public AudioMixerGroup Group => mixerGroup;
        public SelectionMode Mode => selectionMode;
        public int VariantCount => weightedVariants.Count;
    }
}