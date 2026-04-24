//-----------------------------------------------------------------------
// <copyright file="SceneRef.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [Serializable]
    public class SceneRef
    {
        [SerializeField] private string sceneName;

        [Tooltip("This is optional, if this set, then the scene will be loaded with Addressables.")]
        [SerializeField] private string sceneAddressablesPath;

        public string SceneName
        {
            get => this.sceneName;
            set => this.sceneName = value;
        }

        public string SceneAddressablesPath
        {
            get => this.sceneAddressablesPath;
            set => this.sceneAddressablesPath = value;
        }

        public IEnumerator LoadScene()
        {
            if (IsSceneLoaded(this.sceneName) == false)
            {
                if (string.IsNullOrWhiteSpace(this.SceneAddressablesPath) == false)
                {
                    // yield return UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(this.SceneAddressablesPath, LoadSceneMode.Additive);
                    throw new System.NotImplementedException();
                }
                else
                {
                    yield return SceneManager.LoadSceneAsync(this.sceneName, LoadSceneMode.Additive);
                }
            }
        }

        public bool IsLoaded()
        {
            return SceneManager.GetSceneByName(this.sceneName).isLoaded;
        }

        private static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
