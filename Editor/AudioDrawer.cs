using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

namespace LeafAudio.Editor
{
    /// <summary>
    /// Draws an audio definition. Conceals uneeded vars and allows playing any individual spec or group of specs.
    /// </summary>
    [CustomPropertyDrawer(typeof(Audio))]
    public class AudioDrawer : PropertyDrawer
    {
        class TrackedInt
        {
            public TrackedInt(int val) => this.val = val;
            int val;
            public int Value { get => val; set { val = value; Changed?.Invoke(val); } }
            public event Action<int> Changed;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty audioProp)
        {

            Debug.Log($"CreatePropertyGUI called for: {audioProp.propertyPath}");


            SerializedProperty specsProp = audioProp.FindPropertyRelative("audioSpecs");

            // Cant have field at class level property drawers share those
            TrackedInt selectedIndex = new(-1);
            TrackedInt knownSpecCount = new(0);



            // Create root container
            VisualElement root = new Foldout()
            {
                text = audioProp.displayName,
                style =
            {
                paddingRight = 5,
                borderBottomLeftRadius = 5,
                borderBottomRightRadius = 5,
                borderTopLeftRadius = 5,
                borderTopRightRadius = 5,
                backgroundColor = new Color(0.25f, 0.25f, 0.25f)
            }
            };
            if (audioProp.serializedObject.targetObject is AudioAsset) // Do not encase in foldout if in ShakeSpecsAsset
                root = new VisualElement();



            VisualElement specsContainer = GetSpecsContainer(audioProp, specsProp, knownSpecCount, selectedIndex);

            // Populate Root
            root.Add(GetHeader("Main Settings"));
            root.Add(GetLabeledElement(new PropertyField(audioProp.FindPropertyRelative("mixerGroup"), ""), "Mixer Group"));
            root.Add(GetWeightedToggle(specsContainer, audioProp));
            root.Add(GetHeader("Audio Specs"));
            root.Add(specsContainer);
            root.Add(GetTestButton(audioProp, selectedIndex));
            root.Add(GetAddButton(specsProp));
            root.Add(GetRemoveButton(specsProp, selectedIndex));

            return root;
        }
        void RebuildSpecsContainer(VisualElement specsContainer, SerializedProperty audioProp, SerializedProperty specsProp, TrackedInt knownSpecCount, TrackedInt selectedIndex)
        {
            specsContainer.Clear();

            for (int i = 0; i < specsProp.arraySize; i++)
                InsertNewAudioSpecUI(specsContainer, audioProp, i, selectedIndex);

            knownSpecCount.Value = specsProp.arraySize;
        }
        VisualElement GetSpecsContainer(SerializedProperty audioProp, SerializedProperty specsProp, TrackedInt knownSpecCount, TrackedInt selectedIndex)
        {
            // Generate Specs Container
            VisualElement specsContainer = new VisualElement();
            RebuildSpecsContainer(specsContainer, audioProp, specsProp, knownSpecCount, selectedIndex);


            // Add/Remove UI from Specs Container on UNDO/REDO
            specsContainer.TrackPropertyValue(specsProp, (p) => { if (specsProp.arraySize != knownSpecCount.Value) RebuildSpecsContainer(specsContainer, audioProp, specsProp, knownSpecCount, selectedIndex); });

            return specsContainer;
        }
        void UpdateWeightFieldDisplays(VisualElement specsContainer, SerializedProperty prop)
        {
            DisplayStyle style = GetDisplayStyle(prop.FindPropertyRelative("useWeights").boolValue);
            foreach (VisualElement element in specsContainer.Children()) element[4].style.display = style;
        }
        void InsertNewAudioSpecUI(VisualElement root, SerializedProperty audioProp, int index, TrackedInt selectedIndex)
        {
            float labelWidth = 69;

            SerializedProperty weightedAudioSpecProp = audioProp.FindPropertyRelative("audioSpecs").GetArrayElementAtIndex(index);
            SerializedProperty audioSpecProp = weightedAudioSpecProp.FindPropertyRelative("item");
            SerializedProperty weightProp = weightedAudioSpecProp.FindPropertyRelative("weight");
            SerializedProperty useWeightsProp = audioProp.FindPropertyRelative("useWeights");

            // FOLDOUT
            var foldout = new VisualElement(); // Can change this back to foldout if i desire
            foldout.style.paddingLeft = new StyleLength(15); // Add padding
            foldout.style.paddingRight = new StyleLength(5); // Add padding
            foldout.style.marginBottom = new StyleLength(5); // Add padding
            foldout.style.borderBottomLeftRadius = 10;
            foldout.style.borderBottomRightRadius = 10;
            foldout.style.borderTopLeftRadius = 10;
            foldout.style.borderTopRightRadius = 10;
            foldout.RegisterCallback<MouseDownEvent>(evt =>
            {   // Deselect on clicking same item otherwise select item

                int myIndex = root.IndexOf(foldout);

                if (myIndex == selectedIndex.Value) selectedIndex.Value = -1;
                else selectedIndex.Value = myIndex;


                // Have to find the element ourselves because unity occasionally performs a deep copy of all VisualElements rebuilding the tree
                // so if I hold a reference to a visual element in an event, and throw that event, the event will act on the visual element in the old tree
                var rootArray = root.Children().ToArray();
                for (int i = 0; i < root.childCount; i++)
                {
                    if (selectedIndex.Value == i) rootArray[i].style.backgroundColor = new Color(.17f, .32f, .56f);
                    else rootArray[i].style.backgroundColor = new Color(0.27f, 0.27f, 0.27f);
                }
            });
            foldout.style.backgroundColor = new Color(0.27f, 0.27f, 0.27f);


            // Weight Field
            var weightField = new PropertyField(weightProp, "");
            weightField.BindProperty(weightProp);
            var weightLabeled = GetLabeledElement(weightField, "Weight", labelWidth);
            weightLabeled.style.display = GetDisplayStyle(useWeightsProp.boolValue);

            var clipProp = audioSpecProp.FindPropertyRelative("clip");
            var clipField = new PropertyField(clipProp, "");
            clipField.BindProperty(clipProp);

            var title = new Label() { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 15, marginTop = 3f, overflow = Overflow.Hidden } };
            clipField.RegisterValueChangeCallback((val) => title.text = clipProp.objectReferenceValue == null ? "No Clip" : clipProp.objectReferenceValue.name);



