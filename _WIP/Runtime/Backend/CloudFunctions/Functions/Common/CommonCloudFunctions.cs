//-----------------------------------------------------------------------
// <copyright file="CommonCloudFunctions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY

namespace Lost.CloudFunctions.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using global::PlayFab;
    using global::PlayFab.ServerModels;
    using OGT;
    using UnityEngine;

    public static class CommonCloudFunctions
    {
        private const string DefaultCharacterItemId = "DefaultCharacter";
        private const string DefaultCharacterName = "Default";

        private static string ablyBasicAuthString;

        [CloudFunction("Common", "IncrementBadCallCount")]
        public static Task IncrementBadCallCount(CloudFunctionContext context)
        {
            return IncrementStatBy(context, "BadCallCount", 1);
        }

        [CloudFunction("Common", "StartUnityCloudBuilds")]
        public static async Task StartUnityCloudBuilds(CloudFunctionContext context, StartUnityCloudBuildsRequest request)
        {
            if (request.BuildTargets.IsNullOrEmpty())
            {
                Debug.LogError("No BuildTargets Specified!");
                return;
            }

            int secondsDelay = 0;

            foreach (string buildTarget in request.BuildTargets)
            {
                await KickOffBuild(request.BasicAuth, request.Org, request.Project, buildTarget, secondsDelay);
                secondsDelay += request.SecondsBetweenBuilds;
            }

            async Task KickOffBuild(string basicAuth, string org, string project, string buildTarget, int delayInSeconds)
            {
                string uri = $"https://build-api.cloud.unity3d.com/api/v1/orgs/{org}/projects/{project}/buildtargets/{buildTarget}/builds";
                string json = "{\"clean\": false, \"delay\": " + delayInSeconds + "}";

                var response = await HttpUtil.SendJsonPost(uri, json, basicAuth);

                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
                {
                    Debug.Log($"Successfully kicked off build {buildTarget}");
                }
                else
                {
                    Debug.LogError($"Unable to kick off build {buildTarget}");
                }
            }
        }

        [CloudFunction("Common", "GrantDefaultCharacter")]
        public static async Task GrantDefaultCharacter(CloudFunctionContext context)
        {
            await PlayFabServerAPI.GrantItemsToUserAsync(new GrantItemsToUserRequest
            {
                PlayFabId = context.PlayerId,
                AuthenticationContext = context.TitleAuthenticationContext,
                ItemIds = new List<string> { DefaultCharacterItemId },
            });

            await PlayFabServerAPI.GrantCharacterToUserAsync(new GrantCharacterToUserRequest
            {
                PlayFabId = context.PlayerId,
                AuthenticationContext = context.TitleAuthenticationContext,
                CharacterType = DefaultCharacterItemId,
                CharacterName = DefaultCharacterName,
            });
        }

        public static Task<HttpWebResponse> SendRealtimeMessage(CloudFunctionContext context, string receiverId, RealtimeMessage realtimeMessage)
        {
            if (ablyBasicAuthString == null)
            {
                ablyBasicAuthString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(context.AblyServerKey));
            }

            return HttpUtil.SendJsonPost(
                $"https://rest.ably.io/channels/{receiverId}/messages",
                JsonUtil.Serialize(new Dictionary<string, object> { { "data", realtimeMessage } }),
                ablyBasicAuthString);
        }

        public static async Task<PlayFabResult<UpdatePlayerStatisticsResult>> IncrementStatBy(CloudFunctionContext context, string statisticName, int value)
        {
            var getStatistics = await PlayFabServerAPI.GetPlayerStatisticsAsync(new GetPlayerStatisticsRequest
            {
                PlayFabId = context.PlayerId,
                StatisticNames = new List<string> { statisticName },
                AuthenticationContext = context.TitleAuthenticationContext,
            });

            var statisticValue = getStatistics.Result.Statistics.FirstOrDefault(x => x.StatisticName == statisticName);
            int newStatisticValue = (statisticValue == null ? 0 : statisticValue.Value) + value;

            return await PlayFabServerAPI.UpdatePlayerStatisticsAsync(new UpdatePlayerStatisticsRequest
            {
                PlayFabId = context.PlayerId,
                AuthenticationContext = context.TitleAuthenticationContext,
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate
                    {
                        StatisticName = statisticName,
                        Value = newStatisticValue,
                    },
                },
            });
        }
    }
}

#endif
