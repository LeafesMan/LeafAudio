using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LeafAudio.Editor
{
    [CustomEditor(typeof(SpatialSettings))]
    public class SpatialSettingsEditor : UnityEditor.Editor
    {
        SerializedProperty maxDistanceProp;
        Action MaxDistanceChanged;

        public override VisualElement CreateInspectorGUI()
        {   // Grab Props and Vars from field
            BindableElement root = new BindableElement();

            maxDistanceProp = serializedObject.FindProperty(nameof(SpatialSettings.maxDistance));
            FloatField maxDistanceField = new FloatField("");
            maxDistanceField.BindProperty(maxDistanceProp);
            maxDistanceField.TrackPropertyValue(maxDistanceProp, p => MaxDistanceChanged?.Invoke());
            maxDistanceField.RegisterValueChangedCallback(evt => maxDistanceField.SetValueWithoutNotify(SpatialSettings.ValidateMaxDistance(evt.newValue)));
            VisualElement maxDistanceElement = Util.GetLabeledElement(maxDistanceField, "Max Distance", labelWidth: 110);

            // Set up doppler fields
            SerializedProperty dopplerProp = serializedObject.FindProperty(nameof(SpatialSettings.doppler));
            Slider dopplerSlider = new Slider("") { lowValue = SpatialSettings.DopplerRange.x, highValue = SpatialSettings.DopplerRange.y, style = { flexGrow = 3 } };
            dopplerSlider.BindProperty(dopplerProp);

            FloatField dopplerFloatField = new FloatField("") { style = { flexGrow = 0.9f, flexBasis = 0, marginRight = 7 } };
            dopplerFloatField.BindProperty(dopplerProp);
            dopplerFloatField.RegisterValueChangedCallback(evt => dopplerFloatField.SetValueWithoutNotify(SpatialSettings.ValidateDoppler(evt.newValue)));

            VisualElement dopplerElement = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
            dopplerElement.Add(dopplerFloatField);
            dopplerElement.Add(dopplerSlider);
            dopplerElement = Util.GetLabeledElement(dopplerElement, "Doppler", labelWidth: 110);
            Util.ShowIfCondition(serializedObject, dopplerElement, () => serializedObject.FindProperty(nameof(SpatialSettings.useDoppler)).boolValue);


            root.Add(maxDistanceElement);
            root.Add(dopplerElement);
            root.Add(GetCurveElement(nameof(SpatialSettings.attenuation), canBeValue: false));
            root.Add(GetCurveElement(nameof(SpatialSettings.spatial), canBeValue: false));
            root.Add(GetCurveElement(nameof(SpatialSettings.spread), canBeValue: true));
            root.Add(GetCurveElement(nameof(SpatialSettings.reverb), canBeValue: true, 1.1f));
            root.Add(GetDataSettingsElement());

            root.Bind(serializedObject);

            return root;
        }

        VisualElement GetDataSettingsElement()
        {
            Foldout root = new Foldout();
            root.text = "Data Settings";
            root.viewDataKey = nameof(SpatialSettingsEditor) + "DataSettings";

            float labelWidth = 90;
            root.Add(Util.GetPropField(nameof(SpatialSettings.useAttenuation), "Attenuation", labelWidth));
            root.Add(Util.GetPropField(nameof(SpatialSettings.useSpatial), "Spatial", labelWidth));
            root.Add(Util.GetPropField(nameof(SpatialSettings.useDoppler), "Doppler", labelWidth));
            root.Add(Util.GetPropField(nameof(SpatialSettings.spreadType), "Spread", labelWidth));
            root.Add(Util.GetPropField(nameof(SpatialSettings.reverbType), "Reverb", labelWidth));

            return root;
        }
        VisualElement GetCurveElement(string var, bool canBeValue, float range = 1)
        {
            // Grab the curve prop and make an element for it
            SerializedProperty curveProp = serializedObject.FindProperty(var);
            VisualElement root = new VisualElement() { style = { flexDirection = FlexDirection.Column } };

            // Setup Max Distance Field
            // curveDomainField.Query<VisualElement>(classes: "unity-base-field__label--with-dragger").ForEach(element => element.RegisterCallback<MouseUpEvent>(e => UpdateDomainValue()));
            //curveDomainField.RegisterCallback<BlurEvent>(e => UpdateDomainValue());
            /*void UpdateDomainValue()
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
            }*/



            // Setup Curve Field
            VisualElement curveElement = new VisualElement() { style = { marginTop = 5 } };
            Label curveLabel = new Label(Util.CaptializeFirstLetter(var)) { style = { fontSize = 12, backgroundColor = Color.gray1, flexGrow = 0, flexShrink = 0, borderTopLeftRadius = 3, borderTopRightRadius = 3, marginBottom = 0, paddingBottom = 0, borderBottomWidth = 0, paddingLeft = 3 } };
            CurveField curveField = new CurveField() { name = var, style = { width = new StyleLength(Length.Percent(100)), flexShrink = 0, marginLeft = 0, marginTop = 0, paddingTop = 0 } };
            curveField.Q<VisualElement>(className: "unity-curve-field__input").style.marginTop = 0;

            curveField.RegisterCallback<GeometryChangedEvent>(evt => // Maintains Square CurveField
            {
                float width = evt.newRect.width;
                if (width > 0f && !Mathf.Approximately(curveField.resolvedStyle.height, width)) curveField.style.height = width;
            });
            AnimationCurve Denormalize(AnimationCurve normalized)
            {
                AnimationCurve denormalized = new AnimationCurve();

                float maxDistance = maxDistanceProp.floatValue;
                for (int i = 0; i < normalized.length; i++)
                {
                    Keyframe k = normalized[i];
                    k.time = k.time * maxDistance;
                    k.inTangent /= maxDistance;
                    k.outTangent /= maxDistance;
                    denormalized.AddKey(k);
                }
                return denormalized;
            }
            AnimationCurve Normalize(AnimationCurve denormalized)
            {
                AnimationCurve normalized = new AnimationCurve();

                float maxDistance = maxDistanceProp.floatValue;
                for (int i = 0; i < denormalized.length; i++)
                {
                    Keyframe k = denormalized[i];
                    k.time = k.time / maxDistance;
                    k.inTangent *= maxDistance;
                    k.outTangent *= maxDistance;
                    normalized.AddKey(k);
                }

                return normalized;
            }
            void ApplyPropertyToCurveField()
            {
                curveField.ranges = new Rect(0, 0, maxDistanceProp.floatValue, range);
                curveField.SetValueWithoutNotify(Denormalize(curveProp.animationCurveValue));
            }
            void ApplyCurveFieldToProperty()
            {
                curveProp.animationCurveValue = Normalize(curveField.value);
                serializedObject.ApplyModifiedProperties();
            }

            ApplyPropertyToCurveField();
            curveField.TrackPropertyValue(curveProp, p => ApplyPropertyToCurveField());
            curveField.RegisterValueChangedCallback(evt => ApplyCurveFieldToProperty());
            MaxDistanceChanged += ApplyPropertyToCurveField;


            curveElement.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Copy", _ => EditorGUIUtility.systemCopyBuffer = "AC:" + JsonUtility.ToJson(new CurveClipboardData(curveProp.animationCurveValue)));

                evt.menu.AppendAction("Paste", _ =>
                {
                    var clip = EditorGUIUtility.systemCopyBuffer;
                    if (!clip.StartsWith("AC:")) return;
                    var wrapper = JsonUtility.FromJson<CurveClipboardData>(clip.Substring(3));
                    var normalizedCurve = wrapper.ToCurve();
                    curveProp.animationCurveValue = normalizedCurve; // triggers your change callback
                    serializedObject.ApplyModifiedProperties();
                }, clip => EditorGUIUtility.systemCopyBuffer.StartsWith("AC:")
                    ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));



            curveElement.Add(curveLabel);
            curveElement.Add(curveField);
            root.Add(curveElement);


            // Setup Display as Value
            if (canBeValue)
            {
                SerializedProperty curveType = serializedObject.FindProperty(var + "Type");


                // Make Value Field
                FloatField valueField = new FloatField();
                valueField.RegisterValueChangedCallback(evt => UpdateCurveProp());
                valueField.TrackPropertyValue(curveProp, p => UpdateValueField());
                UpdateValueField();
                void UpdateCurveProp()
                {
                    if (curveType.enumValueIndex != (int)SpatialSettings.CurveValueType.Value) return; // Dont lock if not using value
                    curveProp.animationCurveValue = new AnimationCurve(new Keyframe(0, valueField.value));
                    serializedObject.ApplyModifiedProperties();
                }
                void UpdateValueField()
                {
                    var animationCurve = curveProp.animationCurveValue;
                    valueField.value = animationCurve.keys[0].value;
                }
                VisualElement valueElement = Util.GetLabeledElement(valueField, Util.CaptializeFirstLetter(var));

                root.Add(valueElement);
                // Show Curve if on Curve Mode
                // Show Value if on Value Mode
                Util.ShowIfCondition(serializedObject, curveElement, () => curveType.enumValueIndex == (int)SpatialSettings.CurveValueType.Curve);
                Util.ShowIfCondition(serializedObject, valueElement, () => curveType.enumValueIndex == (int)SpatialSettings.CurveValueType.Value);
            }
            else Util.ShowIfCondition(serializedObject, curveElement, () => serializedObject.FindProperty("use" + Util.CaptializeFirstLetter(var)).boolValue);


            return root;
        }
    }



    [Serializable]
    internal struct CurveClipboardData
    {
        public float[] times;
        public float[] values;
        public float[] inTangents;
        public float[] outTangents;

        public CurveClipboardData(AnimationCurve curve)
        {
            int n = curve.length;
            times = new float[n];
            values = new float[n];
            inTangents = new float[n];
            outTangents = new float[n];
            for (int i = 0; i < n; i++)
            {
                var k = curve[i];
                times[i] = k.time;
                values[i] = k.value;
                inTangents[i] = k.inTangent;
                outTangents[i] = k.outTangent;
            }
        }

        public AnimationCurve ToCurve()
        {
            var keys = new Keyframe[times.Length];
            for (int i = 0; i < times.Length; i++)
                keys[i] = new Keyframe(times[i], values[i], inTangents[i], outTangents[i]);
            return new AnimationCurve(keys);
        }
    }
}