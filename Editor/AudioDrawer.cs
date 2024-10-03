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

[CustomPropertyDrawer(typeof(Audio))]
public class AudioDrawer : PropertyDrawer
{
    bool initialized = false;


    static bool showWeight;
    static bool showFieldType;
    static bool showTestButton;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {

        // Create the root container
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
        // Do not encase in foldout if in ShakeSpecsAsset
        if (property.serializedObject.targetObject is AudioAsset)
            root = new VisualElement();


        SerializedProperty audioSpecsProperty = property.FindPropertyRelative("audioSpecs");

        VisualElement specsContainer = new VisualElement();

        // Button to add a shake component
        Button addButton = new Button();
        addButton = new Button(() =>
        {
            // Add a new shake component of the selected type
            audioSpecsProperty.arraySize++;
            audioSpecsProperty.serializedObject.ApplyModifiedProperties();

            // Initialize the new shake component in the list
            var newSpecProperty = audioSpecsProperty.GetArrayElementAtIndex(audioSpecsProperty.arraySize - 1);
            newSpecProperty.managedReferenceValue = (AudioSpec)Activator.CreateInstance(typeof(AudioSpec));

            newSpecProperty.serializedObject.ApplyModifiedProperties();

            // Refresh the UI to show the new element
            RefreshSpecsContainer();
        }) { text = "Add" };

        

        // Settings
        VisualElement settingsFoldout = new Foldout() { text = "Settings" };

        // Show Weights
        Toggle weightToggle = new Toggle("Show Weights");
        weightToggle.value = showWeight;
        weightToggle.RegisterValueChangedCallback((val) => { showWeight = val.newValue; RefreshSpecsContainer(); });
        settingsFoldout.Add(weightToggle);

        // Show Types
        Toggle fieldTypesToggle = new Toggle("Show Field Types");
        fieldTypesToggle.value = showWeight;
        fieldTypesToggle.RegisterValueChangedCallback((val) => { showFieldType = val.newValue; RefreshSpecsContainer(); });
        settingsFoldout.Add(fieldTypesToggle);

        // Show Test Button
        Toggle testButtonToggle = new Toggle("Show Test Buttons");
        testButtonToggle.value = showWeight;
        testButtonToggle.RegisterValueChangedCallback((val) => { showTestButton = val.newValue; RefreshSpecsContainer(); });
        settingsFoldout.Add(testButtonToggle);



        root.Add(new PropertyField(property.FindPropertyRelative("audioMixer")));
        root.Add(settingsFoldout);
        root.Add(GetHeader("Audio Specs"));
        root.Add(specsContainer);
        root.Add(addButton);

        void RefreshSpecsContainer()
        {
            Debug.Log("Refreshing");
            specsContainer.Clear(); // Clear previous UI


            // Add Remove and add button afterwards
            int selectedSpecs = -1;
            var removeButton = new Button(() =>
            {
                // Remove shake component and corresponding bool
                audioSpecsProperty.DeleteArrayElementAtIndex(selectedSpecs);
                audioSpecsProperty.serializedObject.ApplyModifiedProperties();
                RefreshSpecsContainer(); // Refresh the UI
            }) { text = "Remove Selected" };
            removeButton.SetEnabled(false);
            Action specsClicked = () => removeButton.SetEnabled(selectedSpecs != -1);


            // Add each shake component to specs container
            // - Add property drawer for componentSpecs
            // - Add bool for overriding 
            for (int i = 0; i < audioSpecsProperty.arraySize; i++)
            {
                int index = i; // Cache index


                // Get current shake component and override bool
                var audioSpecProperty = audioSpecsProperty.GetArrayElementAtIndex(i);

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
                    if (index == selectedSpecs) selectedSpecs = -1;
                    else selectedSpecs = index;

                    specsClicked?.Invoke();
                });
                specsClicked += () =>
                {
                    if (selectedSpecs == index) foldout.style.backgroundColor = new Color(.17f, .32f, .56f);
                    else foldout.style.backgroundColor = new Color(0.27f, 0.27f, 0.27f);
                };
                foldout.style.backgroundColor = new Color(0.27f, 0.27f, 0.27f);
                specsContainer.Add(foldout);

                // Add the shake component fields

                // Weight Field
                var weightProperty = audioSpecProperty.FindPropertyRelative("weight");
                var weightField = new PropertyField(weightProperty);
                weightField.style.display = showWeight ? DisplayStyle.Flex : DisplayStyle.None;
                weightField.BindProperty(weightProperty);


                VisualElement root = new VisualElement();

                var clipProp = audioSpecProperty.FindPropertyRelative("clip");
                var clipField = new PropertyField(clipProp);
                clipField.BindProperty(clipProp);

                var title = new Label() { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 15, marginTop = 3f } };
                clipField.RegisterValueChangeCallback((val) => title.text = clipProp.objectReferenceValue == null ? "No Clip" : clipProp.objectReferenceValue.name);

