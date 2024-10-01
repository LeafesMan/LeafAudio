/*
 * Auth: Ian
 * 
 * Proj: SpaceS
 * 
 * Desc: Custom Editor Drawer for 
 * 
 * Date: /24
 */

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Rendering;

[CustomPropertyDrawer(typeof(AudioSpec))]
public class AudioSpecDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        Foldout root = new Foldout();


        SerializedProperty weightProperty = property.FindPropertyRelative("weight");
        PropertyField weightField = new PropertyField(weightProperty);
        weightField.RegisterValueChangeCallback(weight => { root.text = $"Weighted Clip ({weightProperty.floatValue})"; });

        root.Add(GetHeader("General"));
        root.Add(weightField);
        root.Add(new PropertyField(property.FindPropertyRelative("clip")));
        root.Add(GetHeader("Volume"));
        root.Add(GetEnumBasedField(property, "volume", new Vector2(0,1)));
        root.Add(GetHeader("Pitch"));
        root.Add(GetEnumBasedField(property, "pitch", new Vector2(-3, 3)));

        root.Add(GetTestButton(property));
        root.Add(GetSpacer());

        return root;
    }


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
        button.style.marginTop= 10;


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

                floatArray[i] = new (element.FindPropertyRelative("element").floatValue, element.FindPropertyRelative("weight").floatValue);
                
            }

            return floatArray;
        }
    }
    public VisualElement GetEnumBasedField(SerializedProperty property, string var, Vector2 range)
    {
        VisualElement box = new VisualElement();
        var typeField = new PropertyField(property.FindPropertyRelative(var + "Type"));
        box.Add(typeField);

        // Get field Properties

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

        // Create a container for the float field
        VisualElement floatContainer = new VisualElement();
        floatContainer.style.flexDirection = FlexDirection.Row;
        floatContainer.Add(floatField);
        floatContainer.Add(floatSlider);


        var rangeSlider = new MinMaxSlider(range.x, range.y, range.x, range.y) { bindingPath = property.FindPropertyRelative(var + "Range").propertyPath };
        rangeSlider.style.flexGrow = 1;
        TextField rangeFieldX = GetLabel(rangeSlider, true);
        TextField rangeFieldY = GetLabel(rangeSlider, false);

        // Create a container for the float field
        VisualElement rangeContainer = new VisualElement();
        rangeContainer.style.flexDirection = FlexDirection.Row;
        rangeContainer.Add(rangeFieldX);
        rangeContainer.Add(rangeSlider);
        rangeContainer.Add(rangeFieldY);


        var listField = new PropertyField(property.FindPropertyRelative(var + "List"));


        // Add the fields
        box.Add(floatContainer);
        box.Add(rangeContainer);
        box.Add(listField);



        // Show correct field based on field type
        typeField.RegisterValueChangeCallback(
            (value) =>
            {
                floatContainer.style.display = DisplayStyle.None;
                rangeContainer.style.display = DisplayStyle.None;
                listField.style.display = DisplayStyle.None;


                var fieldType = value.changedProperty.GetEnumValue<AudioSpec.FieldType>();
                if (fieldType == AudioSpec.FieldType.Float) floatContainer.style.display = DisplayStyle.Flex;
                if (fieldType == AudioSpec.FieldType.Range) rangeContainer.style.display = DisplayStyle.Flex;
                if (fieldType == AudioSpec.FieldType.List) listField.style.display = DisplayStyle.Flex;
            });

        return box;
    }


    /// <summary>
    /// Generates a label for the given slider
    /// </summary>
    /// <param name="rangeSlider"></param>
    /// <returns></returns>
    private static TextField GetLabel(MinMaxSlider rangeSlider, bool isXLabel)
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
                float newVal = Mathf.Clamp(newValue, rangeSlider.minValue, rangeSlider.maxValue);
                rangeSlider.value = isXLabel ? new Vector2(newVal, rangeSlider.value.y) : new Vector2(rangeSlider.value.x, newVal);
            }
        });

        // Range Slider updated
        rangeSlider.RegisterValueChangedCallback(evt =>
        {
            rangeField.value = isXLabel ? evt.newValue.x.ToString() : evt.newValue.y.ToString();
        });
        return rangeField;
    }
}
