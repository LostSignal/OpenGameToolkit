//-----------------------------------------------------------------------
// <copyright file="TimelineEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Playables;

    [CustomEditor(typeof(PlayableDirector))]
    public class TimelineEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying && GUILayout.Button("Play"))
            {
                ((PlayableDirector)this.target).Play();
            }
        }
    }
}
