//#pragma warning disable

////-----------------------------------------------------------------------
//// <copyright file="AzureFunctionsProjectGenerator.cs" company="Lost Signal LLC">
////     Copyright (c) Lost Signal LLC. All rights reserved.
//// </copyright>
////-----------------------------------------------------------------------

//namespace OGT
//{
//    using System.Collections.Generic;
//    using System.IO;
//    using System.Linq;
//    using System.Text;

//#if USING_PLAYFAB
//    using OGT.PlayFab;
//#endif

//    using UnityEditor;
//    using UnityEngine;

//    public class AzureFunctionsProjectGenerator : CSharpProjectGenerator
//    {
//        private static readonly OGTLogger Logger = OGTLogger.LostEditor;

//#pragma warning disable 0649
//        [SerializeField]
//        private string[] extraUsings = new string[]
//        {
//            "Lost.CloudFunctions.Common",
//            "Lost.CloudFunctions.Debug",
//            "Lost.CloudFunctions.Login",
//        };
//#pragma warning restore 0649

//        static AzureFunctionsProjectGenerator()
//        {
//            // Making sure all default assets are created
//            EditorApplication.delayCall += () =>
//            {
//                GetInstance();
//            };
//        }

//        public static AzureFunctionsProjectGenerator GetInstance()
//        {
//#if USING_PLAYFAB

//            const string AzureFunctionsEditorBuildSettingsId = "com.lostsignal.azurefunctions";

//            if (EditorBuildSettings.TryGetConfigObject(AzureFunctionsEditorBuildSettingsId, out AzureFunctionsProjectGenerator azureFunctionsGenerator) == false || !azureFunctionsGenerator)
//            {
//                azureFunctionsGenerator = LostLibrary.CreateScriptableObject<AzureFunctionsProjectGenerator>("264a336d161a54948b392550765de177", "AzureFunctionsGenerator.asset");
//                EditorBuildSettings.AddConfigObject(AzureFunctionsEditorBuildSettingsId, azureFunctionsGenerator, true);
//            }

//            return azureFunctionsGenerator;

//#else

//            return null;

//#endif
//        }

//#if USING_PLAYFAB

//        private string azureFunctionsUsingsCSharp;
//        private string functionsCSharp;

//        public async void UploadFunctionsToPlayFab()
//        {
//            var playFabSettings = LostSettingsBuildConfigs.Instance.ActiveBuildConfig.GetSettings<PlayFabSettings>();

//            if (playFabSettings == null || string.IsNullOrEmpty(playFabSettings.FunctionsSite) || string.IsNullOrEmpty(playFabSettings.FunctionsHostKey))
//            {
//                Logger.LogError("PlayFabSettings BuildConfig must define functions site and host key.");
//                return;
//            }

//            global::PlayFab.PlayFabSettings.staticSettings.TitleId = playFabSettings.TitleId;
//            global::PlayFab.PlayFabSettings.staticSettings.DeveloperSecretKey = playFabSettings.SecretKey;

//            var getTitleEntityToken = await PlayFabUtil.GetTitleEntityTokenAsync();

//            // Calculating the function site (need to append "/api/" to the end correctly)
//            string functionsSite = playFabSettings.FunctionsSite;

//            if (functionsSite.EndsWith("/api/"))
//            {
//                // Do Nothing
//            }
//            else if (functionsSite.EndsWith("/api"))
//            {
//                functionsSite += "/";
//            }
//            else if (functionsSite.EndsWith("/"))
//            {
//                functionsSite += "api/";
//            }
//            else
//            {
//                functionsSite += "/api/";
//            }

//            foreach (var function in this.GetAllCloudFunctions())
//            {
//                await global::PlayFab.PlayFabCloudScriptAPI.RegisterHttpFunctionAsync(new global::PlayFab.CloudScriptModels.RegisterHttpFunctionRequest
//                {
//                    FunctionName = function.FullName,
//                    FunctionUrl = functionsSite + function.FullName + "?code=" + playFabSettings.FunctionsHostKey,
//                    AuthenticationContext = new global::PlayFab.PlayFabAuthenticationContext
//                    {
//                        EntityId = getTitleEntityToken.Result.Entity.Id,
//                        EntityType = getTitleEntityToken.Result.Entity.Type,
//                        EntityToken = getTitleEntityToken.Result.EntityToken,
//                    },
//                });

//                Logger.Log($"Uploaded Function: {function.FullName}");
//            }
//        }

