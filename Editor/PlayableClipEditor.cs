/*
 *  Name: Ian
 *
 *  Proj: Audio Library
 *
 *  Date: 7/27/23 
 *  
 *  Desc: Allows for testing Playable Clips with a button.
 */

using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(PlayableClip), true)]
public class PlayableClipEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlayableClip playableClipScript = (PlayableClip)target;

        //Test Clip Button
        if (GUILayout.Button("Test Audio"))
        {
            Debug.Log("Testing Clip!");
            playableClipScript.Test();
        }

        base.OnInspectorGUI();
    }
}
