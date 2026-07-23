using System.Linq;
using Mono.Cecil.Cil;
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
            VisualElement root = new VisualElement();
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
            var labeledCurveDomainField = SoundEditor.GetLabeledElement(curveDomainField, "Distance");


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



            root.Add(curveField);
            root.Add(labeledCurveDomainField);


            // Setup toggle curve
            // (Only if the Distance profile can be represented as a value)
            if ((target as DistanceProfile).CanShowAsValue)
            {
                SerializedProperty useCurveProp = serializedObject.FindProperty(nameof(DistanceProfile.useCurve));



                // Make Value Field
                FloatField valueField = new FloatField();
                valueField.RegisterValueChangedCallback(evt => UpdateCurveProp());
                valueField.TrackPropertyValue(curveProp, p => UpdateValueField());
                UpdateValueField();
                void UpdateCurveProp()
                {
                    if (useCurveProp.boolValue) return; // Dont lock if using curve
                    curveProp.animationCurveValue = new AnimationCurve(new Keyframe(0, valueField.value));
                    serializedObject.ApplyModifiedProperties();
                }
                void UpdateValueField()
                {
                    var animationCurve = curveProp.animationCurveValue;
                    valueField.value = animationCurve.keys[0].value;
                }
                VisualElement labeledValueField = SoundEditor.GetLabeledElement(valueField, "Value");


                // Make Toggle Field                
                VisualElement useCurveToggle = new PropertyField(useCurveProp, "");
                useCurveToggle = SoundEditor.GetLabeledElement(useCurveToggle, "Curve");
                UpdateCurveFieldsShown();
                root.TrackPropertyValue(useCurveProp, p => UpdateCurveFieldsShown());
                void UpdateCurveFieldsShown()
                {
                    DisplayStyle GetStyle(bool show) => show ? DisplayStyle.Flex : DisplayStyle.None;

                    curveField.style.display = GetStyle(useCurveProp.boolValue);
                    labeledCurveDomainField.style.display = GetStyle(useCurveProp.boolValue);
                    labeledValueField.style.display = GetStyle(!useCurveProp.boolValue);
                }

                root.Add(labeledValueField);
                root.Add(useCurveToggle);
            }



            return root;
        }
    }
}