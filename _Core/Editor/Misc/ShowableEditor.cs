
namespace OGT
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(Showable))]
    public class ShowableEditor : OGT.Editor, IHasHidableComponents
    {
        [field: NonSerialized]
        public bool AreComponentsHidden { get; set; }

        public IEnumerable<Type> GetHidableComponents()
        {
            yield return typeof(Animation);
        }

        protected override void NewOnInspectorGUI()
        {
            this.DrawDefaultInspector();

            var showable = this.target as Showable;
            showable.SetupAnimationComponent();

            if (Application.isPlaying)
            {
                GUILayout.Space(20);

                if (showable.ShowClip != null && GUILayout.Button("Show"))
                {
                    showable.Show();
                }

                if (showable.HideClip != null && GUILayout.Button("Hide"))
                {
                    showable.Hide();
                }
            }
        }
    }
}
