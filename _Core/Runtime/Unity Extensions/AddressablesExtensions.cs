using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressablesExtensions
{
    public static Task<GameObject> InstantiateAsyncTask(this AssetReference assetReference, Transform parent = null)
    {
        var tcs = new TaskCompletionSource<GameObject>();
        var handle = Addressables.InstantiateAsync(assetReference.RuntimeKey, parent);

        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                tcs.SetResult(op.Result);
            }
            else
            {
                tcs.SetException(op.OperationException);
            }
        };

        return tcs.Task;
    }
}
