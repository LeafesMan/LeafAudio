using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.Collections.Generic;
using LeafRand.Collections;
using System;
namespace LeafAudio.Editor
{
    [CustomEditor(typeof(Sound)), CanEditMultipleObjects]
    public class SoundEditor : UnityEditor.Editor
    {
        ListView variantsListView;
        SerializedProperty variantsProp;
        bool HasMultipleVariants => variantsProp.arraySize > 1;

        public override VisualElement CreateInspectorGUI()
        {
            variantsProp = serializedObject.FindProperty("weightedVariants");

            // Create root container and specs container
            VisualElement root = new VisualElement();
            variantsListView = GetVariantsListView();

            VisualElement selectionModeField = GetLabeledElement(new PropertyField(serializedObject.FindProperty("selectionMode"), ""), "Selection", tooltip: "How a variant will be selected.");
            ShowIfCondition(selectionModeField, () => HasMultipleVariants);


            // Populate Root
            root.Add(GetScriptField());
            root.Add(GetLabeledElement(new PropertyField(serializedObject.FindProperty("mixerGroup"), ""), "Mixer"));
            if (targets.Length > 1) return root; // Multi editing stops here!



            // Variant
            VisualElement firstVariantField = GetFirstVariantField();

            VisualElement testButton = GetTestButton(variantsListView);
            ShowIfCondition(testButton, () => !HasMultipleVariants);

            root.Add(firstVariantField);
            root.Add(GetSpacer());
            root.Add(selectionModeField);
            root.Add(variantsListView);
            root.Add(testButton);
            root.Add(GetSpacer());
            root.Add(GetSettingsFoldout());

            return root;
        }
        VisualElement GetSettingsFoldout()
        {
            Foldout settingsFoldout = new Foldout() { text = "Data Settings", toggleOnLabelClick = true, viewDataKey = "SettingsFoldout" };

            // Setup Set All Modes buttons
            VisualElement setAllButtonContainer = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
            setAllButtonContainer.Add(new Button(() => SetAllModes(Sound.ValueMode.Shared)) { text = "Set All Shared", style = { flexGrow = 1 } });
            setAllButtonContainer.Add(new Button(() => SetAllModes(Sound.ValueMode.Unique)) { text = "Set All Unique", style = { flexGrow = 1, marginRight = 0, paddingRight = 0, borderRightWidth = 0 } });
            ShowIfCondition(setAllButtonContainer, () => HasMultipleVariants);

            // Setup Add Variant Button
            Button addVariantButton = new Button(() => { variantsProp.arraySize++; serializedObject.ApplyModifiedProperties(); });
            addVariantButton.text = "Add Variant";
            ShowIfCondition(addVariantButton, () => !HasMultipleVariants);

            // Mode Fields
            var clipModeField = GetPropField("clipMode", "Clip");
            var volumeModeField = GetPropField("volumeMode", "Volume");
            var volumeVariationModeField = GetPropField("volumeVariationMode", "Volume Variation");
            var volumeVariationModeToggle = GetVariationModeToggle("volumeVariationMode", "Volume Variation");
            var pitchModeField = GetPropField("pitchMode", "Pitch");
            var pitchVariationModeField = GetPropField("pitchVariationMode", "Pitch Variation");
            var pitchVariationModeToggle = GetVariationModeToggle("pitchVariationMode", "Pitch Variation");
            var pitchRangeField = GetPropField("pitchRange", "Pitch Range");

            // Hide Certain Mode fields
            ShowIfCondition(clipModeField, () => HasMultipleVariants);
            ShowIfCondition(volumeModeField, () => HasMultipleVariants);
            ShowIfCondition(volumeVariationModeField, () => HasMultipleVariants);
            ShowIfCondition(pitchModeField, () => HasMultipleVariants);
            ShowIfCondition(pitchVariationModeField, () => HasMultipleVariants);
            ShowIfCondition(volumeVariationModeToggle, () => !HasMultipleVariants);
            ShowIfCondition(pitchVariationModeToggle, () => !HasMultipleVariants);

            // Setup settings foldout
            settingsFoldout.Add(addVariantButton);
            settingsFoldout.Add(setAllButtonContainer);
            settingsFoldout.Add(clipModeField);
            settingsFoldout.Add(volumeModeField);
            settingsFoldout.Add(volumeVariationModeField);
            settingsFoldout.Add(volumeVariationModeToggle);
            settingsFoldout.Add(pitchModeField);
            settingsFoldout.Add(pitchVariationModeField);
            settingsFoldout.Add(pitchVariationModeToggle);
            settingsFoldout.Add(pitchRangeField);

            PropertyField GetPropField(string propName, string label) => new PropertyField(serializedObject.FindProperty(propName), label);

            return settingsFoldout;

            VisualElement GetVariationModeToggle(string propName, string label)
            {
                SerializedProperty prop = serializedObject.FindProperty(propName);

                Toggle modeToggle = new Toggle(label);

                // Set and Update VariationMode and the Toggle
                modeToggle.RegisterValueChangedCallback(b =>
                {
                    prop.enumValueIndex = b.newValue ? (int)Sound.VariationMode.Unique : (int)Sound.VariationMode.None;
                    serializedObject.ApplyModifiedProperties();
                });
                modeToggle.TrackPropertyValue(prop, p => UpdateToggleValue());
                UpdateToggleValue();

                return modeToggle;
                void UpdateToggleValue() => modeToggle.SetValueWithoutNotify(prop.enumValueIndex != (int)Sound.VariationMode.None);
            }
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
            variantsListView.bindItem += BindVariantField;
            variantsListView.makeItem += () => GetVariantField(false);
            variantsListView.BindProperty(variantsProp);

            ShowIfCondition(variantsListView, () => HasMultipleVariants);

            // Add test button to header
            VisualElement listViewHeader = variantsListView.Q<VisualElement>(className: "unity-foldout__input");
            if (listViewHeader == null) throw new Exception("Unity has moved the ListView header element. Use UI Toolkit Debugger to find and assign it again!");
            listViewHeader.Add(GetTestButton(variantsListView, forListView: true));

            return variantsListView;
        }
        #region Make and Bind Variant/Varied/Variation
        VisualElement GetVariantField(bool isSharedField)
        {
            // Container
            var container = new BindableElement(); // Can change this back to foldout if i desire

            // Weight Field
            var weightField = GetLabeledElement(new FloatField("") { bindingPath = "weight" }, "Weight", "weight");
            ShowIfCondition(weightField, () => serializedObject.FindProperty("selectionMode").enumValueIndex == (int)Sound.SelectionMode.WeightedRandom);

            var clipField = GetLabeledElement(new ObjectField("") { bindingPath = "item.clip", name = "clip", objectType = typeof(AudioClip) }, "Clip");
            ShowIfCondition(clipField, () => serializedObject.FindProperty("clipMode").enumValueIndex == (int)Sound.ValueMode.Unique);

            var volumeElements = GetVariedField(new Vector2(0, 1), isSharedField, "volume", "Volume");
            var pitchElements = GetVariedField(serializedObject.FindProperty("pitchRange").vector2Value, isSharedField, "pitch", "Pitch");

            container.Add(clipField);
            container.Add(volumeElements);
            container.Add(pitchElements);
            container.Add(weightField);

            // Add the Element to the given container
            return container;
        }
        void BindVariantField(VisualElement element, int index)
        {
            ((BindableElement)element).BindProperty(variantsProp.GetArrayElementAtIndex(index));

            // Setup previews
            var variantProp = variantsProp.GetArrayElementAtIndex(index).FindPropertyRelative("item");
            var volumeVariedField = element.Q<VisualElement>("volumeVariedField");
            var pitchVariedField = element.Q<VisualElement>("pitchVariedField");
            BindVariationPreview(variantProp, volumeVariedField);
            BindVariationPreview(variantProp, pitchVariedField);
        }
        /// <summary>
        /// Returns a Variant Field that will always be bound to the first element of variantsProp
        /// </summary>
        VisualElement GetFirstVariantField()
        {
            VisualElement firstVariantField = GetVariantField(true);
            BindVariantField(firstVariantField, 0);

            // Ensure this field remains bound to variant 0 in case the original is moved/destroyed
            firstVariantField.TrackPropertyValue(variantsProp, p => BindVariantField(firstVariantField, 0));

            return firstVariantField;
        }
        void BindVariationPreview(SerializedProperty variantProp, VisualElement variedField)
        {           // Prefix is set as the elements userData and thus may be updated to rebind the variation preview
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
        VisualElement GetVariedField(Vector2 valueRange, bool isSharedField, string var = "", string label = "", string variantPath = "")
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
            bool DoShowValue() => !HasMultipleVariants || (isSharedField && GetValueMode() == Sound.ValueMode.Shared) || (!isSharedField && GetValueMode() == Sound.ValueMode.Unique);
            bool DoShowVariation() => GetVariationMode() != Sound.VariationMode.None && (!HasMultipleVariants || (isSharedField && GetVariationMode() == Sound.VariationMode.Shared) || (!isSharedField && GetVariationMode() == Sound.VariationMode.Unique));

            ShowIfCondition(valueField, DoShowValue);
            ShowIfCondition(valueSlider, DoShowValue);
            ShowIfCondition(variationField, DoShowVariation);
            ShowIfCondition(labeledElement, () => DoShowValue() || DoShowVariation());

            return labeledElement;
        }
        #endregion
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
        Button GetTestButton(ListView variantsListView, bool forListView = false)
        {
            Button button = new Button
            {
                text = "Test",
                style = { flexGrow = forListView ? 6 : 0, marginLeft = 0, marginRight = forListView ? 66 : 0 }
            };
            // Disable Test Button if there are no clips asd
            variantsListView.selectedIndicesChanged += (indices) => button.text = "Test" + (forListView && indices.Any() ? " Selected" : "");

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
        BindableElement GetLabeledElement(VisualElement toLabel, string text, string name = "", float labelWidth = 55, string tooltip = "")
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
                tooltip = tooltip,
                style = { flexDirection = FlexDirection.Row, overflow = Overflow.Hidden, paddingRight = 2 }
            };
            labeledElement.Add(label);
            labeledElement.Add(toLabel);

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
        VisualElement GetSpacer() => new VisualElement() { style = { height = 10 } };
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
}