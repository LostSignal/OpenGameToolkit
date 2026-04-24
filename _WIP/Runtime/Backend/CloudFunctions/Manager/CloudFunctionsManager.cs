//-----------------------------------------------------------------------
// <copyright file="CloudFunctionsManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_PLAYFAB

namespace Lost.CloudFunctions
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using OGT;
    using OGT.PlayFab;
    using UnityEngine;

    public sealed class CloudFunctionsManager : Manager
    {
        private PlayFabManager playFabManager;

#if UNITY_EDITOR
        private static string titleEntityTokenCache = null;
#endif

        protected override Task InitializeManager(Bootloader bootloader)
        {
            this.playFabManager = bootloader.FindManager<PlayFabManager>();

            return Task.CompletedTask;
        }

        public Task<Result> Execute(string functionName, object functionParameter = null)
        {
            if (CloudFunctionsUtil.UseLocalhostFunctions)
            {
                return this.ExecuteLocalhostCloudFuntion(functionName, functionParameter);
            }

            return this.ExecutePlayFabCloudFunction(functionName, functionParameter);
        }

        public Task<ResultT<T>> Execute<T>(string functionName, object functionParameter = null)
            where T : class
        {
            if (CloudFunctionsUtil.UseLocalhostFunctions)
            {
                return this.ExecuteLocalhostCloudFuntion<T>(functionName, functionParameter);
            }

            return this.ExecutePlayFabCloudFunction<T>(functionName, functionParameter);
        }

#if UNITY_EDITOR

        private static async Task<string> GetTitleEntityToken()
        {
            if (PlayFabSecretKeyCheck() == false)
            {
                return null;
            }

            if (titleEntityTokenCache.IsNullOrWhitespace())
            {
                try
                {
                    var getTitleEntityToken = await PlayFabUtil.GetTitleEntityTokenAsync();
                    titleEntityTokenCache = getTitleEntityToken.Result.EntityToken;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }

            return titleEntityTokenCache;
        }

        private static bool PlayFabSecretKeyCheck()
        {
            if (global::PlayFab.PlayFabSettings.staticSettings.TitleId.IsNullOrWhitespace() ||
                global::PlayFab.PlayFabSettings.staticSettings.DeveloperSecretKey.IsNullOrWhitespace())
            {
                UnityEngine.Debug.LogError(
                    "PlayFab Title Id and/or Developer Secret Key Not Set!  Localhost Azure Functions will not work. \n" +
                    "Do you have an active BuildConfig with PlayFab Settings added and filled out?");

                return false;
            }

            return true;
        }

#endif

        private async Task<Result> ExecutePlayFabCloudFunction(string functionName, object functionParameter)
        {
            try
            {
                var execute = await global::PlayFab.PlayFabCloudScriptAPI.ExecuteFunctionAsync(new global::PlayFab.CloudScriptModels.ExecuteFunctionRequest
                {
                    FunctionName = functionName,
                    FunctionParameter = JsonUtil.Serialize(functionParameter),
                    GeneratePlayStreamEvent = true,
                });

                if (execute.Error == null)
                {
                    return Result.Ok();
                }
                else
                {
                    var exception = new PlayFabException(execute.Error);
                    UnityEngine.Debug.LogException(exception);
                    return Result.Failure(exception);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Exception occured when calling Function {functionName}");
                UnityEngine.Debug.LogException(ex);
                return Result.Failure(ex);
            }
        }

        private async Task<ResultT<T>> ExecutePlayFabCloudFunction<T>(string functionName, object functionParameter)
            where T : class
        {
            try
            {
                var execute = await global::PlayFab.PlayFabCloudScriptAPI.ExecuteFunctionAsync(new global::PlayFab.CloudScriptModels.ExecuteFunctionRequest
                {
                    FunctionName = functionName,
                    FunctionParameter = JsonUtil.Serialize(functionParameter),
                    GeneratePlayStreamEvent = true,
                });

                if (execute.Error == null)
                {
                    // NOTE [bgish]: Even though our cloud functions return strings, PlayFab converts them to objects, so lets convert them back to a string
                    string resultJson = JsonUtil.Serialize(execute.Result.FunctionResult);

                    return ResultT<T>.Ok(JsonUtil.Deserialize<T>(resultJson));
                }
                else
                {
                    var exception = new PlayFabException(execute.Error);
                    UnityEngine.Debug.LogException(exception);
                    return ResultT<T>.Failure(exception);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Exception occured when calling Function {functionName}");
                UnityEngine.Debug.LogException(ex);
                return ResultT<T>.Failure(ex);
            }
        }

        private string GetLocalhostUrl(string functionName)
        {
            return $"http://localhost:7075/api/{functionName}";
        }

        private async Task<Result> ExecuteLocalhostCloudFuntion(string functionName, object functionParameter)
        {
#if UNITY_EDITOR
            if (PlayFabSecretKeyCheck() == false)
            {
                return null;
            }

            var functionExecution = new FunctionExecutionContext(
                JsonUtil.Serialize(functionParameter),
                this.playFabManager.User.PlayFabId,
                await GetTitleEntityToken());

            try
            {
                var response = await HttpUtil.SendJsonPost(this.GetLocalhostUrl(functionName), JsonUtil.Serialize(functionExecution));

                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                {
                    return Result.Ok();
                }
                else
                {
                    var exception = new Exception($"Bad Server Response from Function {functionName}: {response.StatusCode}");
                    UnityEngine.Debug.LogException(exception);
                    return Result.Failure(exception);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Exception occured when calling Function {functionName}");
                UnityEngine.Debug.LogException(ex);
                return Result.Failure(ex);
            }
#else
            return await Task.FromResult<Result>(default);
#endif
        }

        private async Task<ResultT<T>> ExecuteLocalhostCloudFuntion<T>(string functionName, object functionParameter)
            where T : class
        {
#if UNITY_EDITOR
            if (PlayFabSecretKeyCheck() == false)
            {
                return null;
            }

            var functionExecution = new FunctionExecutionContext(
                JsonUtil.Serialize(functionParameter),
                this.playFabManager.User.PlayFabId,
                await GetTitleEntityToken());

            try
            {
                var response = await HttpUtil.SendJsonPost(this.GetLocalhostUrl(functionName), JsonUtil.Serialize(functionExecution));

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using Stream responseStream = response.GetResponseStream();
                    using StreamReader responseReader = new StreamReader(responseStream);
                    return ResultT<T>.Ok(JsonUtil.Deserialize<T>(responseReader.ReadToEnd()));
                }
                else
                {
                    var exception = new Exception($"Bad Server Response from Function {functionName}: {response.StatusCode}");
                    UnityEngine.Debug.LogException(exception);
                    return ResultT<T>.Failure(exception);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Exception occured when calling Function {functionName}");
                UnityEngine.Debug.LogException(ex);
                return ResultT<T>.Failure(ex);
            }
#else
            return await Task.FromResult<ResultT<T>>(default);
#endif
        }

        public class FunctionResult
        {
            public string StringResult { get; set; }
        }
    }
}

#endif