//        public void GenerateClientWrapperFiles()
//        {
//            foreach (var csharpFilePath in this.GetAllCSharpFiles())
//            {
//                List<CloudFunction> cloudFunctions = this.GetAllCloudFunctionsFromFilePath(csharpFilePath).OrderBy(x => x.Category).ToList();

//                if (cloudFunctions.Count == 0)
//                {
//                    continue;
//                }

//                string clientOutputFilePath = csharpFilePath.Replace(".cs", "Extensions.cs");
//                string className = Path.GetFileNameWithoutExtension(clientOutputFilePath);

//                StringBuilder clientFile = new StringBuilder();
//                CloudFunction lastCloudFunction = cloudFunctions.Last();

//                clientFile.AppendLine("// <auto-generated/>");
//                clientFile.AppendLine("#pragma warning disable");
//                clientFile.AppendLine();
//                clientFile.AppendLine("#if USING_PLAYFAB");
//                clientFile.AppendLine();
//                clientFile.AppendLine($"namespace OGT.CloudFunctions");
//                clientFile.AppendLine("{");
//                clientFile.AppendLine("    using System;");
//                clientFile.AppendLine("    using System.Collections.Generic;");
//                clientFile.AppendLine("    using System.Threading.Tasks;");

//                // Adding in extra usings
//                foreach (var extraUsing in this.extraUsings)
//                {
//                    clientFile.AppendLine($"    using {extraUsing};");
//                }

//                clientFile.AppendLine();
//                clientFile.AppendLine($"    public static class {className}");
//                clientFile.AppendLine(@"    {");

//                foreach (var cloudFunction in cloudFunctions)
//                {
//                    var templateType = cloudFunction.ResultType != "Task" ?
//                        $"<{cloudFunction.ResultType.Substring(0, cloudFunction.ResultType.Length - 1).Replace("Task<", string.Empty)}>" :
//                        string.Empty;

//                    if (cloudFunction.RequestType != null)
//                    {
//                        clientFile.AppendLine($"        public static Task<Result{templateType}> {cloudFunction.FullName}(this CloudFunctionsManager cloudFunctionsManager, {cloudFunction.RequestType} request) => cloudFunctionsManager.Execute{templateType}(\"{cloudFunction.FullName}\", request);");
//                    }
//                    else
//                    {
//                        clientFile.AppendLine($"        public static Task<Result{templateType}> {cloudFunction.FullName}(this CloudFunctionsManager cloudFunctionsManager) => cloudFunctionsManager.Execute{templateType}(\"{cloudFunction.FullName}\");");
//                    }

//                    if (cloudFunction != lastCloudFunction)
//                    {
//                        clientFile.AppendLine();
//                    }
//                }

//                clientFile.AppendLine("    }");
//                clientFile.AppendLine("}");
//                clientFile.AppendLine();
//                clientFile.AppendLine("#endif");

//                Logger.Log($"Writing Client Wrapper File {clientOutputFilePath}");
//                FileUtil.CreateOrUpdateFile(clientFile.ToString(), clientOutputFilePath, true);
//            }

//            AssetDatabase.Refresh();
//        }

//        public override void Generate()
//        {
//            this.functionsCSharp = this.GenerateAzureFunctionsCSharp();
//            this.azureFunctionsUsingsCSharp = this.GenerateAzureFunctionsUsings();
//            base.Generate();
//        }

//        protected override void WriteFile(string filePath, string contents)
//        {
//            contents = contents
//                .Replace("__AZURE_FUNCTION_USINGS__", this.azureFunctionsUsingsCSharp)
//                .Replace("__AZURE_FUNCTIONS__", this.functionsCSharp);

//            base.WriteFile(filePath, contents);
//        }

//        private List<string> GetAllCSharpFiles()
//        {
//            var result = new List<string>();

//            foreach (var codeFolder in this.CodeFolders)
//            {
//                string codeFolderPath = AssetDatabase.GetAssetPath(codeFolder.Folder);
//                var directoryInfo = new DirectoryInfo(codeFolderPath);

//                foreach (var csharpFilePath in Directory.GetFiles(directoryInfo.FullName, "*.cs", SearchOption.AllDirectories))
//                {
//                    result.Add(csharpFilePath);
//                }
//            }

//            return result;
//        }

//        private List<CloudFunction> GetAllCloudFunctions()
//        {
//            var result = new List<CloudFunction>();

