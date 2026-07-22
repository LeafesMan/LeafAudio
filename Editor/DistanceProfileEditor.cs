using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LeafAudio.Editor
{
    [CustomEditor(typeof(DistanceProfile), editorForChildClasses: true)]
    public class DistanceProfileEditor : UnityEditor.Editor
    {
        Vector2 prevDomain;
        public override VisualElement CreateInspectorGUI()
        {   // Grab Props and Vars from field
            SerializedProperty curveProp = serializedObject.FindProperty(nameof(DistanceProfile.curve));
            SerializedProperty curveDomainProp = serializedObject.FindProperty(nameof(DistanceProfile.curveDomain));
            Vector2 curveRange = (target as DistanceProfile).CurveRange; // This value is set and thus not a prop

            // Setup Max Distance Field
            var curveDomainField = new Vector2Field();
            curveDomainField.Query<VisualElement>(classes: "unity-base-field__label--with-dragger").ForEach(element => element.RegisterCallback<MouseUpEvent>(e => UpdateDomainValue()));
            curveDomainField.RegisterCallback<BlurEvent>(e => UpdateDomainValue());
            void UpdateDomainValue()
            {
                Vector2 newDomain = DistanceProfile.ValidateCurveDomain(curveDomainField.value);

                // Ensure value validated immediately
                curveDomainField.SetValueWithoutNotify(newDomain);
                if (prevDomain == newDomain) return; // Exit early if no change after validation




                // Update Domain Value
                curveDomainProp.vector2Value = newDomain;

                // Update Curve Values for new domain
                float prevWidth = prevDomain.y - prevDomain.x;
                float newWidth = newDomain.y - newDomain.x;
                float percentWidthChange = newWidth / prevWidth;
                AnimationCurve newCurve = curveProp.animationCurveValue;
                Keyframe[] keys = newCurve.keys;
                for (int i = keys.Length - 1; i >= 0; i--)
                {
                    keys[i].time = newDomain.x + (keys[i].time - prevDomain.x) * percentWidthChange;
                    keys[i].inTangent /= percentWidthChange;
                    keys[i].outTangent /= percentWidthChange;
                }

                newCurve.keys = keys;
                curveProp.animationCurveValue = newCurve;

                serializedObject.ApplyModifiedProperties();
            }
            UpdateUIValue(); // Initialize UI Values
            curveDomainField.TrackPropertyValue(curveDomainProp, p => UpdateUIValue()); // Update UI Values
            void UpdateUIValue()
            {
                curveDomainField.SetValueWithoutNotify(curveDomainProp.vector2Value); // Initialize field value
                prevDomain = curveDomainProp.vector2Value; // Update prev value
            }



            // Setup Curve Field
            var curveField = new CurveField();
            UpdateCurveFieldRange();
            void UpdateCurveFieldRange()
            {
                Vector2 curveDomain = curveDomainProp.vector2Value;

                curveField.ranges = new Rect(curveDomain.x, 0, curveDomain.y - curveDomain.x, curveRange.y - curveRange.x);
            }
            curveField.RegisterValueChangedCallback(evt => { curveProp.animationCurveValue = curveField.value; serializedObject.ApplyModifiedProperties(); }); // Update prop
            curveField.TrackPropertyValue(curveProp, p => curveField.SetValueWithoutNotify(p.animationCurveValue)); // Update field
            curveField.SetValueWithoutNotify(curveProp.animationCurveValue); // Initialize field

            curveField.style.width = new StyleLength(Length.Percent(100));
            curveField.style.flexShrink = 0;

            // Prop Change Update Value
            curveField.TrackPropertyValue(curveDomainProp, p =>
            {
                UpdateCurveFieldRange();
            });


            // Keep it square: height tracks whatever width layout gives it
            curveField.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                float width = evt.newRect.width;
                if (width > 0f && !Mathf.Approximately(curveField.resolvedStyle.height, width))
                    curveField.style.height = width;
            });


            VisualElement root = new VisualElement();
            root.Add(curveField);
            root.Add(SoundEditor.GetLabeledElement(curveDomainField, "Distance"));
            return root;
        }
    }
}