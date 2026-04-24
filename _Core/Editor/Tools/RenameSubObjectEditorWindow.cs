//-----------------------------------------------------------------------
// <copyright file="RenameSubObjectEditorWindow.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Linq;
    using UnityEditor;
    using UnityEditor.VersionControl;
    using UnityEngine;

    public class RenameSubObjectEditorWindow : EditorWindow
    {
        private string subObjectName = string.Empty;

        [MenuItem("Assets/OGT/Sub-Object/Rename Sub-Object", true, -105)]
        public static bool RenameSubAnimationClipToAnimatorValidator()
        {
            var subObject = Selection.objects.FirstOrDefault();
            var parentObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GetAssetPath(subObject));

            return Selection.objects.Length == 1 && subObject != null && parentObject != null && subObject != parentObject;
        }

        [MenuItem("Assets/OGT/Sub-Object/Rename Sub-Object", false, -105)]
        public static void RenameSubAnimationClipToAnimator()
        {
            var subObject = Selection.objects.FirstOrDefault();

            var window = (RenameSubObjectEditorWindow)EditorWindow.GetWindow(typeof(RenameSubObjectEditorWindow));
            window.titleContent = new GUIContent("Rename Sub-Object");
            window.subObjectName = subObject.name;
            window.minSize = new Vector2(300, 75);
            window.maxSize = new Vector2(300, 75);
            window.ShowPopup();
        }

        private void OnGUI()
        {
            this.subObjectName = EditorGUILayout.TextField("Sub-Object Name", this.subObjectName);

            if (GUILayout.Button("Rename"))
            {
                var subObject = Selection.objects.FirstOrDefault();
                var parentObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GetAssetPath(subObject));

                if (Provider.isActive)
                {
                    Provider.Checkout(parentObject, CheckoutMode.Asset);
                }

                subObject.name = this.subObjectName;
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(parentObject));
                AssetDatabase.Refresh();

                this.Close();
            }
        }
    }
}
