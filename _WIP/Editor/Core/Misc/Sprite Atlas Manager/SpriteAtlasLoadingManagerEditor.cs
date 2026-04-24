#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="SpriteAtlasLoadingManagerEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.U2D;

    [CustomEditor(typeof(SpriteAtlasLoadingManager))]
    public class SpriteAtlasLoadingManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //// if (GUILayout.Button("Find Atlases"))
            //// {
            ////     var spriteAtlasManager = this.target as SpriteAtlasLoadingManager;
            ////     spriteAtlasManager.Atlases.Clear();
            ////
            ////     var atlasGuids = AssetDatabase.FindAssets("t:SpriteAtlas");
            ////
            ////     foreach (var atlasGuid in atlasGuids)
            ////     {
            ////         string atlasPath = AssetDatabase.GUIDToAssetPath(atlasGuid);
            ////         var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            ////
            ////         // Figuring out if this is included in the build
            ////         var atlasSerializedObject = new SerializedObject(atlas);
            ////         var bindAsDefault = atlasSerializedObject.FindProperty("m_EditorData.bindAsDefault");
            ////         bool includeInBuild = bindAsDefault.boolValue;
            ////
            ////         if (includeInBuild == false)
            ////         {
            ////             spriteAtlasManager.Atlases.Add(new SpriteAtlasLoadingManager.Atlas(atlas.tag, atlasGuid));
            ////         }
            ////     }
            ////
            ////     EditorUtility.SetDirty(this.target);
            //// }
        }
    }
}
