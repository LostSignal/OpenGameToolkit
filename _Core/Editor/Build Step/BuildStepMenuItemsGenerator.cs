//-----------------------------------------------------------------------
// <copyright file="BuildStepMenuItemsGenerator.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public static class BuildStepMenuItemsGenerator
    {
        [GenerateMenuItems]
        public static void GenerateBuildStepMenuItems(List<MenuItemData> menuItems)
        {
            foreach (var postBuildStepType in TypeCache.GetTypesDerivedFrom<PostBuildStep>().Where(x => x.IsAbstract == false))
            {
                var buildStepName = GetBuildStepName(postBuildStepType);
                var buildStepNameNoSpace = buildStepName.Replace(" ", string.Empty);

                menuItems.Add(new MenuItemData
                {
                    MenuPath = $"{PostBuildStep.PostBuildStepMenuPath}{buildStepName}",
                    MethodName = $"Add{buildStepNameNoSpace}",
                    MethodBody = $"OGT.BuildStep.AddBuildStep<{postBuildStepType.FullName}>(\"{buildStepName}\");",
                    ValidateBody = "return OGT.BuildStep.IsBuildProfileSelected();",
                });
            }

            foreach (var preBuildStepType in TypeCache.GetTypesDerivedFrom<PreBuildStep>().Where(x => x.IsAbstract == false))
            {
                var buildStepName = GetBuildStepName(preBuildStepType);
                var buildStepNameNoSpace = buildStepName.Replace(" ", string.Empty);

                menuItems.Add(new MenuItemData
                {
                    MenuPath = $"{PreBuildStep.PreBuildStepMenuPath}{buildStepName}",
                    MethodName = $"Add{buildStepNameNoSpace}",
                    MethodBody = $"OGT.BuildStep.AddBuildStep<{preBuildStepType.FullName}>(\"{buildStepName}\");",
                    ValidateBody = "return OGT.BuildStep.IsBuildProfileSelected();",
                });
            }
        }

        private static string GetBuildStepName(Type type)
        {
            var instance = ScriptableObject.CreateInstance(type) as BuildStep;
            string name = instance?.Name ?? "ERROR!!!";
            ScriptableObject.DestroyImmediate(instance);
            return name;
        }
    }
}
