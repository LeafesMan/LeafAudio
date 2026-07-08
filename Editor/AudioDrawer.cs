using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using LeafRand.Collections;
using LeafRand.Global;

/// <summary>
/// Draws an audio definition. Conceals uneeded vars and allows playing any individual spec or group of specs.
/// </summary>
[CustomPropertyDrawer(typeof(Audio))]
public class AudioDrawer : PropertyDrawer
{
    // Selection Vars
    int selectedIndex;
    int SelectedIndex { set { selectedIndex = value; SelectedIndexChanged?.Invoke(); } }
    event Action SelectedIndexChanged = null;

    // May be able to go without this look into it
    List<SerializedProperty> audioSpecPropsCache;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {   // Refresh State
        // Unity maintains values when returning to a property drawer
        SelectedIndex = -1;
        audioSpecPropsCache = new();

        // Get Audio Specs
        SerializedProperty audioSpecs = property.FindPropertyRelative("audioSpecs");


        // Generate Specs Container
        VisualElement specsContainer = new VisualElement();
        for (int i = 0; i < audioSpecs.arraySize; i++)
            InsertNewAudioSpecUI(specsContainer, property, i);


        // Create root container
        VisualElement root = new Foldout()
        {
            text = property.displayName,
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
        if (property.serializedObject.targetObject is AudioAsset) // Do not encase in foldout if in ShakeSpecsAsset
            root = new VisualElement();

        // Add/Remove UI from Specs Container on UNDO/REDO
        root.TrackPropertyValue(audioSpecs, (p) =>
        {
            // Nullify Selected Index
            SelectedIndex = -1;

            // On Adding element Update Weight Field Displays
            UpdateWeightFieldDisplays(property);

            // Removed an Element
            if (audioSpecs.arraySize < audioSpecPropsCache.Count)
            {

                int indexToRemoveUI = audioSpecPropsCache.Count - 1;
                // Find index of removed element
                for (int i = 0; i < audioSpecs.arraySize; i++)
                    if (!audioSpecs.GetArrayElementAtIndex(i).propertyPath.Equals(audioSpecPropsCache[i].propertyPath))
                        indexToRemoveUI = i;


                // Remove UI
                specsContainer.RemoveAt(indexToRemoveUI);
                audioSpecPropsCache.RemoveAt(indexToRemoveUI);
            }
            // Added an element
            else if (audioSpecs.arraySize > audioSpecPropsCache.Count)
            {
                int indexToAddUI = audioSpecs.arraySize - 1;
                // Find index of added element
                for (int i = 0; i < audioSpecPropsCache.Count; i++)
                {
                    // Cannot compare properties directly since they may actually be different object refs
                    if (!audioSpecs.GetArrayElementAtIndex(i).propertyPath.Equals(audioSpecPropsCache[i].propertyPath))
                        indexToAddUI = i;
                }


                Debug.Log($"Adding at: {indexToAddUI}");
                InsertNewAudioSpecUI(specsContainer, property, indexToAddUI);
            }
        });

        // Populate Root
        root.Add(GetHeader("Main Settings"));
        root.Add(GetLabeledElement(new PropertyField(property.FindPropertyRelative("mixerGroup"), ""), "Mixer Group"));
        root.Add(GetWeightedToggle(property));
        root.Add(GetHeader("Audio Specs"));
        root.Add(specsContainer);
        root.Add(GetTestButton(property));
        root.Add(GetAddButton(audioSpecs));
        root.Add(GetRemoveButton(audioSpecs));

        return root;
    }


    void UpdateWeightFieldDisplays(SerializedProperty prop)
    {
        DisplayStyle style = GetDisplayStyle(prop.FindPropertyRelative("useWeights").boolValue);
        //foreach (var info in audioSpecUIInfos)
        //info.weightElement.style.display = style;
    }

    void InsertNewAudioSpecUI(VisualElement root, SerializedProperty audioProp, int index)
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

            if (myIndex == selectedIndex) SelectedIndex = -1;
            else SelectedIndex = myIndex;

            Debug.Log("Selected Index: " + selectedIndex);



            // Have to find the element ourselves because unity occasionally performs a deep copy of all VisualElements rebuilding the tree
            // so if I hold a reference to a visual element in an event, and throw that event, the event will act on the visual element in the old tree
            var rootArray = root.Children().ToArray();
            for (int i = 0; i < root.childCount; i++)
            {
                Debug.Log("Contains Foldout? " + root.Contains(foldout));

                if (selectedIndex == i) rootArray[i].style.backgroundColor = new Color(.17f, .32f, .56f);
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


        // Add Latest AudioSpecUI Info
        audioSpecPropsCache.Insert(index, audioSpecProp);
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
    VisualElement GetWeightedToggle(SerializedProperty audio)
    {
        PropertyField weightToggle = new PropertyField(audio.FindPropertyRelative("useWeights"), "");
        weightToggle.RegisterValueChangeCallback((evt) => UpdateWeightFieldDisplays(audio));
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
    Button GetRemoveButton(SerializedProperty audioSpecs)
    {
        var removeButton = new Button(() =>
        {
            // Remove shake component and corresponding bool
            audioSpecs.DeleteArrayElementAtIndex(selectedIndex);
            audioSpecs.serializedObject.ApplyModifiedProperties();

            SelectedIndex = -1;
        })
        { text = "Remove" };
        removeButton.SetEnabled(false);
        SelectedIndexChanged += () => removeButton.SetEnabled(selectedIndex != -1);

        return removeButton;
    }
    Button GetTestButton(SerializedProperty property)
    {
        SerializedProperty audioSpecs = property.FindPropertyRelative("audioSpecs");
        SerializedProperty useWeights = property.FindPropertyRelative("useWeights");

        Button button = new Button();
        button.text = "Test";
        button.style.height = 20;
        button.style.marginTop = 5;

        // Disable Test Button if there are no clips
        button.TrackPropertyValue(audioSpecs, (evt) => button.SetEnabled(audioSpecs.arraySize != 0));

        button.RegisterCallback<ClickEvent>(
            (evt) =>
            {
                // Choose the AudioSpec that should be played
                SerializedProperty audioSpec;
                if (selectedIndex == -1)
                {
                    // Build Array of weighted Indices matching the audio spec weights
                    List<Weighted<int>> weightedIndices = new();
                    for (int i = 0; i < audioSpecs.arraySize; i++)
                        weightedIndices.Add(new(i, audioSpecs.GetArrayElementAtIndex(i).FindPropertyRelative("weight").floatValue));

                    // Select Rand Index
                    // If use weights is selected                                         select a weighted random index  otherwise    grab a random index
                    int randIndex = useWeights.boolValue ? Rand.Item(weightedIndices).Item : Rand.ItemWeighted(weightedIndices);

                    audioSpec = audioSpecs.GetArrayElementAtIndex(randIndex);
                }
                else audioSpec = audioSpecs.GetArrayElementAtIndex(selectedIndex);


                // Test the Audio Spec
                AudioManager.Test(audioSpec.FindPropertyRelative("clip").objectReferenceValue as AudioClip, (audioSpec.boxedValue as AudioSpec).GetVolume(), (audioSpec.boxedValue as AudioSpec).GetPitch());
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