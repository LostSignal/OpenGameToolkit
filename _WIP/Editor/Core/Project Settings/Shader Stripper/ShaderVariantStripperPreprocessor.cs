//-----------------------------------------------------------------------
// <copyright file="ShaderVariantStripperPreprocessor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR

namespace OGT
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Rendering;
    using UnityEngine;
    using UnityEngine.Rendering;

    ////
    //// https://gist.github.com/yasirkula/d8fa2fb5f22aefcc7a232f6feeb91db7
    ////
    public class ShaderVariantStripperPreprocessor : IPreprocessShaders
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        public int callbackOrder => 1;

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> inputData)
        {
            var settings = ProjectSettingsShaderStripper.Instance;

            if (settings == null || settings.EnableShaderStripping == false)
            {
                return;
            }

            string shaderPath = AssetDatabase.GetAssetPath(shader);
            string shaderName = shader.name;

            // Testing if it's an essential shader
            bool isEssential = false;

            foreach (var startsWith in settings.EssentialShadersStartWith)
            {
                isEssential |= shaderName.StartsWith(startsWith);
            }

            foreach (var equals in settings.EssentialShadersEqual)
            {
                isEssential |= shaderName == equals;
            }

            // Testing if it's in the approved folder list
            bool isInApprovedFolder = false;

            foreach (var folder in settings.ApprovedFolders)
            {
                isInApprovedFolder |= shaderPath.StartsWith($"{folder}/");
            }

            // If it's in neither, then don't process it
            if (isEssential == false && isInApprovedFolder == false)
            {
                inputData.Clear();
                return;
            }

            // Skipping any deffered related shaders (our games are forward)
            if (settings.IsForwardRenderer)
            {
                if (snippet.passType == PassType.Deferred)
                {
                    inputData.Clear();
                    return;
                }
            }

            // Removing any variants we don't want
            for (int i = inputData.Count - 1; i >= 0; --i)
            {
                foreach (ShaderKeyword keywordToSkip in settings.VarientsToSkip)
                {
                    if (inputData[i].shaderKeywordSet.IsEnabled(keywordToSkip))
                    {
                        inputData.RemoveAt(i);
                        break;
                    }
                }
            }

            // Printing off all shaders that will be processed
            for (int i = 0; i < inputData.Count; i++)
            {
                var keywords = inputData[i].shaderKeywordSet.GetShaderKeywords();

                if (keywords.Length > 0)
                {
                    Logger.Log($"ShaderVariantStripper: Processing {shaderName}, Path = {shaderPath}, Keywords = {string.Join(", ", keywords.Select(x => x.name))}");
                }
            }
        }
    }
}

#endif
