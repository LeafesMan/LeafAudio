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
using Unity.VisualScripting;

[CustomPropertyDrawer(typeof(RandomAudio))]
public class RandomAudioDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = new VisualElement();
        Box box = new Box();

        Label title = new Label("Clip Specs");
        title.style.unityTextAlign = TextAnchor.MiddleCenter;
        title.style.fontSize = 18;

        root.Add(GetSpacer());
        box.Add(GetTitle("Potential Clip"));
        //box.Add(GetHeader("General"));
        box.Add(new PropertyField(property.FindPropertyRelative("clip")));
        box.Add(new PropertyField(property.FindPropertyRelative("weight")));
        box.Add(GetHeader("Volume"));
        box.Add(GetEnumBasedField(property, "volume", new Vector2(0,1)));
        box.Add(GetHeader("Pitch"));
        box.Add(GetEnumBasedField(property, "pitch", new Vector2(-3, 3)));
        box.Add(GetTestButton(property));
        root.Add(box);


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
        header.style.marginTop = 10; // Add margin below the header

        return header;
    }
    public VisualElement GetTitle(string txt)
    {
        Label header = new Label(txt);
        header.style.fontSize = 16; // Set the font size for the header
        header.style.unityFontStyleAndWeight = FontStyle.Bold; // Make the font bold
        header.style.unityTextAlign = TextAnchor.MiddleCenter; // Center align text

        return header;
    }

    public Button GetTestButton(SerializedProperty property)
    {
        Button button = new Button();
        button.text = "Test Clip";
        button.style.height = 24;
        button.style.fontSize = 16;
        button.style.marginTop= 10;

        /*        // Try to get clip specs
                var targetObject = property.serializedObject.targetObject;
                var clipSpecs = fieldInfo.GetValue(targetObject) as TestClipSpecs;

                Debug.Log("Checking");
                if(clipSpecs != null)
                {
                    Debug.Log("Good");
                    button.RegisterCallback<ClickEvent>((evt) => { AudioManager.Test(clipSpecs.GetAudioData()); });

                }*/

        // Try to get clip specs

        // Extract the index from the property path
        string propertyPath = property.propertyPath;
        int startIndex = propertyPath.IndexOf("[") + 1;
        int endIndex = propertyPath.IndexOf("]");
        int index = int.Parse(propertyPath.Substring(startIndex, endIndex - startIndex));


        var targetObject = property.serializedObject.targetObject;
        var value = (fieldInfo.GetValue(targetObject) as RandomAudio[])[index];

        Debug.Log($"Field Value: {value} and index: {index}");

        if (value != null)
        {
            Debug.Log("Good");
            button.RegisterCallback<ClickEvent>((evt) => { AudioManager.Test(value.GetAudioData()); });
        }
        else
        {
            Debug.Log("ClipSpecs is null or of incorrect type.");
        }

        return button;
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


                var fieldType = value.changedProperty.GetEnumValue<RandomAudio.FieldType>();
                if (fieldType == RandomAudio.FieldType.Float) floatContainer.style.display = DisplayStyle.Flex;
                if (fieldType == RandomAudio.FieldType.Range) rangeContainer.style.display = DisplayStyle.Flex;
                if (fieldType == RandomAudio.FieldType.List) listField.style.display = DisplayStyle.Flex;
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
