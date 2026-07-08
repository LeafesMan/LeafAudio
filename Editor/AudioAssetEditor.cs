using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LeafAudio.Editor
{
    /// <summary>
    /// Editor that draws default inspector and prevents multi-object editing
    /// </summary>
    [CustomEditor(typeof(AudioAsset), false)]
    public class AudioAssetEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();

            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            return root;
        }
    }
}