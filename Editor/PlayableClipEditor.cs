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
        if (GUILayout.Button(new GUIContent("Test Audio", "Plays the clip as it would be played in game.")))
            playableClipScript.Test(playableClipScript.GetSpecs());

        base.OnInspectorGUI();
    }
}
