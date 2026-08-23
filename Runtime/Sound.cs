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
        [SerializeField] internal SpatialSettings spatialSettings = null;
        [SerializeField] internal SelectionMode selectionMode = SelectionMode.UniformRandom;
        [SerializeField] internal List<Weighted<SoundVariant>> weightedVariants = new() { new() };
        [SerializeField] internal Vector2 pitchRange = new Vector2(0, 2);

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
        /// Gets PlaybackSettings from this sound but uses the specified weightedVariants
        /// </summary>
        internal PlaybackSettings GetPlaybackSettingsInternal(List<Weighted<SoundVariant>> weightedVariants)
        {
            var variant = SelectVariant(weightedVariants);

            // Clamp Volume/Pitch so that they aren't outside of their respective ranges
            // * Clamping the variation amount rather than then final value ensures we get uniform distribution across our range
            Vector2 volumeVariationRange = new Vector2(-variant.volume, 1 - variant.volume); // Volume has a static range of 0,1
            float volume = variant.volume + Rand.Float(Mathf.Max(-variant.volumeVariation, volumeVariationRange.x), Mathf.Min(variant.volumeVariation, volumeVariationRange.y));

            Vector2 pitchVariationRange = new Vector2(pitchRange.x - variant.pitch, pitchRange.y - variant.pitch);
            float pitch = variant.pitch + Rand.Float(Mathf.Max(-variant.pitchVariation, pitchVariationRange.x), Mathf.Min(variant.pitchVariation, pitchVariationRange.y));


            return new PlaybackSettings(variant.clip, volume, pitch, mixerGroup, spatialSettings);
        }
        #region With Chaining Wrappers
        // Wrappers to allow same chaining on Sound as on PlaybackSettings
        // With these methods we can do sound.WithXXXX().Play();
        // Without we need to do        sound.GetPlaybackSettings().WithXXXX().Play();
        public PlaybackSettings WithMixerGroup(AudioMixerGroup mixerGroup) => this.GetPlaybackSettings().WithMixerGroup(mixerGroup);
        public PlaybackSettings WithClip(AudioClip clip) => this.GetPlaybackSettings().WithClip(clip);
        public PlaybackSettings WithStartTime(float startTime) => this.GetPlaybackSettings().WithStartTime(startTime);
        public PlaybackSettings WithVolume(float volume) => this.GetPlaybackSettings().WithVolume(volume);
        public PlaybackSettings WithPitch(float pitch) => this.GetPlaybackSettings().WithPitch(pitch);
        public PlaybackSettings WithPosition(Vector3? position) => this.GetPlaybackSettings().WithPosition(position);
        public PlaybackSettings WithOrigin(Transform origin) => this.GetPlaybackSettings().WithOrigin(origin);
        public PlaybackSettings WithSpatialSettings(SpatialSettings settings) => this.GetPlaybackSettings().WithSpatialSettings(settings);
        /// <summary> 
        /// Sets the playback duration in seconds.<br/> 
        /// Overrides any previous duration specification.
        /// </summary>
        public PlaybackSettings WithRealTimeDuration(float duration) => this.GetPlaybackSettings().WithRealTimeDuration(duration);
        /// <summary> 
        /// Sets the playback duration in seconds.<br/> 
        /// Overrides any previous duration specification.
        /// </summary>
        public PlaybackSettings WithClipTimeDuration(float duration) => this.GetPlaybackSettings().WithClipTimeDuration(duration);
        /// <summary> 
        /// Sets the playback duration as a number of traversals of the clip length. <br/> 
        /// Overrides any previous duration specification.
        /// </summary>
        public PlaybackSettings WithTraversals(float traversals) => this.GetPlaybackSettings().WithTraversals(traversals);
        /// <summary> 
        /// Sets the playback duration as a number of loops after the initial traversal of the clip. <br/> 
        /// Overrides any previous duration specification.
        /// </summary>
        public PlaybackSettings WithLoops(float loops) => this.GetPlaybackSettings().WithLoops(loops);
        public PlaybackSettings WithKillOnDone(bool value) => this.GetPlaybackSettings().WithKillOnDone(value);
        public PlaybackSettings WithIgnoreTimeScale(bool value) => this.GetPlaybackSettings().WithIgnoreTimeScale(value);
        #endregion
#if UNITY_EDITOR
        // These values are all used in the SoundEditor
        // Whether values are shared between variants
        [SerializeField] internal bool shareClip = false;
        [SerializeField] internal bool shareVolume = false;
        [SerializeField] internal bool sharePitch = false;

        // Whether the following fields will be shown and used 
        [SerializeField] internal bool useSpatialSettings = false;

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
            if (!useSpatialSettings) spatialSettings = null;

            // Ensure shared values are shared
            SoundVariant firstVariant = weightedVariants[0].Item;
            foreach (var weightedVariant in weightedVariants)
            {
                SoundVariant variant = weightedVariant.Item;

                // Update Shared Fields
                if (shareClip) variant.clip = firstVariant.clip;
                if (shareVolume) variant.volume = firstVariant.volume;
                if (volumeVariationMode == VariationMode.Shared) variant.volumeVariation = firstVariant.volumeVariation;
                if (volumeVariationMode == VariationMode.None) variant.volumeVariation = 0;
                if (sharePitch) variant.pitch = firstVariant.pitch;
                if (pitchVariationMode == VariationMode.Shared) variant.pitchVariation = firstVariant.pitchVariation;
                if (pitchVariationMode == VariationMode.None) variant.pitchVariation = 0;
            }
        }
#endif
    }
}