                foldout.Add(title);
                foldout.Add(clipField);
                foldout.Add(GetHeader("Volume"));
                foldout.Add(GetEnumBasedField(audioSpecProperty, "volume", new Vector2(0, 1), showFieldType));
                foldout.Add(GetHeader("Pitch"));
                foldout.Add(GetEnumBasedField(audioSpecProperty, "pitch", new Vector2(-3, 3), showFieldType));

                if(showTestButton) foldout.Add(GetTestButton(audioSpecProperty));
                foldout.Add(GetSpacer());

                foldout.Add(weightField);
            }


            specsContainer.Add(removeButton);
        }

        // Initial call to refresh the list of Audio Specs
        RefreshSpecsContainer();


        // Refresh Specs Container on UNDO/REDO
        // only subscribe once hence initialized
        if (!initialized) root.TrackPropertyValue(audioSpecsProperty, (p) => RefreshSpecsContainer());
        initialized = false;

        return root;
    }



    /*
    private VisualElement GetHeader(string headerText)
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
    */
    public VisualElement GetSpacer()
    {
        VisualElement spacer = new VisualElement();
        spacer.style.flexGrow = 1;
        spacer.style.height = 12;


        return spacer;
    }
    public VisualElement GetHeader(string txt)
    {
        Label header = new Label(txt);
        header.style.fontSize = 12; // Set the font size for the header
        header.style.unityFontStyleAndWeight = FontStyle.Bold; // Make the font bold
        header.style.marginTop = 12;


        return header;
    }
    public Button GetTestButton(SerializedProperty property)
    {
        Button button = new Button();
        button.text = "Test";
        button.style.height = 24;
        button.style.fontSize = 16;
        button.style.marginTop = 10;


        //Debug.Log($"Playing with volume: {volume} ({volFieldType}) and pitch: {pitch} ({fieldType})");


        button.RegisterCallback<ClickEvent>(
            (evt) =>
            {
                AudioManager.Test(property.FindPropertyRelative("clip").objectReferenceValue as AudioClip, GetValue("volume"), GetValue("pitch"));

            }
        );

        return button;


        float GetValue(string toGet)
        {
            var fieldType = property.FindPropertyRelative(toGet + "Type").GetEnumValue<AudioSpec.FieldType>();
            var floatValue = property.FindPropertyRelative(toGet).floatValue;
            var rangeValue = property.FindPropertyRelative(toGet + "Range").vector2Value;
            var arrayValue = ConvertPropertyToFloatArray(property.FindPropertyRelative(toGet + "List"));
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
    public VisualElement GetEnumBasedField(SerializedProperty property, string var, Vector2 range, bool showType)
    {
        VisualElement box = new VisualElement();
        var typeProperty = property.FindPropertyRelative(var + "Type");
        var typeField = new PropertyField(typeProperty, "Type");
        typeField.BindProperty(typeProperty);
        typeField.style.display = showFieldType ? DisplayStyle.Flex : DisplayStyle.None;
        box.Add(typeField);




        // VALUE FIELD
        var floatSlider = new Slider(range.x, range.y) { bindingPath = property.FindPropertyRelative(var).propertyPath };
        floatSlider.style.flexGrow = 1;

        // Create a TextField to display and edit the float value
        var floatField = new TextField
        {
            value = floatSlider.value.ToString(),
            style = { width = 60, marginLeft = 10 }
        };
        floatField.RegisterValueChangedCallback(evt =>
        {
            if (float.TryParse(evt.newValue, out var newValue))
            {
                floatSlider.value = Mathf.Clamp(newValue, floatSlider.lowValue, floatSlider.highValue);
            }
        });
        floatSlider.RegisterValueChangedCallback(evt =>
        {
            floatField.value = evt.newValue.ToString();
        });

        VisualElement floatContainer = new VisualElement();
        floatContainer.style.flexDirection = FlexDirection.Row;
        floatContainer.Add(floatField);
        floatContainer.Add(floatSlider);


        // RANGE FIELD
        var rangeSlider = new MinMaxSlider(range.x, range.y, range.x, range.y) { bindingPath = property.FindPropertyRelative(var + "Range").propertyPath };
        rangeSlider.style.flexGrow = 1;
        TextField rangeFieldX = GetRangeField(rangeSlider, true);
        TextField rangeFieldY = GetRangeField(rangeSlider, false);

        // Create a container for the float field
        VisualElement rangeContainer = new VisualElement();
        rangeContainer.style.flexDirection = FlexDirection.Row;
        rangeContainer.Add(rangeFieldX);
        rangeContainer.Add(rangeSlider);
        rangeContainer.Add(rangeFieldY);

        // LIST FIELD
        var listField = new PropertyField(property.FindPropertyRelative(var + "List"), " ");
        listField.BindProperty(property.FindPropertyRelative(var + "List"));

        // Add the fields
        box.Add(floatContainer);
        box.Add(rangeContainer);
        box.Add(listField);



        // Show correct field based on field type
        DisplayField(typeProperty.GetEnumValue<AudioSpec.FieldType>());
        typeField.RegisterValueChangeCallback((v) => DisplayField(v.changedProperty.GetEnumValue<AudioSpec.FieldType>()));
        void DisplayField(AudioSpec.FieldType typeToDisplay)
        {

            floatContainer.style.display = typeToDisplay == AudioSpec.FieldType.Float ? DisplayStyle.Flex : DisplayStyle.None;
            rangeContainer.style.display = typeToDisplay == AudioSpec.FieldType.Range ? DisplayStyle.Flex : DisplayStyle.None;
            listField.style.display      = typeToDisplay == AudioSpec.FieldType.List  ? DisplayStyle.Flex : DisplayStyle.None;
        }


        return box;
    }
    private static TextField GetRangeField(MinMaxSlider rangeSlider, bool isXField)
    {
        var rangeField = new TextField
        {
            value = rangeSlider.value.x.ToString(),
            style = { width = 40, marginLeft = 10, marginRight = 10, }
        };

        // Range Field updated
        rangeField.RegisterValueChangedCallback(evt =>
        {
            if (float.TryParse(evt.newValue, out var newValue))
            {
                float newVal = Mathf.Clamp(newValue, rangeSlider.lowLimit, rangeSlider.highLimit);
                rangeSlider.value = isXField ? new Vector2(newVal, rangeSlider.value.y) : new Vector2(rangeSlider.value.x, newVal);
            }
        });

        // Range Slider updated
        rangeSlider.RegisterValueChangedCallback(evt =>
        {
            rangeField.value = isXField ? evt.newValue.x.ToString() : evt.newValue.y.ToString();
        });
        return rangeField;
    }
}
