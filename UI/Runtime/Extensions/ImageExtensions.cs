namespace OGT
{
    using System;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.UI;

    public static class ImageExtensions
    {
        public static void SetAddressableSprite(this Image image, AssetReferenceT<Sprite> spriteReference, Action onComplete = null)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                image.sprite = spriteReference?.editorAsset;
                onComplete?.Invoke();
                return;
            }
#endif

            if (string.IsNullOrEmpty(spriteReference?.AssetGUID) == false)
            {
                // If it's valid, then it's complete or in progress
                if (spriteReference.IsValid())
                {
                    if (IsComplete(spriteReference.OperationHandle))
                    {
                        HandleCompleteResponse(spriteReference.OperationHandle, image, spriteReference?.AssetGUID, onComplete);
                    }
                    else
                    {
                        spriteReference.OperationHandle.Completed += (handle) => HandleCompleteResponse(handle, image, spriteReference?.AssetGUID, onComplete);
                    }
                }
                else
                {
                    spriteReference.LoadAssetAsync<Sprite>().Completed += (handle) => HandleCompleteResponse(handle, image, spriteReference?.AssetGUID, onComplete);
                }
            }

            static bool IsComplete(AsyncOperationHandle handle) => handle.Status == AsyncOperationStatus.Succeeded || handle.Status == AsyncOperationStatus.Failed;

            static void HandleCompleteResponse(AsyncOperationHandle operationHandle, Image image, string guid, Action onComplete)
            {
                Debug.Assert(IsComplete(operationHandle), $"{nameof(ImageExtensions)}.{nameof(HandleCompleteResponse)} was given an incomplete Response!");

                if (operationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    image.sprite = operationHandle.Result as Sprite;
                    onComplete?.Invoke();
                }
                else if (operationHandle.Status == AsyncOperationStatus.Failed)
                {
                    Debug.LogError($"Unexpected AssetReferenceT<Sprite> status: {operationHandle.Status} for {guid}");
                    onComplete?.Invoke();
                }
            }
        }
    }
}
