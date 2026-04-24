//-----------------------------------------------------------------------
// <copyright file="BuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build.Profile;
    using UnityEditor.VersionControl;
    using UnityEngine;

    public abstract class BuildStep : ScriptableObject
    {
        public const string BuildStepMenuPath = "Assets/OGT/Build Steps/";

        public static readonly OGTLogger Logger = new OGTLogger("Build Step");

        public abstract string Name { get; }

        [SerializeField] private int order;

        public int Order => this.order;

        public static bool IsBuildProfileSelected() => Selection.objects.Length == 1 && Selection.objects.First() is BuildProfile;

        public static void AddBuildStep<T>(string name)
            where T : BuildStep, new()
        {
            var parentScriptableObject = Selection.objects.FirstOrDefault() as ScriptableObject;

            if (Provider.isActive)
            {
                Provider.Checkout(parentScriptableObject, CheckoutMode.Asset);
            }

            var buildStep = ScriptableObject.CreateInstance<T>();
            buildStep.name = name;

            AssetDatabase.AddObjectToAsset(buildStep, parentScriptableObject);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(parentScriptableObject));
            AssetDatabase.Refresh();
        }

        [MenuItem(BuildStepMenuPath + "Delete Build Step", true)]
        public static bool DeleteBuildStepValidate() => Selection.objects.Length == 1 && Selection.objects.First() is BuildStep;

        [MenuItem(BuildStepMenuPath + "Delete Build Step", false)]
        public static void DeleteBuildStepExecute()
        {
            var obj = Selection.objects.FirstOrDefault();
            var objParent = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GetAssetPath(obj));

            if (Provider.isActive)
            {
                Provider.Checkout(objParent, CheckoutMode.Asset);
            }

            AssetDatabase.RemoveObjectFromAsset(obj);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(objParent));
        }
    }
}
