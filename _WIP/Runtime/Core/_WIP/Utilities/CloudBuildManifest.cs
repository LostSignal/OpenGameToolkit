//-----------------------------------------------------------------------
// <copyright file="CloudBuildManifest.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.CloudBuild
{
    using System.Collections.Generic;
    using System.IO;
    using OGT;
    using UnityEngine;

    public class CloudBuildManifest
    {
        private readonly Dictionary<string, object> dictionary;
        private readonly string json;

        private CloudBuildManifest(string json)
        {
            this.json = json;
            this.dictionary = JsonUtil.Deserialize<Dictionary<string, object>>(json);
        }

        public string CloudBuildTargetName
        {
            get { return this.GetString("cloudBuildTargetName"); }
        }

        public int BuildNumber
        {
            get { return this.GetInt("buildNumber", -1); }
        }

        public string ScmCommitId
        {
            get { return this.GetString("scmCommitId"); }
        }

        public string ScmBranch
        {
            get { return this.GetString("scmBranch"); }
        }

        public string ProjectId
        {
            get { return this.GetString("projectId"); }
        }

        public string BundleId
        {
            get { return this.GetString("bundleId"); }
        }

        public string UnityVersion
        {
            get { return this.GetString("unityVersion"); }
        }

        public static CloudBuildManifest Find()
        {
            var resourcesManifest = Resources.Load<TextAsset>("UnityCloudBuildManifest.json");
            var buildManifestJson = Path.Combine(Directory.GetCurrentDirectory(), "build_manifest.json");

            if (resourcesManifest != null)
            {
                return new CloudBuildManifest(resourcesManifest.text);
            }
            else if (File.Exists(buildManifestJson))
            {
                return new CloudBuildManifest(File.ReadAllText(buildManifestJson));
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            return this.json;
        }

        private string GetString(string key, string defaultValue = "")
        {
            if (this.dictionary.TryGetValue(key, out object value))
            {
                return value as string;
            }
            else
            {
                return defaultValue;
            }
        }

        private int GetInt(string key, int defaultValue = 0)
        {
            if (this.dictionary.TryGetValue(key, out object value))
            {
                if (value is string stringValue)
                {
                    if (int.TryParse(stringValue, out int result))
                    {
                        return result;
                    }
                    else
                    {
                        return defaultValue;
                    }
                }
                else if (value is long longValue)
                {
                    return (int)longValue;
                }
                else if (value is int intValue)
                {
                    return intValue;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
