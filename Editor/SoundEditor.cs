using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Collections.Generic;
using LeafRand.Collections;
using System;
using UnityEngine.Audio;
using Codice.CM.Common.Merge;
using System.Runtime.InteropServices;

namespace LeafAudio.Editor
{
    [CustomEditor(typeof(Sound)), CanEditMultipleObjects]
    public class SoundEditor : UnityEditor.Editor
    {
        SerializedProperty variantsProp;
        ListView variantsListView;

        public override VisualElement CreateInspectorGUI()
        {
            variantsProp = serializedObject.FindProperty("weightedVariants");

            // Create root container and specs container
            VisualElement root = new VisualElement();
            SetupNoVariantProtection(root);
            variantsListView = GetVariantsListView();

            // Populate Root
            root.Add(GetScriptField());
            root.Add(GetLabeledElement(new ObjectField("") { bindingPath = serializedObject.FindProperty("mixerGroup").propertyPath, objectType = typeof(AudioMixerGroup) }, "Group"));
            if (targets.Length > 1) return root; // Multi editing stops here!



            // Make Shared Fields
            VisualElement sharedClipField = GetLabeledElement(new ObjectField("") { name = "clip", objectType = typeof(AudioClip) }, "Clip");
            ShowIfCondition(sharedClipField, () => serializedObject.FindProperty("clipMode").enumValueIndex == (int)Sound.ValueMode.Shared);
            MakeAllMatchingChildrenShared<UnityEngine.Object>(sharedClipField, "clip");

            VisualElement sharedVolumeField = GetVariedField(Vector2.up, true, "volume", "Volume");
            MakeAllMatchingChildrenShared<float>(sharedVolumeField, "volume");
            MakeAllMatchingChildrenShared<float>(sharedVolumeField, "volumeVariation");

            VisualElement sharedPitchField = GetVariedField(new Vector2(-1, 1) * 3, true, "pitch", "Pitch");
            MakeAllMatchingChildrenShared<float>(sharedPitchField, "pitch");
            MakeAllMatchingChildrenShared<float>(sharedPitchField, "pitchVariation");

            VisualElement testButton = GetTestButton(variantsListView);
            ShowIfCondition(testButton, () => !IsAnyUnique());

            root.Add(GetSpacer());
            root.Add(variantsListView);
            root.Add(testButton);
            root.Add(sharedClipField);
            root.Add(sharedVolumeField);
            root.Add(sharedPitchField);
            root.Add(GetSpacer());
            root.Add(GetSettingsFoldout());

            return root;
        }
        VisualElement GetSpacer() => new VisualElement() { style = { height = 10 } };
        void SetupNoVariantProtection(VisualElement root)
        {   // Protect once on starting the editor then whenever the serialized object changes
            ProtectAgainstNoVariants();
            root.TrackSerializedObjectValue(serializedObject, (obj) => ProtectAgainstNoVariants());
            void ProtectAgainstNoVariants()
            {
                if (variantsProp.arraySize == 0)
                {
                    variantsProp.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                    Debug.LogError($"{target.name} has no SoundVariants! One has been added. Did you remove the last variant via reflection or debug mode?");
                }
            }
        }
        VisualElement GetSettingsFoldout()
        {
            Foldout settingsFoldout = new Foldout() { text = "Settings", toggleOnLabelClick = true, viewDataKey = "SoundSettingsFoldout" };

            // Setup Set All Modes buttons
            VisualElement setAllButtonContainer = new VisualElement() { style = { flexDirection = FlexDirection.Row, alignSelf = Align.Stretch, flexGrow = 1, paddingLeft = 0, paddingRight = 0, marginRight = 0, marginLeft = 0 } };
            setAllButtonContainer.Add(new Button(() => SetAllModes(Sound.ValueMode.Shared)) { text = "Set All Shared", style = { flexGrow = 1, marginLeft = 0, paddingLeft = 0, borderLeftWidth = 0 } });
            setAllButtonContainer.Add(new Button(() => SetAllModes(Sound.ValueMode.Unique)) { text = "Set All Unique", style = { flexGrow = 1, marginRight = 0, paddingRight = 0, borderRightWidth = 0 } });

            // Setup settings foldout
            settingsFoldout.Add(setAllButtonContainer);
            settingsFoldout.Add(GetPropField("selectionMode", "Selection"));
            settingsFoldout.Add(GetPropField("clipMode", "Clip"));
            settingsFoldout.Add(GetPropField("volumeMode", "Volume"));
            settingsFoldout.Add(GetPropField("volumeVariationMode", "Volume Variation"));
            settingsFoldout.Add(GetPropField("pitchMode", "Pitch"));
            settingsFoldout.Add(GetPropField("pitchVariationMode", "Pitch Variation"));
            settingsFoldout.Add(GetPropField("pitchRange", "Pitch Range"));
            PropertyField GetPropField(string propName, string label) => new PropertyField(serializedObject.FindProperty(propName), label);

            return settingsFoldout;

            void SetAllModes(Sound.ValueMode newMode)
            {
                serializedObject.FindProperty("clipMode").enumValueIndex = (int)newMode;
                serializedObject.FindProperty("volumeMode").enumValueIndex = (int)newMode;
                serializedObject.FindProperty("volumeVariationMode").enumValueIndex = (int)newMode;
                serializedObject.FindProperty("pitchMode").enumValueIndex = (int)newMode;
                serializedObject.FindProperty("pitchVariationMode").enumValueIndex = (int)newMode;
                serializedObject.ApplyModifiedProperties();
            }
        }
        void MakeAllMatchingChildrenShared<T>(VisualElement container, string propName) => container.Query<BaseField<T>>(propName).ForEach((field) => MakeFieldShared(field, propName));
        void MakeFieldShared<T>(BaseField<T> field, string propName)
        {
            SerializedProperty typeProp = serializedObject.FindProperty(propName + "Mode");


            // Ensure always bound to 0th variants
            BindToVariant0();
            variantsListView.itemIndexChanged += (a, b) => BindToVariant0();
            void BindToVariant0()
            {
                SerializedProperty variantProp = variantsProp.GetArrayElementAtIndex(0).FindPropertyRelative("item");
                field.BindProperty(variantProp.FindPropertyRelative(propName));

                if (typeof(T) == typeof(float)) BindVariationPreview(variantProp, field.parent);
            }

            // When propMode becomes shared
            // - Update all prop values to shared value
            field.TrackPropertyValue(typeProp, (typeProp) => UpdatePropertiesToSharedFieldValue(propName, field.value));

            // Ensure all fields are updated to this fields value when Shared is on for this value 
            field.RegisterValueChangedCallback((evt) => UpdatePropertiesToSharedFieldValue(propName, evt.newValue));
            void UpdatePropertiesToSharedFieldValue(string pathRelativeToVariant, T newFieldValue)
            {
                if (typeProp.enumValueIndex == (int)Sound.ValueMode.Unique) return; // If unique dont distribute values
                if (typeProp.enumValueIndex == (int)Sound.VariationMode.None) newFieldValue = default;

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
            bool IsUnique(string propName) => serializedObject.FindProperty(propName).enumValueIndex == (int)Sound.ValueMode.Unique;
            return IsUnique("clipMode") || IsUnique("volumeMode") || IsUnique("volumeVariationMode") || IsUnique("pitchMode") || IsUnique("pitchVariationMode");
        }
        ListView GetVariantsListView()
        {
            ListView variantsListView = new ListView()
            {
                showBorder = true,
                showAddRemoveFooter = true,
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderMode = ListViewReorderMode.Animated,
                reorderable = true,
                headerTitle = "Variants"
            };
            variantsListView.bindItem += BindVariantUI;
            variantsListView.makeItem += MakeVariantUI;
            variantsListView.BindProperty(variantsProp);

            // Only allow remove when > 1 element
            UpdateAllowRemove();
            variantsListView.TrackPropertyValue(variantsProp, (p) => UpdateAllowRemove());
            void UpdateAllowRemove() => variantsListView.allowRemove = variantsProp.arraySize > 1;

            ShowIfCondition(variantsListView, IsAnyUnique);

            // Add test button to header
            VisualElement listViewHeader = variantsListView.Q<VisualElement>(className: "unity-foldout__input");
            if (listViewHeader == null) throw new Exception("Unity has moved the ListView header element. Use UI Toolkit Debugger to find and assign it again!");
            listViewHeader.Add(GetTestButton(variantsListView, 6, 66));


            return variantsListView;

            void BindVariantUI(VisualElement element, int index)
            {
                ((BindableElement)element).BindProperty(variantsProp.GetArrayElementAtIndex(index));

                // Setup previews
                var variantProp = variantsProp.GetArrayElementAtIndex(index).FindPropertyRelative("item");
                var volumeVariedField = element.Q<VisualElement>("volumeVariedField");
                var pitchVariedField = element.Q<VisualElement>("pitchVariedField");
                BindVariationPreview(variantProp, volumeVariedField);
                BindVariationPreview(variantProp, pitchVariedField);
            }




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
                ShowIfCondition(clipField, () => serializedObject.FindProperty("clipMode").enumValueIndex == (int)Sound.ValueMode.Unique);

                var volumeElements = GetVariedField(new Vector2(0, 1), false, "volume", "Volume");
                var pitchElements = GetVariedField(serializedObject.FindProperty("pitchRange").vector2Value, false, "pitch", "Pitch");

                container.Add(clipField);
                container.Add(volumeElements);
                container.Add(pitchElements);
                container.Add(weightField);

                // Add the Element to the given container
                return container;
            }
        }



