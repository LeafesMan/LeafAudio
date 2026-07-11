using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using UnityEditorInternal;

namespace LeafAudio.Editor
{
    /// <summary>
    /// Draws an audio definition. Conceals uneeded vars and allows playing any individual spec or group of specs.
    /// </summary>
    [CustomEditor(typeof(Sound))]
    public class SoundEditor : UnityEditor.Editor
    {
        class TrackedInt
        {
            public TrackedInt(int val) => this.val = val;
            int val;
            public int Value { get => val; set { val = value; Changed?.Invoke(val); } }
            public event Action<int> Changed;
        }

        SerializedProperty variantsProp;
        SerializedProperty groupProp;
        SerializedProperty modeProp;

        ListView variantsListView;

        public override VisualElement CreateInspectorGUI()
        {

            // Cant have field at class level property drawers share those
            TrackedInt selectedIndex = new(-1);

            // Grab props
            variantsProp = serializedObject.FindProperty("weightedVariants");
            groupProp = serializedObject.FindProperty("mixerGroup");
            modeProp = serializedObject.FindProperty("selectionMode");


            // Create root container and specs container
            VisualElement root = new VisualElement();

            variantsListView = new ListView()
            {
                showBorder = true,
                showAddRemoveFooter = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderMode = ListViewReorderMode.Animated,
                reorderable = true,
                headerTitle = "Variants"
            };
            variantsListView.makeItem += MakeVariantUI;
            variantsListView.bindItem += BindVariantUI;
            variantsListView.BindProperty(variantsProp);


            // Populate Root
            root.Add(GetScriptField());
            root.Add(new PropertyField(serializedObject.FindProperty("mixerGroup"), "Mixer Group"));
            root.Add(GetWeightedToggle());
            root.Add(variantsListView);
            root.Add(GetTestButton(selectedIndex));

            // Initialize certain things


            return root;
        }


        private void BindVariantUI(VisualElement element, int index)
        {
            SerializedProperty weightedVariantProp = variantsProp.GetArrayElementAtIndex(index);
            SerializedProperty variantProp = weightedVariantProp.FindPropertyRelative("item");

            element.Q<ObjectField>("clip").BindProperty(variantProp.FindPropertyRelative("clip"));
            element.Q<FloatField>("volume").BindProperty(variantProp.FindPropertyRelative("volume"));
            element.Q<FloatField>("volumeVariation").BindProperty(variantProp.FindPropertyRelative("volumeVariation"));
            element.Q<FloatField>("pitch").BindProperty(variantProp.FindPropertyRelative("pitch"));
            element.Q<FloatField>("pitchVariation").BindProperty(variantProp.FindPropertyRelative("pitchVariation"));
            element.Q<FloatField>("weight").BindProperty(weightedVariantProp.FindPropertyRelative("weight"));

        }

        private VisualElement MakeVariantUI()
        {
            float labelWidth = 69;

            // SerializedProperty weightedAudioSpecProp = variantsProp.GetArrayElementAtIndex();
            // SerializedProperty audioSpecProp = weightedAudioSpecProp.FindPropertyRelative("item");
            // SerializedProperty weightProp = weightedAudioSpecProp.FindPropertyRelative("weight");

            // Container
            var container = new VisualElement(); // Can change this back to foldout if i desire
            container.style.paddingLeft = new StyleLength(15); // Add padding
            container.style.paddingRight = new StyleLength(5); // Add padding
            container.style.marginBottom = new StyleLength(5); // Add padding
            container.style.borderBottomLeftRadius = 10;
            container.style.borderBottomRightRadius = 10;
            container.style.borderTopLeftRadius = 10;
            container.style.borderTopRightRadius = 10;
            // container.RegisterCallback<MouseDownEvent>(evt =>
            // {   // Deselect on clicking same item otherwise select item

            //     int myIndex = root.IndexOf(container);

            //     if (myIndex == selectedIndex.Value) selectedIndex.Value = -1;
            //     else selectedIndex.Value = myIndex;


            //     // Have to find the element ourselves because unity occasionally performs a deep copy of all VisualElements rebuilding the tree
            //     // so if I hold a reference to a visual element in an event, and throw that event, the event will act on the visual element in the old tree
            //     var rootArray = root.Children().ToArray();
            //     for (int i = 0; i < root.childCount; i++)
            //     {
            //         if (selectedIndex.Value == i) rootArray[i].style.backgroundColor = new Color(.17f, .32f, .56f);
            //         else rootArray[i].style.backgroundColor = new Color(0.27f, 0.27f, 0.27f);
            //     }
            // });


            // Weight Field
            var weightField = new FloatField("Weight") { name = "weight", style = { display = GetWeightFieldDisplayStyle } };

            var clipField = new ObjectField("Clip") { name = "clip", objectType = typeof(AudioClip) };
            var title = new Label() { style = { unityFontStyleAndWeight = FontStyle.Bold, fontSize = 15, marginTop = 3f, overflow = Overflow.Hidden } };

            var volumeElements = GetVariedField("volume", new Vector2(0, 1), 0.5f, labelWidth);
            var pitchElements = GetVariedField("pitch", new Vector2(-3, 3), 1f, labelWidth);



            container.Add(title);
            container.Add(clipField);
            container.Add(volumeElements);
            container.Add(pitchElements);
            container.Add(weightField);



            // Add the Element to the given container
            return container;
        }
        VisualElement GetScriptField()
        {
            var scriptField = new ObjectField("Script")
            {
                objectType = typeof(MonoScript),
                value = MonoScript.FromScriptableObject((ScriptableObject)target)
            };

            scriptField.SetEnabled(false);
            return scriptField;
        }
        VisualElement GetWeightedToggle()
        {
            PropertyField weightToggle = new PropertyField(modeProp, "Selection Mode");

            // Initialize and update weights shown
            UpdateWeightFieldsShown();
            weightToggle.RegisterValueChangeCallback((evt) => UpdateWeightFieldsShown());

            return weightToggle;

            void UpdateWeightFieldsShown() => variantsListView.Query<FloatField>().Name("weight").ToList().ForEach(element => element.style.display = GetWeightFieldDisplayStyle);
        }
        DisplayStyle GetWeightFieldDisplayStyle => ((Sound.SelectionMode)modeProp.enumValueIndex == Sound.SelectionMode.WeightedRandom) ? DisplayStyle.Flex : DisplayStyle.None;
        Button GetTestButton(TrackedInt selectedIndex)
        {
            Button button = new Button
            {
                text = "Test",
                style = { height = 20, marginTop = 5 }
            };
            // Disable Test Button if there are no clips asd
            button.TrackPropertyValue(variantsProp, (evt) => button.SetEnabled(variantsProp.arraySize != 0));

            button.RegisterCallback<ClickEvent>(
                (evt) =>
                {
                    Sound audio = target as Sound;
                    SoundVariant spec;
                    if (selectedIndex.Value == -1) spec = audio.SelectVariant();
                    else spec = variantsProp.GetArrayElementAtIndex(selectedIndex.Value).FindPropertyRelative("item").boxedValue as SoundVariant;

                    SoundTester.Test(spec.GetClip(), spec.GetVolume(), spec.GetPitch());
                }
            );

            return button;
        }
        VisualElement GetVariedField(string var, Vector2 valueRange, float varyRange, float labelWidth)
        {
            var capitalizedVar = char.ToUpper(var[0]) + var.Substring(1);

            VisualElement box = new VisualElement();


            VisualElement labeledValueField = new FloatField("Value") { name = var };
            VisualElement labeledVariationField = new FloatField("Variation") { name = var + "Variation" };


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
}