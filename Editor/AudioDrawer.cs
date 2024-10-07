/*
 * Auth: Ian
 * 
 * Proj: Audio
 * 
 * Date: 10/2/24
 * 
 * Desc: An audio Drawer for
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Rendering;
using System.Linq;
using Unity.VisualScripting;

[CustomPropertyDrawer(typeof(Audio))]
public class AudioDrawer : PropertyDrawer
{
    // Selection Vars
    int selectedIndex;
    int SelectedIndex { set { selectedIndex = value; SelectedIndexChanged?.Invoke(); } }
    event Action SelectedIndexChanged = null;

    // UI Cache
    List<AudioSpecUIInfo> audioSpecUIInfos;

    // Static Vars
    static bool showFieldType;


    [System.Serializable]
    class AudioSpecUIInfo
    {
        public SerializedProperty prop;
        public VisualElement weightElement;
        public VisualElement pitchElement;
        public VisualElement volumeElement;
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {   // Refresh State
        // Unity maintains values when returning to a property drawer
        SelectedIndex = -1;
        audioSpecUIInfos = new();


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
            Debug.Log($"{audioSpecs.arraySize} < {audioSpecUIInfos.Count}");
            if (audioSpecs.arraySize < audioSpecUIInfos.Count)
            {

                int indexToRemoveUI = audioSpecUIInfos.Count - 1;
                // Find index of removed element
                for (int i = 0; i < audioSpecs.arraySize; i++)
                    if (!audioSpecs.GetArrayElementAtIndex(i).propertyPath.Equals(audioSpecUIInfos[i].prop.propertyPath))
                        indexToRemoveUI = i;


                // Remove UI
                specsContainer.RemoveAt(indexToRemoveUI);
                audioSpecUIInfos.RemoveAt(indexToRemoveUI);
            }
            // Added an element
            else if (audioSpecs.arraySize > audioSpecUIInfos.Count)
            {
                int indexToAddUI = audioSpecs.arraySize - 1;
                // Find index of added element
                for (int i = 0; i < audioSpecUIInfos.Count; i++)
                {
                    // Cannot compare properties directly since they may actually be different object refs
                    if (!audioSpecs.GetArrayElementAtIndex(i).propertyPath.Equals(audioSpecUIInfos[i].prop.propertyPath))
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
        root.Add(GetFieldTypesToggle());
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
        foreach (var info in audioSpecUIInfos) 
            info.weightElement.style.display = style;
    }

    void InsertNewAudioSpecUI(VisualElement root, SerializedProperty audio, int index)
    {
        float labelWidth = 69;
        SerializedProperty audioSpec = audio.FindPropertyRelative("audioSpecs").GetArrayElementAtIndex(index);
        SerializedProperty useWeights = audio.FindPropertyRelative("useWeights");

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
            for(int i = 0; i < root.childCount; i++)
            {
                Debug.Log("Contains Foldout? " + root.Contains(foldout));

                if (selectedIndex == i) rootArray[i].style.backgroundColor = new Color(.17f, .32f, .56f);
                else rootArray[i].style.backgroundColor = new Color(0.27f, 0.27f, 0.27f);
            }
        });
        foldout.style.backgroundColor = new Color(0.27f, 0.27f, 0.27f);



        // Weight Field
        var weightProperty = audioSpec.FindPropertyRelative("weight");
        var weightField = new PropertyField(weightProperty, "");
        weightField.BindProperty(weightProperty);
        var weightLabeled = GetLabeledElement(weightField, "Weight", labelWidth);
        weightLabeled.style.display = GetDisplayStyle(useWeights.boolValue);

        var clipProp = audioSpec.FindPropertyRelative("clip");
        var clipField = new PropertyField(clipProp, "");
        clipField.BindProperty(clipProp);

        var title = new Label() { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 15, marginTop = 3f, overflow = Overflow.Hidden } };
        clipField.RegisterValueChangeCallback((val) => title.text = clipProp.objectReferenceValue == null ? "No Clip" : clipProp.objectReferenceValue.name);



        var volumeElements = GetLabeledEnumBasedField(audioSpec, "volume", new Vector2(0, 1), labelWidth);
        var pitchElements = GetLabeledEnumBasedField(audioSpec, "pitch", new Vector2(-3, 3), labelWidth);



        foldout.Add(title);
        foldout.Add(GetLabeledElement(clipField, "Clip", labelWidth));
        foldout.Add(volumeElements.main);
        foldout.Add(pitchElements.main);
        foldout.Add(weightLabeled);
        


        // Add the Element to the given container
        root.Insert(index, foldout);


        // Add Latest AudioSpecUI Info
        audioSpecUIInfos.Insert(index, new() 
        { 
            prop = audioSpec, 
            pitchElement = pitchElements.typeField, 
            volumeElement = volumeElements.typeField, 
            weightElement = weightLabeled
        });
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
    VisualElement GetFieldTypesToggle()
    {
        Toggle fieldTypesToggle = new Toggle();
        fieldTypesToggle.value = showFieldType;
        fieldTypesToggle.RegisterValueChangedCallback(
            (val) =>
            {
                showFieldType = val.newValue;
                audioSpecUIInfos.ForEach((info) =>
                {
                    info.pitchElement.style.display = GetDisplayStyle(showFieldType);
                    info.volumeElement.style.display = GetDisplayStyle(showFieldType);
                }
                );
            }
        );
        return GetLabeledElement(fieldTypesToggle, "Field Types");
    }
    Button GetAddButton(SerializedProperty audioSpecs)
    {
        Button addButton = new Button(() =>
        {
            // Add a new shake component of the selected type
            audioSpecs.arraySize++;

            // Initialize the new shake component in the list
            var newAudioSpec = audioSpecs.GetArrayElementAtIndex(audioSpecs.arraySize - 1);
            newAudioSpec.managedReferenceValue = (AudioSpec)Activator.CreateInstance(typeof(AudioSpec));
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
                    int randIndex = useWeights.boolValue ? SRand.Weighted(weightedIndices) : SRand.Element(weightedIndices).element;

                    audioSpec = audioSpecs.GetArrayElementAtIndex(randIndex);
                }
                else audioSpec = audioSpecs.GetArrayElementAtIndex(selectedIndex);


                // Test the Audio Spec
                AudioManager.Test(audioSpec.FindPropertyRelative("clip").objectReferenceValue as AudioClip, GetValue("volume"), GetValue("pitch"));


                float GetValue(string toGet)
                {
                    var fieldType = audioSpec.FindPropertyRelative(toGet + "Type").GetEnumValue<AudioSpec.FieldType>();
                    var floatValue = audioSpec.FindPropertyRelative(toGet).floatValue;
                    var rangeValue = audioSpec.FindPropertyRelative(toGet + "Range").vector2Value;
                    var arrayValue = ConvertPropertyToFloatArray(audioSpec.FindPropertyRelative(toGet + "List"));
                    return AudioSpec.GetValue(fieldType, floatValue, rangeValue, arrayValue);
                }
                Weighted<float>[] ConvertPropertyToFloatArray(SerializedProperty property)
                {
                    // Ensure the property is an array or list
                    if (!property.isArray)
                    {
                        Debug.LogError("Property is not an array.");
                        return null;
                    }

                    // Get the size of the array
                    int arraySize = property.arraySize;

                    // Create a float array to store the values
                    Weighted<float>[] floatArray = new Weighted<float>[arraySize];

                    // Loop through the SerializedProperty array and get each element as a float
                    for (int i = 0; i < arraySize; i++)
                    {
                        // Get the float value from each array element
                        SerializedProperty element = property.GetArrayElementAtIndex(i);

                        floatArray[i] = new(element.FindPropertyRelative("element").floatValue, element.FindPropertyRelative("weight").floatValue);

                    }

                    return floatArray;
                }
            }
        );

        return button;
    }
    (VisualElement main, VisualElement typeField) GetLabeledEnumBasedField(SerializedProperty property, string var, Vector2 range, float labelWidth)
    {
        var capitalizedVar = var.FirstCharacterToUpper();

        VisualElement box = new VisualElement();
        var typeProperty = property.FindPropertyRelative(var + "Type");
        var typeField = new PropertyField(typeProperty, "");
        typeField.BindProperty(typeProperty);

        VisualElement labeledTypeField = GetLabeledElement(typeField, "Field Type", labelWidth);
        labeledTypeField.style.display = GetDisplayStyle(showFieldType);
        box.Add(labeledTypeField);




        // VALUE FIELD
        var floatSlider = new Slider(range.x, range.y) { style = {flexGrow = 1, marginLeft = 0}};
        floatSlider.BindProperty(property.FindPropertyRelative(var));

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

        VisualElement labeledFloat = GetLabeledElement(floatSliderField, capitalizedVar, labelWidth);


        // RANGE FIELD
        var rangeSlider = new MinMaxSlider(range.x, range.y, range.x, range.y) { style = { marginLeft = 0}};
        rangeSlider.BindProperty(property.FindPropertyRelative(var + "Range"));
        rangeSlider.style.flexGrow = 1;
        FloatField rangeFieldX = GetRangeField(rangeSlider, true);
        FloatField rangeFieldY = GetRangeField(rangeSlider, false);

        // Create a container for the float field
        VisualElement rangeContainer = new VisualElement();
        rangeContainer.style.flexDirection = FlexDirection.Row;
        rangeContainer.Add(rangeFieldX);
        rangeContainer.Add(rangeSlider);
        rangeContainer.Add(rangeFieldY);

        VisualElement labeledRange = GetLabeledElement(rangeContainer, capitalizedVar, labelWidth);


        // LIST FIELD
        var listField = new PropertyField(property.FindPropertyRelative(var + "List"), var.FirstCharacterToUpper());
        listField.BindProperty(property.FindPropertyRelative(var + "List"));

        // Add the fields
        box.Add(labeledFloat);
        box.Add(labeledRange);
        box.Add(listField);



        // Show correct field based on field type
        DisplayField(typeProperty.GetEnumValue<AudioSpec.FieldType>());
        typeField.RegisterValueChangeCallback((v) => DisplayField(v.changedProperty.GetEnumValue<AudioSpec.FieldType>()));
        void DisplayField(AudioSpec.FieldType typeToDisplay)
        {

            labeledFloat.style.display = typeToDisplay == AudioSpec.FieldType.Float ? DisplayStyle.Flex : DisplayStyle.None;
            labeledRange.style.display = typeToDisplay == AudioSpec.FieldType.Range ? DisplayStyle.Flex : DisplayStyle.None;
            listField.style.display    = typeToDisplay == AudioSpec.FieldType.List  ? DisplayStyle.Flex : DisplayStyle.None;
        }


        return (box, labeledTypeField);
    }
    FloatField GetRangeField(MinMaxSlider rangeSlider, bool isXField)
    {
        var rangeField = GetFloatField();
        rangeField.value = rangeSlider.value.x;
        if (isXField) rangeField.style.marginRight = 5;
        else rangeField.style.marginLeft = 5;

        // Range Field updated
        rangeField.RegisterValueChangedCallback(evt =>
        {
            
            float newVal = Mathf.Clamp(evt.newValue, rangeSlider.lowLimit, rangeSlider.highLimit);
            rangeSlider.value = isXField ? new Vector2(newVal, rangeSlider.value.y) : new Vector2(rangeSlider.value.x, newVal);            
        });

        // Range Slider updated
        rangeSlider.RegisterValueChangedCallback(evt =>
        {
            rangeField.value = isXField ? evt.newValue.x : evt.newValue.y;
        });
        return rangeField;
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
