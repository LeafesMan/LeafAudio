/*
 * Auth: Ian
 * 
 * Proj: Audio
 * 
 * Date: 10/22/24
 * 
 * Desc: Editor that draws default inspector and prevents multi-object editing
 */
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(AudioAsset), false)]
public class AudioAssetEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new();

        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        return root;
    }
}