using System;
using System.Collections.Generic;
using System.Linq;
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


        public override void OnActivate(string searchContext, VisualElement rootElement)
        {

            // Wrap to prevent editor hang on error anywhere inside OnActivate
            try
            {
                SerializedObject settings = new SerializedObject(Settings.instance);

                VisualElement title = new Label("LeafAudio") { style = { fontSize = 20, unityFontStyleAndWeight = FontStyle.Bold, marginLeft = 6 } };

                // Var Setup Spatial Profile Field
                var spatialProfileField = new ObjectField
                {
                    objectType = typeof(SpatialProfile),
                };
                spatialProfileField.SetValueWithoutNotify(PlayerSettings.GetPreloadedAssets().OfType<SpatialProfile>().FirstOrDefault());
                spatialProfileField.RegisterValueChangedCallback(evt =>
                {
                    var newPreloads = PlayerSettings.GetPreloadedAssets().ToList();

                    for (int i = newPreloads.Count - 1; i >= 0; i--) if (newPreloads[i] is SpatialProfile) newPreloads.RemoveAt(i);

                    if (evt.newValue != null) newPreloads.Add(evt.newValue);

                    PlayerSettings.SetPreloadedAssets(newPreloads.ToArray());
                });


                // Populate root
                rootElement.Add(title);
                rootElement.Add(GetSpacer());
                rootElement.Add(spatialProfileField);
                rootElement.Add(new PropertyField(settings.FindProperty(nameof(Settings.SliderVariationColor))));
                rootElement.Add(new PropertyField(settings.FindProperty(nameof(Settings.WarnOnPlayNullSound))));
                rootElement.Add(GetSpacer());
                rootElement.Bind(settings);


                title.TrackSerializedObjectValue(settings, s => Settings.instance.SaveSettings());
            }
            catch { }



            base.OnActivate(searchContext, rootElement);
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