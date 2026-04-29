namespace OGT
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    //// NOTE [bgish]: This is responsible for determining at startup which bootloader obejct to instantiate
    ////               and call Boot on it.  It also needs to look at the editor settings "if in the editor"
    ////               and make sure it doesn't run if it's not suppose to.
    public static class BootloaderRunner
    {
        private static readonly OGTLogger Logger = OGTLogger.Bootloader;

        public static string BootloaderGuid => RuntimeSettings.GetSetting<string>("OGT.Bootloader");

        public static bool IsBootloaderEnabled => string.IsNullOrEmpty(BootloaderGuid) == false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InitializeBootloaderAfterAssemblies()
        {
            CoroutineRunner.Instance.StartCoroutine(BootCoroutine());

            static IEnumerator BootCoroutine()
            {
                var bootloaderGuid = RuntimeSettings.GetSetting<string>("OGT.Bootloader");

                if (string.IsNullOrEmpty(bootloaderGuid))
                {
                    yield break;
                }

                yield return UnityEngine.AddressableAssets.Addressables.InitializeAsync();

                var bootloaderAssetReference = new AssetReference(bootloaderGuid);
                var load = bootloaderAssetReference.LoadAssetAsync<GameObject>();
                yield return load;

                var bootloaderGameObject = GameObject.Instantiate(load.Result);
                bootloaderGameObject.name = $"OGT - {load.Result.name}";
                GameObject.DontDestroyOnLoad(bootloaderGameObject);
                bootloaderGameObject.GetComponent<Bootloader>().Boot();
            }
        }
    }
}

