using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Collections.Generic;
using LeafRand.Collections;
using System;
using UnityEngine.Categorization;
using UnityEngine.Audio;

namespace LeafAudio.Editor
{
    /// <summary>
    /// Draws an audio definition. Conceals uneeded vars and allows playing any individual spec or group of specs.
    /// </summary>
    [CustomEditor(typeof(Sound)), CanEditMultipleObjects]
    public class SoundEditor : UnityEditor.Editor
    {
        SerializedProperty variantsProp;
        event Action VariantUIBinded;

        ListView variantsListView;

        public override VisualElement CreateInspectorGUI()
        {
            // Grab prop
            variantsProp = serializedObject.FindProperty("weightedVariants");

            // Create root container and specs container
            VisualElement root = new VisualElement();
            variantsListView = GetVariantsListView();

            // Populate Root
            root.Add(GetScriptField());
            root.Add(GetLabeledElement(new ObjectField("") { bindingPath = serializedObject.FindProperty("mixerGroup").propertyPath, objectType = typeof(AudioMixerGroup) }, "Group"));
            if (targets.Length > 1) return root; // Multi editing stops here!



            // Make Shared Fields
            VisualElement sharedClipField = GetLabeledElement(new ObjectField("") { name = "clip", objectType = typeof(AudioClip) }, "Clip");
            ShowIfCondition(sharedClipField, () => serializedObject.FindProperty("clipType").enumValueIndex == (int)Sound.ValueType.Shared);
            MakeAllMatchingChildrenShared<UnityEngine.Object>(sharedClipField, "clip");

            VisualElement sharedVolumeField = GetVariedField(Vector2.up, true, "volume", "Volume");
            MakeAllMatchingChildrenShared<float>(sharedVolumeField, "volume");
            MakeAllMatchingChildrenShared<float>(sharedVolumeField, "volumeVariation");

            VisualElement sharedPitchField = GetVariedField(new Vector2(-1, 1) * 3, true, "pitch", "Pitch");
            MakeAllMatchingChildrenShared<float>(sharedPitchField, "pitch");
            MakeAllMatchingChildrenShared<float>(sharedPitchField, "pitchVariation");



            root.Add(sharedClipField);
            root.Add(sharedVolumeField);
            root.Add(sharedPitchField);
            root.Add(variantsListView);
            root.Add(GetTestButton(variantsListView));
            root.Add(GetSettingsFoldout());

            return root;
        }
        VisualElement GetSettingsFoldout()
        {
            Foldout settingsFoldout = new Foldout() { text = "Settings", toggleOnLabelClick = true };

            // Setup settings foldout
            settingsFoldout.Add(GetPropField("selectionMode"));
            settingsFoldout.Add(GetPropField("clipType"));
            settingsFoldout.Add(GetPropField("volumeType"));
            settingsFoldout.Add(GetPropField("volumeVariationType"));
            settingsFoldout.Add(GetPropField("pitchType"));
            settingsFoldout.Add(GetPropField("pitchVariationType"));
            PropertyField GetPropField(string propName) => new PropertyField(serializedObject.FindProperty(propName));

            return settingsFoldout;
        }
        void MakeAllMatchingChildrenShared<T>(VisualElement container, string propName) => container.Query<BaseField<T>>(propName).ForEach((field) => MakeFieldShared(field, propName));
        void MakeFieldShared<T>(BaseField<T> field, string propName)
        {
            string propRelative = $"item.{propName}";
            SerializedProperty typeProp = serializedObject.FindProperty(propName + "Type");


            // Ensure always bound to 0th variants
            BindToVariant0();
            variantsListView.itemIndexChanged += (a, b) => BindToVariant0();
            void BindToVariant0() => field.BindProperty(variantsProp.GetArrayElementAtIndex(0).FindPropertyRelative(propRelative));


            // When propType becomes shared
            // - Update all prop values to shared value
            field.TrackPropertyValue(typeProp, (typeProp) => UpdatePropertiesToSharedFieldValue(propName, field.value));

            // Ensure all fields are updated to this fields value when Shared is on for this value 
            field.RegisterValueChangedCallback((evt) => UpdatePropertiesToSharedFieldValue(propName, evt.newValue));
            void UpdatePropertiesToSharedFieldValue(string pathRelativeToVariant, T newFieldValue)
            {
                if (typeProp.enumValueIndex != (int)Sound.ValueType.Shared) return; // If not shared dont distribute values

                for (int i = 0; i < variantsProp.arraySize; i++)
                {
                    var variantProp = variantsProp.GetArrayElementAtIndex(i).FindPropertyRelative($"item.{pathRelativeToVariant}");

                    // Account for float/audioclip
                    object newValObj = newFieldValue;
                    if (typeof(T) == typeof(float)) variantProp.floatValue = (float)newValObj;
                    if (typeof(T) == typeof(UnityEngine.Object)) variantProp.boxedValue = newValObj;
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
        bool IsAnyUnique()
        {
            bool IsUnique(string propName) => serializedObject.FindProperty(propName).enumValueIndex == (int)Sound.ValueType.Unique;
            return IsUnique("clipType") || IsUnique("volumeType") || IsUnique("volumeVariationType") || IsUnique("pitchType") || IsUnique("pitchVariationType");
        }
        ListView GetVariantsListView()
        {
            ListView variantsListView = new ListView()
            {
                showBorder = true,
                showAddRemoveFooter = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderMode = ListViewReorderMode.Animated,
                reorderable = true,
                headerTitle = "Variants"
            };
            variantsListView.bindItem += BindVariantUI;
            variantsListView.makeItem += MakeVariantUI;
            variantsListView.BindProperty(variantsProp);

            ShowIfCondition(variantsListView, IsAnyUnique);

            return variantsListView;

            void BindVariantUI(VisualElement element, int index) { ((BindableElement)element).BindProperty(variantsProp.GetArrayElementAtIndex(index)); VariantUIBinded?.Invoke(); }
            VisualElement MakeVariantUI()
            {
                // Container
                var container = new BindableElement(); // Can change this back to foldout if i desire
                container.style.paddingLeft = new StyleLength(15); // Add padding
                container.style.paddingRight = new StyleLength(5); // Add padding
                container.style.marginBottom = new StyleLength(5); // Add padding
                container.style.borderBottomLeftRadius = 10;
                container.style.borderBottomRightRadius = 10;
                container.style.borderTopLeftRadius = 10;
                container.style.borderTopRightRadius = 10;

                // Weight Field
                var weightField = GetLabeledElement(new FloatField("") { bindingPath = "weight" }, "Weight", "weight");
                ShowIfCondition(weightField, () => serializedObject.FindProperty("selectionMode").enumValueIndex == (int)Sound.SelectionMode.WeightedRandom);

                var clipField = GetLabeledElement(new ObjectField("") { bindingPath = "item.clip", name = "clip", objectType = typeof(AudioClip) }, "Clip");
                ShowIfCondition(clipField, () => serializedObject.FindProperty("clipType").enumValueIndex == (int)Sound.ValueType.Unique);

                var volumeElements = GetVariedField(new Vector2(0, 1), false, "volume", "Volume");
                var pitchElements = GetVariedField(new Vector2(-3, 3), false, "pitch", "Pitch");



                container.Add(clipField);
                container.Add(volumeElements);
                container.Add(pitchElements);
                container.Add(weightField);


                // Add the Element to the given container
                return container;
            }
        }
        VisualElement GetVariedField(Vector2 valueRange, bool showWhenShared, string var = "", string label = "")
        {
            VisualElement fieldsElement = new VisualElement() { style = { flexDirection = FlexDirection.Row } };

            ClampedFloatField valueField = new ClampedFloatField(valueRange) { bindingPath = $"item.{var}", name = var, style = { flexGrow = 1, maxWidth = 50, flexBasis = 30 } };
            Slider valueSlider = new Slider(valueRange.x, valueRange.y) { bindingPath = $"item.{var}", name = var, style = { flexGrow = 1 } };
            VisualElement variationField = new ClampedFloatField(new Vector2(0, Mathf.Infinity), "+/-") { bindingPath = $"item.{var}Variation", name = $"{var}Variation", style = { flexGrow = 1, maxWidth = 65, flexBasis = 45 } };

            var variationLabel = variationField.Q<Label>();
            variationLabel.style.flexGrow = 0;
            variationLabel.style.flexShrink = 0;
            variationLabel.style.minWidth = 22;
            var variationText = variationField.Q<VisualElement>(className: "unity-base-field__input");
            variationText.style.minWidth = 0;
            variationText.style.flexGrow = 1f;

            // Add the fields
            fieldsElement.Add(valueField);
            fieldsElement.Add(valueSlider);
            fieldsElement.Add(variationField);

            var labeledElement = GetLabeledElement(fieldsElement, label);


            // Toggle elements
            Sound.ValueType GetValueType() => (Sound.ValueType)serializedObject.FindProperty($"{var}Type").enumValueIndex;
            Sound.VariationType GetVariationType() => (Sound.VariationType)serializedObject.FindProperty($"{var}VariationType").enumValueIndex;
            bool DoShowValue() => (showWhenShared && GetValueType() == Sound.ValueType.Shared) || (!showWhenShared && GetValueType() == Sound.ValueType.Unique);
            bool DoShowVariation() => (showWhenShared && GetVariationType() == Sound.VariationType.Shared) || (!showWhenShared && GetVariationType() == Sound.VariationType.Unique);

            ShowIfCondition(valueField, DoShowValue);
            ShowIfCondition(valueSlider, DoShowValue);
            ShowIfCondition(variationField, DoShowVariation);
            ShowIfCondition(labeledElement, () => DoShowValue() || DoShowVariation());

            return labeledElement;
        }

        /// <summary>
        /// Tracks the serialized object and shows whenever the condition is met
        /// </summary>
        void ShowIfCondition(VisualElement element, Func<bool> condition)
        {
            UpdateShown();
            element.TrackSerializedObjectValue(serializedObject, (obj) => UpdateShown());
            void UpdateShown() => element.style.display = condition() ? DisplayStyle.Flex : DisplayStyle.None;
        }

        VisualElement GetScriptField()
        {
            var scriptField = new ObjectField("Script")
            {
                objectType = typeof(MonoScript),
                value = MonoScript.FromScriptableObject((ScriptableObject)target)
            };

            scriptField.SetEnabled(false);
            return scriptField;
        }
        Button GetTestButton(ListView variantsListView)
        {
            Button button = new Button
            {
                text = "Test",
                style = { height = 20, marginTop = 5 }
            };
            // Disable Test Button if there are no clips asd
            variantsListView.selectedIndicesChanged += (indices) => button.text = "Test" + (indices.Any() ? " Selected" : "");

            button.RegisterCallback<ClickEvent>(
                (evt) =>
                {
                    Sound audio = target as Sound;
                    SoundVariant variant;

                    List<int> selectedIndices = new List<int>();
                    foreach (var index in variantsListView.selectedIndices) selectedIndices.Add(index);

                    // No Variants Selected? Use Sounds Variants+SelectionMode
                    if (selectedIndices.Count == 0) variant = audio.SelectVariant();
                    else
                    {   // Variants Selected? Use Selected Variants+SoundsSelectionMode
                        // Make a list of only selected variants then use Sound.SelectVariant on them
                        List<Weighted<SoundVariant>> selectedWeightedSoundVariants = new();
                        for (int i = 0; i < selectedIndices.Count; i++) selectedWeightedSoundVariants.Add((Weighted<SoundVariant>)variantsProp.GetArrayElementAtIndex(selectedIndices[i]).boxedValue);

                        variant = Sound.SelectVariant(selectedWeightedSoundVariants, (Sound.SelectionMode)serializedObject.FindProperty("selectionMode").enumValueIndex);
                    }


                    SoundTester.Test(variant.GetClip(), variant.GetVolume(), variant.GetPitch());
                }
            );

            return button;
        }
        BindableElement GetLabeledElement(VisualElement toLabel, string text, string name = "", float labelWidth = 55)
        {   // Create and Style Label
            Label label = new(text) { style = { width = labelWidth, unityTextAlign = TextAnchor.MiddleLeft } };

            // Style element to Label
            toLabel.style.flexGrow = 1;
            //toLabel.style.paddingRight = 4;
            toLabel.style.overflow = Overflow.Visible;

            // Create, Populate, and Return labeled Element
            BindableElement labeledElement = new BindableElement()
            {
                name = name,
                style = { flexDirection = FlexDirection.Row, overflow = Overflow.Hidden, paddingRight = 2 }
            };
            labeledElement.Add(label);
            labeledElement.Add(toLabel);

            return labeledElement;
        }
        string GetCapitalized(string str) => char.ToUpper(str[0]) + str[1..];
    }
}

class ClampedFloatField : FloatField
{
    public Vector2 ClampRange;
    public ClampedFloatField(Vector2 clampRange, string label = "") { this.ClampRange = clampRange; this.label = label; }

    public override float value { get => base.value; set => base.value = Mathf.Clamp(value, ClampRange.x, ClampRange.y); }
}