//            foreach (var csharpFilePath in this.GetAllCSharpFiles())
//            {
//                result.AddRange(this.GetAllCloudFunctionsFromFilePath(csharpFilePath));
//            }

//            return result;
//        }

//        private string GenerateAzureFunctionsCSharp()
//        {
//            StringBuilder azureFunctionsSource = new StringBuilder();
//            List<CloudFunction> cloudFunctions = this.GetAllCloudFunctions();
//            CloudFunction last = cloudFunctions.Last();

//            foreach (var cloudFunction in cloudFunctions)
//            {
//                azureFunctionsSource.Append(this.GenerateAzureFunctionCSharp(cloudFunction));

//                if (cloudFunction != last)
//                {
//                    azureFunctionsSource.AppendLine();
//                }
//            }

//            return azureFunctionsSource.ToString();
//        }

//        private string GenerateAzureFunctionCSharp(CloudFunction cloudFunction)
//        {
//            string returnType = cloudFunction.ResultType == "Task" ? "Task" : "Task<object>";
//            string authorizationLevel = cloudFunction.Type == CloudFunction.CloudFunctionType.Anonymous ? "AuthorizationLevel.Anonymous" : "AuthorizationLevel.Function";

//            bool returnsObject = cloudFunction.ResultType != "Task";
//            string functionReturnStatement = returnsObject ? "return JsonUtil.Serialize(await" : "await";
//            string functionReturnStatementEnd = returnsObject ? ")" : string.Empty;

//            StringBuilder function = new StringBuilder();

//            function.AppendLine($"    [FunctionName(\"{cloudFunction.FullName}\")]");
//            function.AppendLine($"    public static async {returnType} {cloudFunction.FullName}([HttpTrigger({authorizationLevel}, \"post\", Route = null)] FunctionExecutionContext req, HttpRequest httpRequest, ILogger log)");
//            function.AppendLine(@"    {");
//            function.AppendLine($"        using (new AzureFunctionWrapper(req, httpRequest, log, out CloudFunctionContext context))");
//            function.AppendLine(@"        {");

//            if (cloudFunction.Type == CloudFunction.CloudFunctionType.Development)
//            {
//                function.AppendLine($"            await DoNotRunInProductionCheck(context);");
//            }

//            if (string.IsNullOrEmpty(cloudFunction.RequestType))
//            {
//                function.AppendLine($"            {functionReturnStatement} {cloudFunction.FullFunctionPath}(context){functionReturnStatementEnd};");
//            }
//            else
//            {
//                function.AppendLine($"            {functionReturnStatement} {cloudFunction.FullFunctionPath}(context, JsonUtil.Deserialize<{cloudFunction.RequestType}>(req?.FunctionArgument)){functionReturnStatementEnd};");
//            }

//            function.AppendLine(@"        }");
//            function.AppendLine(@"    }");

//            return function.ToString();
//        }

//        private List<CloudFunction> GetAllCloudFunctionsFromFilePath(string csharpFilePath)
//        {
//            string csharpText = System.IO.File.ReadAllText(csharpFilePath);
//            var result = new List<CloudFunction>();

//            result.AddRange(this.GetAllCloudFunctions(csharpText, "[CloudFunction(", CloudFunction.CloudFunctionType.Default));
//            result.AddRange(this.GetAllCloudFunctions(csharpText, "[DevCloudFunction(", CloudFunction.CloudFunctionType.Development));
//            result.AddRange(this.GetAllCloudFunctions(csharpText, "[AnonymousCloudFunction(", CloudFunction.CloudFunctionType.Anonymous));

//            return result;
//        }

//        private List<CloudFunction> GetAllCloudFunctions(string csharpSource, string attributString, CloudFunction.CloudFunctionType type)
//        {
//            var result = new List<CloudFunction>();

//            string fileNamespace = GetNamespace(csharpSource);
//            string className = GetClassName(csharpSource);
//            int index = csharpSource.IndexOf(attributString);

//            while (index > 0)
//            {
//                var function = new CloudFunction
//                {
//                    Type = type,
//                    Namespace = fileNamespace,
//                };

//                // Calcuating the Category and Name
//                int endOfAttributeIndex = csharpSource.IndexOf("]", index);
//                var categoryAndName = csharpSource.Substring(index, endOfAttributeIndex - index)
//                    .Replace(attributString, string.Empty)
//                    .Replace("\"", string.Empty)
//                    .Replace(" ", string.Empty)
//                    .Replace(")", string.Empty)
//                    .Replace("(", string.Empty)
//                    .Split(',');

