/*
 * Auth: Ian
 * 
 * Proj: SpaceS
 * 
 * Desc: 
 * 
 * Date: /24
 */
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(RandomAudioSet))]
public class RandomAudioSetEditor : Editor
{
    [SerializeField] VisualTreeAsset uxml;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        SerializedObject testClip = new SerializedObject(target);
        RandomAudioSet script = (RandomAudioSet)target;

        Button testButton = new Button();
        testButton.text = "Test Random Clip";
        testButton.style.height = 36;
        testButton.style.fontSize = 18;
        testButton.clicked += () => { AudioManager.Test(script.GetAudioData()); };


        root.Add(testButton);
        root.Add(new PropertyField(testClip.FindProperty("potentialClipSpecs")));

        // Return the finished Inspector UI.
        return root;
    }
}