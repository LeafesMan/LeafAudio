/*
 *  Name: Ian
 *
 *  Proj: MonMon
 *
 *  Date: 7/27/23 
 *  
 *  Desc: Property attribute for Vector2 and Vector2Int. Displays a slider from SliderAttributeMin->Max. Changes in slider value or text field value update the Vectors x and y values.
 */

using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SliderAttribute))]
public class RangedDrawer : PropertyDrawer
{


    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // First get the attribute since it contains the range for the slider
        SliderAttribute sliderAttribute = attribute as SliderAttribute;

        // Now draw the property as a Slider or an IntSlider based on whether it's a float or integer.
        if (property.propertyType == SerializedPropertyType.Vector2)
        {
            float min = property.vector2Value.x;
            float max = property.vector2Value.y;


            /************************************** DRAW FIELDS AND SLIDER **************************************/
            //Draw Label
            EditorGUI.LabelField(position, property.name);

            //Scoot right just enough space to draw next 3 elements
            position.x += position.width - 210;

            //Min TextField
            position.width = 50;
            string minText = EditorGUI.TextField(position, $"{property.vector2Value.x}");

            //Slider
            position.x += 55;
            position.width = 100;
            EditorGUI.MinMaxSlider(position, ref min, ref max, sliderAttribute.min, sliderAttribute.max);

            //Max TextField
            position.x += 105;
            position.width = 50;
            string maxText = EditorGUI.TextField(position, $"{property.vector2Value.y}");

            //UPDATE VALUES
            if (min != property.vector2Value.x || max != property.vector2Value.y) //Slider Value Changed
                property.vector2Value = new Vector2(min, max);
            else if ((float.TryParse(minText, out min) && min != property.vector2Value.x) || (float.TryParse(maxText, out max) && max != property.vector2Value.y))
                property.vector2Value = new Vector2(min, max);
        }
        else if (property.propertyType == SerializedPropertyType.Vector2Int)
        {
            float min = property.vector2IntValue.x;
            float max = property.vector2IntValue.y;


            /************************************** DRAW FIELDS AND SLIDER **************************************/
            //Draw Label
            EditorGUI.LabelField(position, property.name);

            //Scoot right just enough space to draw next 3 elements
            position.x += position.width - 210;

            //Min TextField
            position.width = 50;
            string minText = EditorGUI.TextField(position, $"{property.vector2IntValue.x}");

            //Slider
            position.x += 55;
            position.width = 100;
            EditorGUI.MinMaxSlider(position, ref min, ref max, sliderAttribute.min, sliderAttribute.max);

            //Max TextField
            position.x += 105;
            position.width = 50;
            string maxText = EditorGUI.TextField(position, $"{property.vector2IntValue.y}");

            //UPDATE VALUES
            if (min != property.vector2IntValue.x || max != property.vector2IntValue.y) //Slider Value Changed
                property.vector2IntValue = new Vector2Int((int)min, (int)max);
            else if ((float.TryParse(minText, out min) && min != property.vector2IntValue.x) || (float.TryParse(maxText, out max) && max != property.vector2IntValue.y))
                property.vector2IntValue = new Vector2Int((int)min, (int)max);
        }
        else
            EditorGUI.LabelField(position, label.text, "Use SliderAttribute with Vector 2");
    }
}