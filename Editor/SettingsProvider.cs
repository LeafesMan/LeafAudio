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

            Foldout templateFoldout = new Foldout() { text = "Sound Template" };

            cachedSoundEditor = UnityEditor.Editor.CreateEditor(Settings.instance.SoundTemplate);
            VisualElement soundField = cachedSoundEditor.CreateInspectorGUI();
            soundField.Bind(cachedSoundEditor.serializedObject);
            templateFoldout.Add(soundField);

            rootElement.Add(title);
            rootElement.Add(new PropertyField(settings.FindProperty(nameof(Settings.WarnOnPlayNullSound))));
            rootElement.Add(templateFoldout);
            rootElement.Add(new PropertyField(settings.FindProperty(nameof(Settings.SliderVariationColor))));
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
    }
}