//                function.Category = categoryAndName[0];
//                function.Name = categoryAndName[1];

//                // Calculating ResultType and FullFunctionPath
//                int parenIndex = csharpSource.IndexOf("(", endOfAttributeIndex);
//                string functionAndReturnType = csharpSource.Substring(endOfAttributeIndex + 1, parenIndex - endOfAttributeIndex - 1)
//                    .Replace("public", string.Empty)
//                    .Replace("private", string.Empty)
//                    .Replace("protected", string.Empty)
//                    .Replace("async", string.Empty)
//                    .Replace("static", string.Empty)
//                    .Trim();

//                string functionName = functionAndReturnType.Substring(functionAndReturnType.LastIndexOf(' ')).Trim();
//                function.ResultType = functionAndReturnType.Substring(0, functionAndReturnType.LastIndexOf(' ')).Trim();

//                function.FullFunctionPath = string.IsNullOrEmpty(fileNamespace) == false ?
//                    $"{fileNamespace}.{className}.{functionName}" :
//                    $"{className}.{functionName}";

//                // Calculate RequestType
//                string[] parameters = csharpSource.Substring(parenIndex + 1, csharpSource.IndexOf(')', parenIndex) - parenIndex - 1).Split(',');

//                if (parameters.Length == 1 || parameters.Length == 2)
//                {
//                    if (parameters[0].Contains("CloudFunctionContext") == false)
//                    {
//                        Logger.LogError($"Cloud Function {function.FullName} first parameter must be a CloudFunctionContext!  Skipping...");
//                    }

//                    if (parameters.Length == 2)
//                    {
//                        var secondParameter = parameters[1].Trim();

//                        function.RequestType = secondParameter.Substring(0, secondParameter.IndexOf(" ")).Trim();
//                    }
//                }

//                // Making sure it's a valid cloud function
//                if (function.ResultType.StartsWith("Task") == false)
//                {
//                    Logger.LogError($"Cloud Function {function.FullName} does not return a Task!  Skipping...");
//                }
//                else if (parameters.Length != 1 && parameters.Length != 2)
//                {
//                    Logger.LogError($"Cloud Function {function.FullName} must contain 1 or 2 parameters.  Skipping...");
//                }
//                else
//                {
//                    result.Add(function);
//                }

//                index = csharpSource.IndexOf(attributString, index + attributString.Length);
//            }

//            return result.OrderBy(x => x.Category).ToList();

//            string GetNamespace(string csharpCode)
//            {
//                using (StringReader reader = new StringReader(csharpCode))
//                {
//                    string line;

//                    while ((line = reader.ReadLine()) != null)
//                    {
//                        if (line.Trim().StartsWith("namespace"))
//                        {
//                            return line
//                                .Replace("namespace", string.Empty)
//                                .Replace("{", string.Empty)
//                                .Trim();
//                        }
//                    }
//                }

//                return null;
//            }

//            string GetClassName(string csharpCode)
//            {
//                using (StringReader reader = new StringReader(csharpCode))
//                {
//                    string line;

//                    while ((line = reader.ReadLine()) != null)
//                    {
//                        if (line.Trim().Contains(" class "))
//                        {
//                            return line
//                                .Replace("public", string.Empty)
//                                .Replace("static", string.Empty)
//                                .Replace("class", string.Empty)
//                                .Replace("{", string.Empty)
//                                .Trim();
//                        }
//                    }
//                }

//                return null;
//            }
//        }

//        private string GenerateAzureFunctionsUsings()
//        {
//            StringBuilder usings = new StringBuilder();

//            foreach (var functionUsing in this.extraUsings)
//            {
//                usings.AppendLine($"using {functionUsing};");
//            }

//            return usings.ToString();
//        }

//        public class CloudFunction
//        {
//            public enum CloudFunctionType
//            {
//                Default,
//                Development,
//                Anonymous,
//            }

//            public string Namespace { get; set; }

//            public string Category { get; set; }

//            public string Name { get; set; }

//            public CloudFunctionType Type { get; set; }

//            public string RequestType { get; set; }

//            public string ResultType { get; set; }

//            public string FullFunctionPath { get; set; }

//            public string FullName
//            {
//                get => $"{this.Category}_{this.Name}";
//            }
//        }

//#endif
//    }
//}
