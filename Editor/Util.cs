using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LeafAudio.Editor
{
    public static class Util
    {
        public const float DefaultLabelWidth = 55;
        /// <summary>
        /// Tracks the serialized object and shows whenever the condition is met
        /// </summary>
        public static void ShowIfCondition(SerializedObject serializedObjectToTrack, VisualElement elementToHide, Func<bool> condition)
        {
            UpdateShown();
            elementToHide.TrackSerializedObjectValue(serializedObjectToTrack, (obj) => UpdateShown());
            void UpdateShown() => elementToHide.style.display = condition() ? DisplayStyle.Flex : DisplayStyle.None;
        }
        public static VisualElement GetSpacer() => new VisualElement() { style = { height = 10 } };
        public static VisualElement GetPropField(string propName, string label, float labelWidth = DefaultLabelWidth) => GetLabeledElement(new PropertyField() { label = "", bindingPath = propName }, label, labelWidth: labelWidth);
        internal static BindableElement GetLabeledElement(VisualElement toLabel, string text, string name = "", float labelWidth = DefaultLabelWidth, string tooltip = "")
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
    }
}