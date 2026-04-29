using OGT;
using OGT.PlayFab;
using PlayFab.ClientModels;

public class CloudScriptManager
{
    public UnityTask<ExecuteCloudScriptResult> Execute(string functionName, object functionParameters = null)
    {
        // TODO [bgish]: Can't hard code revision number!
        return PlayFabManager.Instance.Do(new ExecuteCloudScriptRequest
        {
            FunctionName = functionName,
            RevisionSelection = CloudScriptRevisionOption.Specific,
            SpecificRevision = PlayFabManager.Instance.CloudScriptRevision,
            GeneratePlayStreamEvent = true,
            FunctionParameter = functionParameters,
        });
    }

    public T GetCloudScrtipResult<T>(UnityTask<ExecuteCloudScriptResult> result)
    {
        string functionResult = result.Value.FunctionResult != null ? result.Value.FunctionResult.ToString() : null;
        return JsonUtil.Deserialize<T>(functionResult);
    }
}