/*//-----------------------------------------------------------------------
// <copyright file="Bootloader.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class Bootloader : GameBehavior
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

#pragma warning disable 0649
        [SerializeField] private List<string> ignoreSceneNames;
        [SerializeField] private List<SceneRef> alwaysLoadedScenes;
        [SerializeReference] private List<ProviderInitializer> providerInitializers;
        [SerializeReference] private List<Manager> managerInitializers;

        //// [HideInInspector]
        //// [SerializeField]
        //// private string json;
#pragma warning restore 0649

        private static Bootloader instance;

        private List<IManager> managers = new List<IManager>();

        public delegate void OnBootedDelegate();
        private OnBootedDelegate onBooted;

        public event OnBootedDelegate OnBooted
        {
            add
            {
                if (IsBooted)
                {
                    value?.Invoke();
                }
                else
                {
                    onBooted += value;
                }
            }

            remove => onBooted -= value;
        }

        //// #if UNITY_EDITOR
        ////         [EditorEvents.InitializeOnLoad]
        ////         private static void SetupBootloader()
        ////         {
        ////             if (Resources.Load<Bootloader>("Bootloader") == null)
        ////             {
        ////                 string bootloaderAssetPath = "Assets/Resources/Bootloader.asset";
        ////
        ////                 OGTLogger.OGTEditor.Log($"Creating Bootloader Scriptable Object at {bootloaderAssetPath}...");
        ////
        ////                 // Making / Saving the Bootloader Object
        ////                 var bootloader = ScriptableObject.CreateInstance<Bootloader>();
        ////                 bootloader.name = nameof(Bootloader);
        ////                 CreateFolders(bootloaderAssetPath);
        ////                 UnityEditor.AssetDatabase.CreateAsset(bootloader, bootloaderAssetPath);
        ////
        ////                 //// // Making / Saving the Managers Object
        ////                 //// var managers = ScriptableObject.CreateInstance<Managers>();
        ////                 //// managers.name = nameof(Managers);
        ////                 //// UnityEditor.AssetDatabase.CreateAsset(managers, "Assets/Resources/Managers.asset");
        ////                 ////
        ////                 //// bootloader.managerInitializers = managers;
        ////
        ////                 //// NOTE [bgish]: Old method when Managers was a lazy loaded object by guid
        ////                 //// // Making sure Bootloader points to Managers class
        ////                 //// bool found = UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(managers, out string guid, out long localId);
        ////                 //// if (found)
        ////                 //// {
        ////                 ////     bootloader.managers = new LazyAssetT<Managers>(guid);
        ////                 ////     EditorUtil.SetDirty(bootloader);
        ////                 //// }
        ////             }
        ////
        ////             void CreateFolders(string assetPath)
        ////             {
        ////                 var directory = System.IO.Path.GetDirectoryName(assetPath).Replace('\\', '/');
        ////                 var directories = directory.Split('/');
        ////
        ////                 var rootFolder = directories[0];
        ////                 for (int i = 1; i < directories.Length; i++)
        ////                 {
        ////                     string folderPath = rootFolder + "/" + directories[i];
        ////
        ////                     if (UnityEditor.AssetDatabase.AssetPathExists(folderPath) == false)
        ////                     {
        ////                         UnityEditor.AssetDatabase.CreateFolder(rootFolder, directories[i]);
        ////                     }
        ////
        ////                     rootFolder = folderPath;
        ////                 }
        ////             }
        ////         }
        ////
        ////         [EditorEvents.OnExitPlayMode]
        ////         private static void OnExitPlayMode()
        ////         {
        ////             if (instance != null)
        ////             {
        ////                 Resources.UnloadAsset(instance);
        ////             }
        ////
        ////             instance = null;
        ////             IsBooted = false;
        ////         }
        //// #endif

        public static bool IsBooted { get; private set; }

        public static void Reboot()
        {
            //// TODO [bgish]: Implement
            throw new NotImplementedException("Bootloader.Reboot() is not implemented yet!");
        }

        public T FindManager<T>() where T : class
        {
            foreach (var manager in this.managers)
            {
                if (manager is T)
                {
                    return (T)manager;
                }
            }

            Logger.LogError($"Unable to find Manager {typeof(T).Name}!");
            return null;
        }

        //// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        //// private static void InitializeBootloaderAfterAssemblies()
        //// {
        ////     IsBooted = false;
        //// }
        ////
        //// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        //// private static void InitializeBootloaderBeforeSceneLoad()
        //// {
        ////     instance = Resources.Load<Bootloader>("Bootloader");
        ////
        ////     // Initialize Providers
        ////     foreach (var provicerInitializer in instance.providerInitializers)
        ////     {
        ////         provicerInitializer.Initialize();
        ////     }
        ////
        ////     CoroutineRunner.Instance.StartCoroutine(Initialize());
        ////
        ////     IEnumerator Initialize()
        ////     {
        ////         if (ShouldRunBootloader(instance))
        ////         {
        ////             // TODO [bgish]: Get Current Version and Server Version
        ////             // TODO [bgish]: Force Update if needed
        ////             // TODO [bgish]: Set the addressables URL
        ////
        ////             // Initialize Addressables
        ////             yield return UnityEngine.AddressableAssets.Addressables.InitializeAsync();
        ////
        ////             // Initialize Managers
        ////             foreach (var managerInitializer in instance.managerInitializers)
        ////             {
        ////                 if (managerInitializer.InitializeAtStartup == false)
        ////                 {
        ////                     continue;
        ////                 }
        ////
        ////                 var manager = managerInitializer.Initialize();
        ////                 instance.managers.Add(manager);
        ////
        ////                 if (manager != ActivationManager.Instance)
        ////                 {
        ////                     ActivationManager.Register(manager);
        ////                 }
        ////                 else
        ////                 {
        ////                     ActivationManager.Instance.IsPaused = true;
        ////                 }
        ////
        ////                 yield return null;
        ////             }
        ////
        ////             // Wait for all Managers to be ready
        ////             while (true)
        ////             {
        ////                 bool allDone = true;
        ////
        ////                 foreach (var manager in instance.managers)
        ////                 {
        ////                     if (manager.IsReady == false)
        ////                     {
        ////                         allDone = false;
        ////                         break;
        ////                     }
        ////                 }
        ////
        ////                 if (allDone)
        ////                 {
        ////                     break;
        ////                 }
        ////
        ////                 yield return null;
        ////             }
        ////
        ////             ActivationManager.Instance.IsPaused = false;
        ////
        ////             // Load Always Loaded Scenes
        ////             foreach (var scene in instance.alwaysLoadedScenes)
        ////             {
        ////                 yield return scene.LoadScene();
        ////             }
        ////         }
        ////
        ////         IsBooted = true;
        ////         onBooted?.Invoke();
        ////         onBooted = null;
        ////     }
        ////
        ////     bool ShouldRunBootloader(Bootloader bootloader)
        ////     {
        ////         if (bootloader == null)
        ////         {
        ////             return false;
        ////         }
        ////
        ////         var activeSceneName = SceneManager.GetActiveScene().name;
        ////
        ////         foreach (var sceneToIgnore in bootloader.ignoreSceneNames)
        ////         {
        ////             if (activeSceneName == sceneToIgnore)
        ////             {
        ////                 return false;
        ////             }
        ////         }
        ////
        ////         return true;
        ////     }
        //// }
        ////
        //// public static void PopulateSettings()
        //// {
        ////     var bootloader = Resources.Load<Bootloader>("Bootloader");
        ////
        ////     if (bootloader == null)
        ////     {
        ////         return;
        ////     }
        ////
        ////     bool didChange = false;
        ////     didChange |= PopulateInitializerList(ref bootloader.managerInitializers);
        ////     didChange |= PopulateInitializerList(ref bootloader.providerInitializers);
        ////
        ////     if (didChange)
        ////     {
        ////         EditorUtil.SetDirty(bootloader);
        ////     }
        //// }
        ////
        //// public static void ResetInitializers()
        //// {
        ////     var bootloader = Resources.Load<Bootloader>("Bootloader");
        ////
        ////     if (bootloader != null)
        ////     {
        ////         bootloader.managerInitializers.Clear();
        ////         bootloader.providerInitializers.Clear();
        ////         EditorUtil.SetDirty(bootloader);
        ////     }
        //// }
        ////
        //// public static bool PopulateInitializerList<T>(ref List<T> list)
        ////     where T : Initializer
        //// {
        ////     bool didChange = false;
        ////
        ////     if (list == null)
        ////     {
        ////         list = new List<T>();
        ////         didChange = true;
        ////     }
        ////
        ////     didChange |= RemoveNulls(list);
        ////
        ////     foreach (var type in TypeUtil.GetAllTypesOf<T>())
        ////     {
        ////         didChange |= AddToListIfDoesNotExist(list, type);
        ////     }
        ////
        ////     return didChange;
        ////
        ////     static bool RemoveNulls(List<T> list)
        ////     {
        ////         bool foundNullItem = false;
        ////
        ////         for (int i = list.Count - 1; i >= 0; i--)
        ////         {
        ////             if (list[i] == null)
        ////             {
        ////                 foundNullItem = true;
        ////                 list.RemoveAt(i);
        ////             }
        ////         }
        ////
        ////         return foundNullItem;
        ////     }
        ////
        ////     static bool AddToListIfDoesNotExist(List<T> list, Type type)
        ////     {
        ////         foreach (var setting in list)
        ////         {
        ////             if (setting != null && setting.GetType() == type)
        ////             {
        ////                 return false;
        ////             }
        ////         }
        ////
        ////         // Couldn't find it so adding a new one
        ////         var initializer = Activator.CreateInstance(type) as T;
        ////
        ////         // Initializing the new manager settings
        ////         SetField(initializer, "name", initializer.Name);
        ////         SetField(initializer, "initializeAtStartup", true);
        ////         initializer.SetToDefaultValues();
        ////
        ////         // Adding to the list
        ////         list.Add(initializer);
        ////         return true;
        ////
        ////         static void SetField(object instance, string fieldName, object value)
        ////         {
        ////             var nameField = typeof(Initializer).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        ////             nameField.SetValue(instance, value);
        ////         }
        ////     }
        //// }
        ////
        //// public void OnBeforeSerialize()
        //// {
        ////     this.json = this.GetJson();
        //// }
        ////
        //// public void OnAfterDeserialize()
        //// {
        //// }
        ////
        //// public string GetJson()
        //// {
        ////     if (this)
        ////     {
        ////         return JsonUtil.Serialize(this);
        ////     }
        ////
        ////     return null;
        //// }
        ////
        //// private Coroutine StartBootupSequence()
        //// {
        ////     return this.StartCoroutine(Coroutine());
        ////
        ////     IEnumerator Coroutine()
        ////     {
        ////         float startTime = Time.realtimeSinceStartup;
        ////         this.bootloaderDialog.Dialog.Show();
        ////
        ////         yield return DialogManager.WaitForInitialization();
        ////         yield return ReleasesManager.WaitForInitialization();
        ////         yield return ReleasesManager.Instance.ShowForceUpdateDialog();
        ////         yield return AddressablesManager.WaitForInitialization();
        ////
        ////         // Getting the Bootloader Config
        ////         string bootloaderConfigLocation = RuntimeBuildConfig.Instance.GetString(BootloaderConfigLocation);
        ////         int bootloaderCongigLocationInt = int.Parse(bootloaderConfigLocation);
        ////         var bootloaderLocation = (BootloaderConfigLocation)bootloaderCongigLocationInt;
        ////
        ////         if (bootloaderLocation == OGT.BootloaderConfigLocation.RuntimeConfigSettings)
        ////         {
        ////             string bootloaderConfigJson = RuntimeBuildConfig.Instance.GetString(BootloaderConfig);
        ////             this.bootloaderConfig = JsonUtil.Deserialize<BootloaderConfig>(bootloaderConfigJson);
        ////         }
        ////         else if (bootloaderLocation == OGT.BootloaderConfigLocation.Releases)
        ////         {
        ////             throw new NotImplementedException();
        ////         }
        ////         else
        ////         {
        ////             OGT.Logger.LogError($"Unknown BootloaderConfigLocation encountered {bootloaderLocation}");
        ////             yield break;
        ////         }
        ////
        ////         // Loading all Required Scenes
        ////         foreach (var requiredScene in this.bootloaderConfig.RequiredScenes)
        ////         {
        ////             yield return requiredScene.LoadScene();
        ////
        ////             while (requiredScene.IsLoaded() == false)
        ////             {
        ////                 yield return null;
        ////             }
        ////         }
        ////
        ////         yield return null;
        ////         yield return null;
        ////         yield return null;
        ////
        ////         // Waiting for all managers to finish loading
        ////         yield return WaitForManagersToInitialize();
        ////
        ////         // Disabling the Loading camera now that all required scenes are loaded
        ////         this.loadingCamera.gameObject.SetActive(false);
        ////         DialogManager.ForceUpdateDialogCameras(Camera.main);
        ////
        ////         // Making sure we wait the minimum time
        ////         if (this.ShowLoadingInEditor && this.bootloaderDialog)
        ////         {
        ////             float elapsedTime = Time.realtimeSinceStartup - startTime;
        ////
        ////             if (elapsedTime < this.minimumLoadingDialogTime)
        ////             {
        ////                 yield return WaitForUtil.Seconds(this.minimumLoadingDialogTime - elapsedTime);
        ////             }
        ////
        ////             // Making sure we don't say Hide if we're still showing (has a bad pop)
        ////             while (this.bootloaderDialog.Dialog.IsShown == false)
        ////             {
        ////                 yield return null;
        ////             }
        ////         }
        ////
        ////         // Doing a little cleanup before giving user control
        ////         System.GC.Collect();
        ////         yield return null;
        ////
        ////         // TODO [bgish]:  We're done!  Fire the OnBooted event????
        ////
        ////         if (this.ShowLoadingInEditor && this.bootloaderDialog)
        ////         {
        ////             this.bootloaderDialog.Dialog.Hide();
        ////         }
        ////     }
        //// }
    }
}
*/
