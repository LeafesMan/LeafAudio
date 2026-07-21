using System;
using Mono.Cecil;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LeafAudio.Editor
{
    static class LeafAudioSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/LeafAudio", SettingsScope.Project)
            {
                label = "LeafAudio",

                activateHandler = (searchContext, root) =>
                {
                    SerializedObject settings = new SerializedObject(Settings.instance);
                    SerializedProperty soundTemplate = settings.FindProperty(nameof(Settings.SoundDefaults));

                    VisualElement title = new Label("LeafAudio") { style = { fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginLeft = 6 } };

                    Foldout templateFoldout = new Foldout() { text = "Sound Template" };


                    // Setup Hiding for Volume and Pitch Variation on Mode Change
                    var volumeVariationElement = new PropertyField(soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.VolumeVariation)));
                    var volumeVariationModeProp = soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.VolumeVariationMode));
                    ShowIfCondition(volumeVariationElement, () => volumeVariationModeProp.enumValueIndex != (int)Sound.VariationMode.None);


                    var pitchVariationElement = new PropertyField(soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.PitchVariation)));
                    var pitchVariationModeProp = soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.PitchVariationMode));
                    ShowIfCondition(pitchVariationElement, () => pitchVariationModeProp.enumValueIndex != (int)Sound.VariationMode.None);

                    var reverbMixElement = new PropertyField(soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.ReverbMix)));
                    var useReverbMixProp = soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.UseReverbMix));
                    ShowIfCondition(reverbMixElement, () => useReverbMixProp.boolValue);

                    templateFoldout.Add(new PropertyField(soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.AudioMixerGroup))));
                    templateFoldout.Add(new PropertyField(soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.Volume))));
                    templateFoldout.Add(new PropertyField(soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.Pitch))));
                    templateFoldout.Add(new PropertyField(soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.PitchRange))));
                    templateFoldout.Add(volumeVariationElement);
                    templateFoldout.Add(pitchVariationElement);
                    templateFoldout.Add(reverbMixElement);
                    templateFoldout.Add(new PropertyField(volumeVariationModeProp));
                    templateFoldout.Add(new PropertyField(pitchVariationModeProp));
                    templateFoldout.Add(new PropertyField(soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.SelectionMode))));
                    templateFoldout.Add(new PropertyField(soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.ShareClip))));
                    templateFoldout.Add(new PropertyField(soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.ShareVolume))));
                    templateFoldout.Add(new PropertyField(soundTemplate.FindPropertyRelative(nameof(Settings.SoundTemplate.SharePitch))));

                    templateFoldout.Add(new PropertyField(useReverbMixProp));


                    root.Add(title);
                    root.Add(new PropertyField(settings.FindProperty(nameof(Settings.GlobalAudioManagerPoolSize))));
                    root.Add(new Label("Editor") { style = { fontSize = 15, marginTop = 5, marginBottom = 2, marginLeft = 6 } });
                    root.Add(new PropertyField(settings.FindProperty(nameof(Settings.WarnOnPlayNullSound))));
                    root.Add(templateFoldout);
                    root.Add(new PropertyField(settings.FindProperty(nameof(Settings.SliderVariationColor))));
                    root.Bind(settings);

                    title.TrackSerializedObjectValue(settings, s => Settings.instance.SaveSettings());


                    void ShowIfCondition(VisualElement toHide, Func<bool> condition)
                    {
                        UpdateHidden();
                        toHide.TrackSerializedObjectValue(settings, p => UpdateHidden());
                        void UpdateHidden() => toHide.style.display = condition() ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                }
            };
        }

    }
}