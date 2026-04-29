//-----------------------------------------------------------------------
// <copyright file="Context.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY

namespace Lost.CloudFunctions
{
    using System;

    public class CloudFunctionContext
    {
        private bool? isDevelopmentEnvironment;
        private string ablyServerKey;
        private string redisConnectionString;
        private string cosmosDbConnectionString;

        public string PlayerId { get; set; }

        public global::PlayFab.PlayFabAuthenticationContext TitleAuthenticationContext { get; set; }

        public bool IsDevelopmentEnvironment => this.GetEnvironmentVariable(ref this.isDevelopmentEnvironment, "DEVELOPMENT", false);

        public string AblyServerKey => this.GetEnvironmentVariable(ref this.ablyServerKey, "ABLY_SERVER_KEY");

        public string RedisConnectionString => this.GetEnvironmentVariable(ref this.redisConnectionString, "REDIS_CONNECTION_STRING");

        public string CosmosDbConnectionString => this.GetEnvironmentVariable(ref this.cosmosDbConnectionString, "COSMOS_DB_CONNECTION_STRING");

        private string GetEnvironmentVariable(ref string variableCache, string variableName)
        {
            if (variableCache == null)
            {
                variableCache = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process);

                if (string.IsNullOrWhiteSpace(variableCache))
                {
                    UnityEngine.Debug.LogError($"Failed to get Environment Variable {variableName}");
                }
            }

            return variableCache;
        }

        private bool GetEnvironmentVariable(ref bool? variableCache, string variableName, bool defaultValue)
        {
            if (variableCache.HasValue == false)
            {
                string environmentValue = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process);

                if (string.IsNullOrWhiteSpace(environmentValue))
                {
                    UnityEngine.Debug.LogError($"Failed to get Environment Variable {variableName}");
                    variableCache = defaultValue;
                }
                else
                {
                    variableCache = environmentValue.ToUpperInvariant() == "TRUE" || environmentValue.ToUpperInvariant() == "1";
                }
            }

            return variableCache.Value;
        }
    }
}

#endif