        // Prefix is set as the elements userData and thus may be updated to rebind the variation preview
        void BindVariationPreview(SerializedProperty variantProp, VisualElement variedField)
        {
            VariedFieldInfo newVariedFieldInfo = (VariedFieldInfo)variedField.userData;
            newVariedFieldInfo.variantPropPath = variantProp.propertyPath + ".";
            variedField.userData = newVariedFieldInfo;
            string var = newVariedFieldInfo.var;

            var valSliderPreview = variedField.Q<VisualElement>(var + "SliderPreview");

            var variationProp = variantProp.FindPropertyRelative(var + "Variation");
            var valueProp = variantProp.FindPropertyRelative(var);
            valSliderPreview.Unbind(); // Unbinds previous TrackPropertyValue calls



            valSliderPreview.TrackPropertyValue(valueProp, p => UpdateVariationPreview(variedField));
            valSliderPreview.TrackPropertyValue(variationProp, p => UpdateVariationPreview(variedField));
        }
        void UpdateVariationPreview(VisualElement variedElement)
        {
            // Grab info from the field
            VariedFieldInfo variedFieldInfo = (VariedFieldInfo)variedElement.userData;
            string variantPropPath = variedFieldInfo.variantPropPath;
            string var = variedFieldInfo.var;
            Vector2 range = variedFieldInfo.range;

            // Grab required Elements
            VisualElement valSlider = variedElement.Q<Slider>(var);
            VisualElement valSliderPreview = variedElement.Q<VisualElement>(var + "SliderPreview");
            VisualElement dragger = valSlider.Q(className: "unity-base-slider__dragger");

            //  Calculate widths
            float rangeWidth = range.y - range.x;
            float draggerWidth = dragger.resolvedStyle.width;
            float trackWidth = valSlider.resolvedStyle.width;
            float usableWidth = trackWidth - draggerWidth;
            float leftOffset = draggerWidth * 0.5f;

            // Determine neccesary dragger width and position of a percent of usable width
            //                                                     Account for dragger unused width
            float widthPercent = 2 * serializedObject.FindProperty(variantPropPath + var + "Variation").floatValue / rangeWidth + draggerWidth / trackWidth;
            float posPercent = (serializedObject.FindProperty(variantPropPath + var).floatValue - range.x) / rangeWidth - widthPercent * 0.5f;

            // Apply width and position
            valSliderPreview.style.left = leftOffset + usableWidth * posPercent;
            valSliderPreview.style.width = usableWidth * widthPercent;


        }

