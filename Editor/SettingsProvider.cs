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

                    VisualElement title = new Label("LeafAudio") { style = { fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginLeft = 6 } };


                    root.Add(title);
                    root.Add(new PropertyField(settings.FindProperty(nameof(Settings.GlobalAudioManagerPoolSize))));
                    root.Add(new Label("Editor") { style = { fontSize = 15, marginTop = 5, marginBottom = 2, marginLeft = 6 } });
                    root.Add(new PropertyField(settings.FindProperty(nameof(Settings.WarnOnPlayNullSound))));
                    root.Add(new PropertyField(settings.FindProperty(nameof(Settings.SoundDefaults)), "Sound Template"));
                    root.Add(new PropertyField(settings.FindProperty(nameof(Settings.SliderVariationColor))));

                    root.Bind(settings);
                    title.TrackSerializedObjectValue(settings, s => Settings.instance.SaveSettings());
                }
            };
        }
    }
}