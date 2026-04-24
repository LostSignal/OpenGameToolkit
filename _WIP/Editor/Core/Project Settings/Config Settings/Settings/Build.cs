//-----------------------------------------------------------------------
// <copyright file="Build.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Settings
{
    using System.Collections.Generic;
    using UnityEditor;

    public class Build : Settings
    {
        public List<string> Defines { get; set; }
        public bool? BuildPlayerContentOnBuild { get; set; }
        public bool? IsDevelopmentBuild { get; set; }
        public bool? BuildInStrictMode { get; set; }
        public bool? ScriptDebugging { get; set; }
        public bool? DeepProfilingSupport { get; set; }
        public bool? AutoconnectProfiler { get; set; }

        public override void ApplySettingOnBuildStarted()
        {
            if (this.BuildPlayerContentOnBuild != true)
            {
                return;
            }

            UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent();
        }

        public override BuildPlayerOptions ApplyBuildPlayerOptions(BuildPlayerOptions options)
        {
            if (this.IsDevelopmentBuild != null)
            {
                if (this.IsDevelopmentBuild.Value)
                {
                    options.options |= BuildOptions.Development;
                }
                else
                {
                    options.options &= ~BuildOptions.Development;
                }
            }

            if (this.BuildInStrictMode != null)
            {
                if (this.BuildInStrictMode.Value)
                {
                    options.options |= BuildOptions.StrictMode;
                }
                else
                {
                    options.options &= ~BuildOptions.StrictMode;
                }
            }

            if (this.ScriptDebugging != null)
            {
                if (this.ScriptDebugging.Value)
                {
                    options.options |= BuildOptions.AllowDebugging;
                }
                else
                {
                    options.options &= ~BuildOptions.AllowDebugging;
                }
            }

            if (this.DeepProfilingSupport != null)
            {
                if (this.DeepProfilingSupport.Value)
                {
                    options.options |= BuildOptions.EnableDeepProfilingSupport;
                }
                else
                {
                    options.options &= ~BuildOptions.EnableDeepProfilingSupport;
                }
            }

            if (this.AutoconnectProfiler != null)
            {
                if (this.AutoconnectProfiler.Value)
                {
                    options.options |= BuildOptions.ConnectWithProfiler;
                }
                else
                {
                    options.options &= ~BuildOptions.ConnectWithProfiler;
                }
            }

            return options;
        }
    }
}