            var volumeElements = GetVariedField(audioSpecProp, "volume", new Vector2(0, 1), 0.5f, labelWidth);
            var pitchElements = GetVariedField(audioSpecProp, "pitch", new Vector2(-3, 3), 1f, labelWidth);



            foldout.Add(title);
            foldout.Add(GetLabeledElement(clipField, "Clip", labelWidth));
            foldout.Add(volumeElements);
            foldout.Add(pitchElements);
            foldout.Add(weightLabeled);



            // Add the Element to the given container
            root.Insert(index, foldout);
        }

        VisualElement GetHeader(string headerText)
        {
            // Create a root container for the header
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.justifyContent = Justify.Center;
            container.style.alignItems = Align.Center;
            container.style.marginTop = 15;
            container.style.marginBottom = 5;

            // Create the left line
            var leftLine = new VisualElement();
            leftLine.style.flexGrow = 1;
            leftLine.style.height = 1;
            leftLine.style.backgroundColor = new StyleColor(Color.gray);
            leftLine.style.marginRight = 5;
            container.Add(leftLine);

            // Create the header label with bold, centered text
            var headerLabel = new Label(headerText);
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            container.Add(headerLabel);

            // Create the right line
            var rightLine = new VisualElement();
            rightLine.style.flexGrow = 1;
            rightLine.style.height = 1;
            rightLine.style.backgroundColor = new StyleColor(Color.gray);
            rightLine.style.marginLeft = 5;
            container.Add(rightLine);

            return container;
        }
        DisplayStyle GetDisplayStyle(bool show) => show ? DisplayStyle.Flex : DisplayStyle.None;
        VisualElement GetWeightedToggle(VisualElement specsContainer, SerializedProperty audio)
        {
            PropertyField weightToggle = new PropertyField(audio.FindPropertyRelative("useWeights"), "");
            weightToggle.RegisterValueChangeCallback((evt) => UpdateWeightFieldDisplays(specsContainer, audio));
            return GetLabeledElement(weightToggle, "Weighted");
        }
        Button GetAddButton(SerializedProperty audioSpecs)
        {
            Button addButton = new Button(() =>
            {
                audioSpecs.arraySize++;

                var newAudioSpec = audioSpecs.GetArrayElementAtIndex(audioSpecs.arraySize - 1);
                newAudioSpec.serializedObject.ApplyModifiedProperties();
                audioSpecs.serializedObject.ApplyModifiedProperties();
            })
            { text = "Add" };

            return addButton;
        }
        Button GetRemoveButton(SerializedProperty audioSpecsProp, TrackedInt selectedIndex)
        {
            var removeButton = new Button(() =>
            {
                // Remove shake component and corresponding bool
                audioSpecsProp.DeleteArrayElementAtIndex(selectedIndex.Value);
                audioSpecsProp.serializedObject.ApplyModifiedProperties();

                selectedIndex.Value = -1;
            })
            { text = "Remove" };
            removeButton.SetEnabled(false);
            selectedIndex.Changed += (val) => { Debug.Log("Selected index changed now: " + selectedIndex + " and prop path: " + audioSpecsProp.propertyPath); removeButton.SetEnabled(val != -1); };

            return removeButton;
        }
        Button GetTestButton(SerializedProperty property, TrackedInt selectedIndex)
        {
            SerializedProperty audioSpecs = property.FindPropertyRelative("audioSpecs");
            SerializedProperty useWeights = property.FindPropertyRelative("useWeights");


            Button button = new Button
            {
                text = "Test",
                style = { height = 20, marginTop = 5 }
            };
            // Disable Test Button if there are no clips asd
            button.TrackPropertyValue(audioSpecs, (evt) => button.SetEnabled(audioSpecs.arraySize != 0));

            button.RegisterCallback<ClickEvent>(
                (evt) =>
                {
                    Audio audio = property.boxedValue as Audio;
                    AudioSpec spec;
                    if (selectedIndex.Value == -1) spec = audio.RandomAudioSpec;
                    else spec = audioSpecs.GetArrayElementAtIndex(selectedIndex.Value).FindPropertyRelative("item").boxedValue as AudioSpec;

                    AudioTester.Test(spec);
                }
            );

            return button;
        }
        VisualElement GetVariedField(SerializedProperty audioSpecProp, string var, Vector2 valueRange, float varyRange, float labelWidth)
        {
            var capitalizedVar = char.ToUpper(var[0]) + var.Substring(1);

            VisualElement box = new VisualElement();


            VisualElement labeledValueField = GetLabeledElement(GetSliderField(audioSpecProp.FindPropertyRelative(var), valueRange), "Value", labelWidth);
            VisualElement labeledVariationField = GetLabeledElement(GetSliderField(audioSpecProp.FindPropertyRelative(var + "Variation"), new Vector2(0, varyRange)), "Variation", labelWidth);


            // Add the fields
            box.Add(new Label(capitalizedVar));
            box.Add(labeledValueField);
            box.Add(labeledVariationField);



            return box;
        }
        VisualElement GetSliderField(SerializedProperty floatProp, Vector2 range)
        {
            // VALUE FIELD
            var floatSlider = new Slider(range.x, range.y) { style = { flexGrow = 1, marginLeft = 0 } };
            floatSlider.BindProperty(floatProp);

            // Create a TextField to display and edit the float value
            var floatField = GetFloatField();
            floatField.style.marginRight = 5;
            floatField.value = floatSlider.value;
            floatField.RegisterValueChangedCallback(evt =>
            {
                floatSlider.value = Mathf.Clamp(evt.newValue, floatSlider.lowValue, floatSlider.highValue);
            });
            floatSlider.RegisterValueChangedCallback(evt =>
            {
                floatField.value = evt.newValue;
            });

            var floatSliderField = new VisualElement();
            floatSliderField.style.flexDirection = FlexDirection.Row;
            floatSliderField.Add(floatField);
            floatSliderField.Add(floatSlider);

            return floatSliderField;
        }
        FloatField GetFloatField() => new FloatField(4) { style = { width = 33 } };
        /// <summary>
        /// Returns a new element rapping the passed in element with a label and style both elements as neccessary
        /// </summary>
        /// <returns></returns>
        VisualElement GetLabeledElement(VisualElement toLabel, string text, float labelWidth = 80)
        {   // Create and Style Label
            Label label = new(text) { style = { width = labelWidth, unityTextAlign = TextAnchor.MiddleLeft } };

            // Style element to Label
            toLabel.style.flexGrow = 1;
            //toLabel.style.paddingRight = 4;
            toLabel.style.overflow = Overflow.Visible;

            // Create, Populate, and Return labeled Element
            VisualElement labeledElement = new VisualElement() { style = { flexDirection = FlexDirection.Row, overflow = Overflow.Hidden, paddingRight = 2 } };
            labeledElement.Add(label);
            labeledElement.Add(toLabel);

            return labeledElement;
        }
    }
}