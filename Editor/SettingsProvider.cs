using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LeafAudio.Editor
{
    class LeafAudioSettingsProvider : SettingsProvider
    {
        public LeafAudioSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
            label = "LeafAudio";
        }


        UnityEditor.Editor cachedSoundEditor;
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {

            SerializedObject settings = new SerializedObject(Settings.instance);
            SerializedProperty soundTemplate = settings.FindProperty(nameof(Settings.SoundTemplate));

            VisualElement title = new Label("LeafAudio") { style = { fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginLeft = 6 } };

            // Create Sound Template Editor
            Foldout templateFoldout = new Foldout() { text = "Sound Template" };
            cachedSoundEditor = UnityEditor.Editor.CreateEditor(Settings.instance.SoundTemplate);
            VisualElement soundField = cachedSoundEditor.CreateInspectorGUI();
            templateFoldout.Add(soundField);
            soundField.Bind(cachedSoundEditor.serializedObject);



            // Populate root
            rootElement.Add(title);
            rootElement.Add(GetSpacer());
            rootElement.Add(new PropertyField(settings.FindProperty(nameof(Settings.SliderVariationColor))));
            rootElement.Add(new PropertyField(settings.FindProperty(nameof(Settings.WarnOnPlayNullSound))));
            rootElement.Add(GetSpacer());
            rootElement.Add(templateFoldout);
            rootElement.Bind(settings);


            title.TrackSerializedObjectValue(settings, s => Settings.instance.SaveSettings());


            base.OnActivate(searchContext, rootElement);
        }
        public override void OnDeactivate()
        {
            base.OnDeactivate();
            if (cachedSoundEditor != null)
            {
                UnityEngine.Object.DestroyImmediate(cachedSoundEditor);
                cachedSoundEditor = null;
            }
        }
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new LeafAudioSettingsProvider("Project/LeafAudio", SettingsScope.Project);
        }

        VisualElement GetSpacer()
        {
            return new VisualElement() { style = { marginBottom = 5 } };
        }
    }
}