        struct VariedFieldInfo
        {
            public string variantPropPath;
            public string var;
            public Vector2 range;

            public VariedFieldInfo(string variantPropPath, string var, Vector2 range)
            {
                this.variantPropPath = variantPropPath;
                this.var = var;
                this.range = range;
            }
        }
        VisualElement GetVariedField(Vector2 valueRange, bool showWhenShared, string var = "", string label = "", string variantPath = "")
        {
            VisualElement fieldsElement = new VisualElement() { name = var + "VariedField", style = { flexDirection = FlexDirection.Row } };
            fieldsElement.AddToClassList("variedField");
            fieldsElement.userData = new VariedFieldInfo(variantPath, var, valueRange);


            // Setup Value and Variation Fields
            ClampedFloatField valueField = new ClampedFloatField(valueRange) { bindingPath = $"item.{var}", name = var, style = { flexGrow = 1, maxWidth = 50, flexBasis = 30 } };
            ClampedFloatField variationField = new ClampedFloatField(new Vector2(0, Mathf.Infinity), "+/-") { bindingPath = $"item.{var}Variation", name = $"{var}Variation", style = { flexGrow = 1, maxWidth = 65, flexBasis = 45 } };
            var variationLabel = variationField.Q<Label>();
            variationLabel.style.flexGrow = 0;
            variationLabel.style.flexShrink = 0;
            variationLabel.style.minWidth = 22;
            var variationText = variationField.Q<VisualElement>(className: "unity-base-field__input");
            variationText.style.minWidth = 0;
            variationText.style.flexGrow = 1f;

            // Setup Value slider + Variation preview
            Slider valueSlider = new Slider(valueRange.x, valueRange.y) { bindingPath = $"item.{var}", name = var, style = { marginLeft = 10, flexGrow = 1 } };
            var sliderPreview = new VisualElement() { name = $"{var}SliderPreview", pickingMode = PickingMode.Ignore, style = { backgroundColor = Settings.instance.SliderVariationColor, position = Position.Absolute, height = 2, width = 1 } };
            var sliderBkg = valueSlider.Q<VisualElement>("unity-tracker");
            sliderBkg.style.overflow = Overflow.Hidden; // Hide preview when off bkg
            valueSlider.RegisterCallback<GeometryChangedEvent>(evt => UpdateVariationPreview(fieldsElement)); // Update Preview when size changes
            if (var == "pitch") fieldsElement.TrackPropertyValue(serializedObject.FindProperty("pitchRange"), pitchRange =>
            {   // Update the slider range, field range, and the UserData range(for the preview)
                valueSlider.lowValue = pitchRange.vector2Value.x;
                valueSlider.highValue = pitchRange.vector2Value.y;

                valueField.ClampRange = pitchRange.vector2Value;

                var newData = (VariedFieldInfo)fieldsElement.userData;
                newData.range = pitchRange.vector2Value;
                fieldsElement.userData = newData;

                UpdateVariationPreview(fieldsElement);
            }); // Update Ranges when range changes

            // Add the fields
            sliderBkg.Add(sliderPreview);
            fieldsElement.Add(valueField);
            fieldsElement.Add(valueSlider);
            fieldsElement.Add(variationField);

            var labeledElement = GetLabeledElement(fieldsElement, label);


            // Toggle elements
            Sound.ValueMode GetValueMode() => (Sound.ValueMode)serializedObject.FindProperty($"{var}Mode").enumValueIndex;
            Sound.VariationMode GetVariationMode() => (Sound.VariationMode)serializedObject.FindProperty($"{var}VariationMode").enumValueIndex;
            bool DoShowValue() => (showWhenShared && GetValueMode() == Sound.ValueMode.Shared) || (!showWhenShared && GetValueMode() == Sound.ValueMode.Unique);
            bool DoShowVariation() => (showWhenShared && GetVariationMode() == Sound.VariationMode.Shared) || (!showWhenShared && GetVariationMode() == Sound.VariationMode.Unique);

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
        Button GetTestButton(ListView variantsListView, float flexGrow = 1, float marginRight = 0)
        {
            Button button = new Button
            {
                text = "Test",
                style = { flexGrow = flexGrow, marginLeft = 0, marginRight = marginRight }
            };
            // Disable Test Button if there are no clips asd
            variantsListView.selectedIndicesChanged += (indices) => button.text = "Test" + (indices.Any() ? " Selected" : "");

            button.RegisterCallback<ClickEvent>(
                (evt) =>
                {
                    Sound sound = target as Sound;
                    PlaybackSettings playbackSettings;

                    List<int> selectedIndices = new List<int>();
                    foreach (var index in variantsListView.selectedIndices) selectedIndices.Add(index);

                    // No Variants Selected? Use Sounds Variants+SelectionMode
                    if (selectedIndices.Count == 0) playbackSettings = sound.GetPlaybackSettings();
                    else
                    {   // Variants Selected? Use Selected Variants+SoundsSelectionMode
                        // Make a list of only selected variants then use Sound.SelectVariant on them
                        List<Weighted<SoundVariant>> selectedWeightedSoundVariants = new();
                        for (int i = 0; i < selectedIndices.Count; i++) selectedWeightedSoundVariants.Add((Weighted<SoundVariant>)variantsProp.GetArrayElementAtIndex(selectedIndices[i]).boxedValue);

                        playbackSettings = sound.GetPlaybackSettingsFromVariants(selectedWeightedSoundVariants);
                    }


                    SoundTester.Test(playbackSettings);
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

    public override float value
    {
        get => base.value;
        set { if (ClampRange.x <= ClampRange.y) base.value = Mathf.Clamp(value, ClampRange.x, ClampRange.y); }
    }
}