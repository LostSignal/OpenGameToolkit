//-----------------------------------------------------------------------
// <copyright file="BuildProfileExtender.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using System.Collections.Generic;

#if UNITY_EDITOR
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build.Profile;
    using UnityEditor.VersionControl;
#endif

    using UnityEngine;

    public class BuildProfileExtender : ScriptableObject
    {
#if UNITY_EDITOR
        private const string AddBuildStepsMenuPath = "Assets/OGT/Add Build Steps";
        private const string DeleteBuildStepsMenuPath = "Assets/OGT/Delete Build Steps";
#endif

        public enum Environment
        {
            Development,
            Staging,
            Production,
        }

        [SerializeField] private Environment environment = Environment.Development;
        [SerializeField] private List<BuildStep> buildSteps = new List<BuildStep>();

        public Environment CurrentEnvironment => this.environment;

        public IReadOnlyList<BuildStep> BuildSteps => this.buildSteps;

#if UNITY_EDITOR

        public static UnityEditor.Build.Profile.BuildProfile GetSelectedBuildProfile()
        {
            if (Selection.objects.Length == 1 && Selection.objects.First() is UnityEditor.Build.Profile.BuildProfile)
            {
                return (UnityEditor.Build.Profile.BuildProfile)Selection.objects.First();
            }

            return null;
        }

        [MenuItem(AddBuildStepsMenuPath, true)]
        private static bool AddBuildStepsValidate(MenuCommand command)
        {
            var buildProfile = GetSelectedBuildProfile();
            return buildProfile != null && GetBuildProfileExtender(buildProfile) == null;
        }

        [MenuItem(AddBuildStepsMenuPath, false)]
        private static void AddBuildSteps(MenuCommand command)
        {
            var buildProfile = GetSelectedBuildProfile();
            var buildProfileAssetPath = AssetDatabase.GetAssetPath(buildProfile);
            var parentAsset = AssetDatabase.LoadMainAssetAtPath(buildProfileAssetPath);

            if (parentAsset == null)
            {
                return;
            }

            if (Provider.isActive)
            {
                Provider.Checkout(parentAsset, CheckoutMode.Asset);
            }

            var buildProfileExtender = CreateInstance<BuildProfileExtender>();
            buildProfileExtender.name = "Build Steps";

            AssetDatabase.AddObjectToAsset(buildProfileExtender, buildProfile);
            AssetDatabase.ImportAsset(buildProfileAssetPath);
            AssetDatabase.SaveAssets();
        }

        [MenuItem(DeleteBuildStepsMenuPath, true)]
        private static bool DeleteBuildStepsValidate(MenuCommand command)
        {
            var buildProfile = GetSelectedBuildProfile();
            return buildProfile != null && GetBuildProfileExtender(buildProfile) != null;
        }

        [MenuItem(DeleteBuildStepsMenuPath, false)]
        private static void DeleteBuildSteps(MenuCommand command)
        {
            var buildProfile = GetSelectedBuildProfile();
            var buildProfileExtender = GetBuildProfileExtender(buildProfile);

            if (buildProfileExtender == null)
            {
                return;
            }

            var buildProfileAssetPath = AssetDatabase.GetAssetPath(buildProfile);
            var parentAsset = AssetDatabase.LoadMainAssetAtPath(buildProfileAssetPath);

            if (parentAsset == null)
            {
                return;
            }

            if (Provider.isActive)
            {
                Provider.Checkout(parentAsset, CheckoutMode.Asset);
            }

            AssetDatabase.RemoveObjectFromAsset(buildProfileExtender);
            Object.DestroyImmediate(buildProfileExtender, true);
            AssetDatabase.ImportAsset(buildProfileAssetPath);
            AssetDatabase.SaveAssets();
        }

        private static BuildProfileExtender GetBuildProfileExtender(BuildProfile buildProfile)
        {
            var path = AssetDatabase.GetAssetPath(buildProfile);

            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return AssetDatabase
                .LoadAllAssetsAtPath(path)
                .OfType<BuildProfileExtender>()
                .FirstOrDefault();
        }
#endif
    }
}
