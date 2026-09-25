//-----------------------------------------------------------------------
// <copyright file="BuildProfileExtenderEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [UnityEditor.CustomEditor(typeof(BuildProfileExtender))]
    public class BuildProfileExtenderEditor : UnityEditor.Editor
    {
        private static List<Type> buildStepTypes;

        private SerializedProperty environmentProperty;
        private SerializedProperty buildStepsProperty;
        private List<BuildStep> existingBuildSteps;

        private void OnEnable()
        {
            this.environmentProperty = this.serializedObject.FindProperty("environment");
            this.buildStepsProperty = this.serializedObject.FindProperty("buildSteps");

            if (buildStepTypes == null)
            {
                buildStepTypes = TypeCache
                    .GetTypesDerivedFrom<BuildStep>()
                    .Where(t => t.IsAbstract == false)
                    .OrderBy(GetFriendlyTypeName)
                    .ToList();
            }

            this.existingBuildSteps = FindAllExistingBuildSteps();
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.environmentProperty);
            EditorGUILayout.PropertyField(this.buildStepsProperty, true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Create New", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Create a new build step asset and add it to this build profile.", MessageType.None);

            foreach (var buildStepType in buildStepTypes)
            {
                if (GUILayout.Button(GetFriendlyTypeName(buildStepType)))
                {
                    this.CreateAndAddBuildStep(buildStepType);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Add Existing", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Add an existing build step asset from project or packages to this build profile.", MessageType.None);

            if (this.existingBuildSteps.Count == 0)
            {
                EditorGUILayout.LabelField("No build step assets found.");
            }
            else
            {
                foreach (var buildStep in this.existingBuildSteps)
                {
                    bool alreadyAdded = this.ContainsBuildStep(buildStep);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(buildStep, typeof(BuildStep), false);

                    using (new EditorGUI.DisabledScope(alreadyAdded))
                    {
                        if (GUILayout.Button("Add", GUILayout.Width(60)))
                        {
                            this.AddBuildStepReference(buildStep);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            this.serializedObject.ApplyModifiedProperties();
        }

        private static string GetFriendlyTypeName(Type type)
        {
            var name = type.Name.EndsWith("BuildStep", StringComparison.Ordinal)
                ? type.Name.Substring(0, type.Name.Length - "BuildStep".Length)
                : type.Name;

            return ObjectNames.NicifyVariableName(name);
        }

        private static List<BuildStep> FindAllExistingBuildSteps()
        {
            var allBuildSteps = new List<BuildStep>();
            var seenPaths = new HashSet<string>();

            foreach (var buildStepType in buildStepTypes)
            {
                foreach (var guid in AssetDatabase.FindAssets($"t:{buildStepType.Name}"))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);

                    if (string.IsNullOrEmpty(path) || seenPaths.Add(path) == false)
                    {
                        continue;
                    }

                    var buildStep = AssetDatabase.LoadAssetAtPath(path, buildStepType) as BuildStep;

                    if (buildStep != null)
                    {
                        allBuildSteps.Add(buildStep);
                    }
                }
            }

            return allBuildSteps
                .OrderBy(s => s.name)
                .ThenBy(s => s.GetType().Name)
                .ToList();
        }

        private bool ContainsBuildStep(BuildStep buildStep)
        {
            for (int i = 0; i < this.buildStepsProperty.arraySize; i++)
            {
                var existing = this.buildStepsProperty.GetArrayElementAtIndex(i).objectReferenceValue as BuildStep;
                if (existing == buildStep)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddBuildStepReference(BuildStep buildStep)
        {
            if (buildStep == null || this.ContainsBuildStep(buildStep))
            {
                return;
            }

            int index = this.buildStepsProperty.arraySize;
            this.buildStepsProperty.InsertArrayElementAtIndex(index);
            this.buildStepsProperty.GetArrayElementAtIndex(index).objectReferenceValue = buildStep;

            this.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(this.target);
        }

        private void CreateAndAddBuildStep(Type buildStepType)
        {
            var defaultName = $"{GetFriendlyTypeName(buildStepType)}.asset";
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Build Step",
                defaultName,
                "asset",
                "Choose where to save the new build step asset.");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var buildStep = ScriptableObject.CreateInstance(buildStepType) as BuildStep;

            if (buildStep == null)
            {
                return;
            }

            AssetDatabase.CreateAsset(buildStep, path);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();

            if (this.existingBuildSteps.Any(s => s == buildStep) == false)
            {
                this.existingBuildSteps.Add(buildStep);
                this.existingBuildSteps = this.existingBuildSteps
                    .OrderBy(s => s.name)
                    .ThenBy(s => s.GetType().Name)
                    .ToList();
            }

            this.AddBuildStepReference(buildStep);
            EditorGUIUtility.PingObject(buildStep);
        }
    }
}
