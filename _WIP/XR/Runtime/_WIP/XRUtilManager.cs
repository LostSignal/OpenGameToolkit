#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="XRUtilManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.XR
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using OGT;
    using UnityEngine;

#if USING_UNITY_XR_MANAGEMENT
    using UnityEngine.XR.Management;
#endif

    public class XRUtilManager : MonoBehaviour
    {
        private static readonly OGTLogger Logger = new("XR");

#pragma warning disable 0649
        [SerializeField] private bool initializeAtStartup = true;
        [SerializeField] private XRDevice flatMode;
        [SerializeField] private List<XRDevice> devices;

        [Header("HMD Search")]
        [SerializeField] private List<XRLoader> headsetSearchOrder;
        [SerializeField] private List<RuntimePlatform> headsetSearchPlatforms;

        [Header("Mobile AR")]
        [SerializeField] private bool setTargetFramerateOnMobileAR = true;
        [SerializeField] private int mobileArTargetFramerate = 30;

        [Header("Debug")]
        [SerializeField] private bool printDebugInfo = true;
#pragma warning restore 0649

        [NonSerialized]
        private XRDevice currentDevice;

        private Action onXRDeviceChange;

        public event Action OnXRDeviceChange
        {
            add => this.onXRDeviceChange += value;
            remove => this.onXRDeviceChange -= value;
        }

        public bool IsFlatMode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.currentDevice == this.flatMode;
        }

        public XRDevice CurrentDevice
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.currentDevice;
        }

        public void Initialize()
        {
#if USING_UNITY_XR_MANAGEMENT
            this.Initialize(Application.isEditor && Application.isPlaying && ForceFlatModeInEditorUtil.ForceFlatModeInEditor);
#else
            this.Initialize(true);
#endif
        }

        public void ToggleVR()
        {
            this.Initialize(!this.IsFlatMode);
        }

        private void Start()
        {
#if USING_UNITY_XR_MANAGEMENT
            if (this.printDebugInfo)
            {
                UnityEngine.XR.XRDevice.deviceLoaded += (device) => Logger.Log($"XRUtilManager: Device Loaded - {device}");
                UnityEngine.XR.InputDevices.deviceConnected += (device) => Logger.Log($"XRUtilManager: Device Connected - {device.name}");
                UnityEngine.XR.InputDevices.deviceConfigChanged += (device) => Logger.Log($"XRUtilManager: Device Config Changed - {device}");

                Logger.Log($"XRUtilManager: SystemInfo.deviceName - {SystemInfo.deviceName}");

                // Printing off all our loaders
                if (XRGeneralSettings.Instance)
                {
                    var loaders = XRGeneralSettings.Instance.Manager.activeLoaders;

                    if (loaders.Count == 0)
                    {
                        Logger.Log("XRUtilManager: No XR Loaders Present");
                    }

                    for (int i = 0; i < loaders.Count; i++)
                    {
                        string loaderName = loaders[i] != null ? loaders[i].name : "NULL";
                        Logger.Log($"XRUtilManager: XR Loader {i + 1} of {loaders.Count} - {loaderName}");
                    }
                }
            }
#endif
        }

        private void OnEnable()
        {
            if (this.initializeAtStartup)
            {
                this.initializeAtStartup = false;
                this.Initialize();
            }
        }

        private void OnDisable()
        {
            Shutdown();
        }

        [EditorEvents.OnExitingPlayMode]
        private static void Shutdown()
        {
#if USING_UNITY_XR_MANAGEMENT
            if (XRGeneralSettings.Instance &&
                XRGeneralSettings.Instance.Manager &&
                XRGeneralSettings.Instance.Manager.isInitializationComplete &&
                XRGeneralSettings.Instance.Manager.activeLoader != null)
            {
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (this.flatMode == null)
            {
                this.flatMode = GetDevice("8d3f2f802f9cb8847b0bd5cb924559e1");
                UnityEditor.EditorUtility.SetDirty(this);
            }

            if (this.devices == null)
            {
                this.devices = new List<XRDevice>
                {
                    GetDevice("be0804b100bc786458ff89d91df339c5"), // AR Core
                    GetDevice("119576cddaaa8c042bdf2ea4d447df86"), // AR Kit
                    GetDevice("c133f8ca1232e6e489319effd7087f2c"), // HoloLens 2
                    GetDevice("209b30a553eca2846910f75ec1e2355e"), // HoloLens
                    GetDevice("0d9f02d8a0006f2458f79b4b0a008f58"), // Magic Leap
                    GetDevice("39ec68936fcb95d4985925451e779a08"), // Oculus Quest 2
                    GetDevice("574118f13abab3b468f42929f3e27cdb"), // Oculus Quest
                    GetDevice("b73440d4434012e4fbf412a29becd934"), // Oculus Rift
                    GetDevice("c4d7f125f54986e40b1123fe5baea69e"), // Steam VR
                    GetDevice("728fd93faf076044387ba0f3017e332e"), // Oculus Link (Editor Only)
                    GetDevice("834186b0fee049747ae23938e386f6cf"), // Open XR
                };

                UnityEditor.EditorUtility.SetDirty(this);
            }

            if (this.headsetSearchOrder == null)
            {
                this.headsetSearchOrder = new List<XRLoader>
                {
                    XRLoader.Oculus,
                    XRLoader.SteamVR,
                    XRLoader.WindowsMixedReality,
                    XRLoader.OpenXR,
                };

                UnityEditor.EditorUtility.SetDirty(this);
            }

            if (this.headsetSearchPlatforms == null)
            {
                this.headsetSearchPlatforms = new List<RuntimePlatform>
                {
                    RuntimePlatform.WindowsPlayer,
                    RuntimePlatform.OSXPlayer,
                    RuntimePlatform.LinuxPlayer,
                    RuntimePlatform.WindowsEditor,
                    RuntimePlatform.OSXEditor,
                    RuntimePlatform.LinuxEditor,
                };

                UnityEditor.EditorUtility.SetDirty(this);
            }

            static XRDevice GetDevice(string guid)
            {
                var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                return UnityEditor.AssetDatabase.LoadAssetAtPath<XRDevice>(assetPath);
            }
        }

        //// private void OnGUI()
        //// {
        ////     if (GUILayout.Button("Toggle XR"))
        ////     {
        ////         this.ToggleVR();
        ////     }
        //// }

#endif

        private async void Initialize(bool forceFlatMode)
        {
#if USING_UNITY_XR_MANAGEMENT
            // Making sure we haven't already initialized a device (if one was, then deinitialize it)
            var xrSettingsManager = XRGeneralSettings.Instance.Manager;

            if (xrSettingsManager.activeLoader != null)
            {
                while (xrSettingsManager.isInitializationComplete == false)
                {
                    await Task.Yield();
                }

                xrSettingsManager.DeinitializeLoader();
            }

            // Special case for forcing Flat mode
            if (forceFlatMode)
            {
                this.SetCurrentDevice(this.flatMode);
                return;
            }

            // Doing a search through all our devices for the most applicable one
            var hmdName = await this.FindConnectedHMD();
            var platform = Application.platform;
            var deviceName = SystemInfo.deviceName;
            var xrDevice = this.flatMode;

            for (int i = 0; i < this.devices.Count; i++)
            {
                if (this.devices[i].IsApplicable(deviceName, hmdName, platform))
                {
                    xrDevice = this.devices[i];
                    break;
                }
            }

            // We've found our device, so lets initialize and set it
            if (xrDevice != this.flatMode)
            {
                if (this.printDebugInfo)
                {
                    Logger.Log($"XRUtilManager: Attempting to load xrDevice {xrDevice.name}", this);
                }

                this.StartUnityXRPlugin(xrDevice.PrimaryXRLoader, xrDevice.SecondaryXRLoader);
                this.SetCurrentDevice(xrDevice);
            }
            else
            {
                this.SetCurrentDevice(this.flatMode);
            }
#else
            await Task.Yield();
            this.SetCurrentDevice(this.flatMode);
#endif
        }

        private void SetCurrentDevice(XRDevice xrDevice)
        {
            // Enforcing that all XR Plugins are disabled when in Flat Mode
#if USING_UNITY_XR_MANAGEMENT
            var xrSettingsManager = XRGeneralSettings.Instance.Manager;
            if (xrDevice == this.flatMode && xrSettingsManager.activeLoader != null)
            {
                xrSettingsManager.DeinitializeLoader();
            }
#endif

            this.currentDevice = xrDevice;

            if (this.currentDevice != xrDevice)
            {
                this.onXRDeviceChange.SafeInvoke();
            }

            // Setting target framerate on mobile
            if (this.setTargetFramerateOnMobileAR && this.currentDevice.XRType == XRType.ARHanheld)
            {
                Application.targetFrameRate = this.mobileArTargetFramerate;
            }

            if (this.printDebugInfo)
            {
                Logger.Log($"XRUtilManager: Current Device = {this.currentDevice.name}");
            }
        }

#if USING_UNITY_XR_MANAGEMENT
        private void StartUnityXRPlugin(XRLoader primaryXRLoader, XRLoader secondaryXRLoader)
        {
            var xrSettingsManager = XRGeneralSettings.Instance.Manager;
            var activeLoaders = new List<UnityEngine.XR.Management.XRLoader>(xrSettingsManager.activeLoaders);
            bool primarySuccess = MoveLoaderToTop(activeLoaders, secondaryXRLoader);
            bool secondarySuccess = MoveLoaderToTop(activeLoaders, primaryXRLoader);

            // Making sure our loaders are present and moved to the top
            if (primarySuccess == false && secondarySuccess == false)
            {
                Logger.LogError($"XRUtilManager: Couldn't move loaders to top {primaryXRLoader} and {secondaryXRLoader}", this);
                return;
            }

            // Updating the active loaders list order to make sure our desired loaders are at the top
            bool trySetSuccess = xrSettingsManager.TrySetLoaders(activeLoaders);

            if (trySetSuccess == false)
            {
                Logger.LogError($"XRUtilManager: Unable to set our new XR Laoder order!", this);
                return;
            }

            // Starting up the XR Manager if needed
            xrSettingsManager.InitializeLoaderSync();

            var activeLoader = xrSettingsManager.activeLoader;

            if (activeLoader != null && xrSettingsManager.isInitializationComplete)
            {
                if (activeLoader.Start() == false)
                {
                    Logger.LogError($"XRUtilManager: Unable to start activeLoader {activeLoader.name}");
                }
            }
            else
            {
                Logger.LogError($"XRUtilManager: Unable to Initialize XR Loaders {primaryXRLoader} or {secondaryXRLoader}");
            }

            bool MoveLoaderToTop(List<UnityEngine.XR.Management.XRLoader> activeLoaders, XRLoader xrLoader)
            {
                if (xrLoader == XRLoader.None || xrLoader == XRLoader.Unknown)
                {
                    return false;
                }

                int loaderIndex = GetLoaderIndex(activeLoaders, xrLoader);

                if (loaderIndex != -1)
                {
                    var loader = activeLoaders[loaderIndex];
                    activeLoaders.RemoveAt(loaderIndex);
                    activeLoaders.Insert(0, loader);
                    return true;
                }

                return false;
            }

            int GetLoaderIndex(List<UnityEngine.XR.Management.XRLoader> activeLoaders, XRLoader xrLoader)
            {
                string loaderName = this.GetLoaderName(xrLoader);

                if (loaderName == null)
                {
                    Logger.LogError($"XRUtilManager: Unkonwn XRLoader Type \"{primaryXRLoader}\" encountered");
                    return -1;
                }

                // Searching for the loader and moving to the front of the list
                for (int i = 0; i < activeLoaders.Count; i++)
                {
                    var loader = activeLoaders[i];

                    if (loader.name == loaderName)
                    {
                        return activeLoaders.IndexOf(loader);
                    }
                }

                return -1;
            }
        }

        private string GetLoaderName(XRLoader loader)
        {
            switch (loader)
            {
                case XRLoader.None: return null;
                case XRLoader.Unknown: return null;
                case XRLoader.Oculus: return "Oculus Loader";
                case XRLoader.ARCore: return "AR Core Loader";
                case XRLoader.ARKit: return "AR Kit Loader";
                case XRLoader.MagicLeap: return "Magic Leap Loader";
                case XRLoader.WindowsMixedReality: return "Windows MR Loader";
                case XRLoader.SteamVR: return "Steam VR Loader";
                case XRLoader.OpenXR: return "Open XR Loader";
                case XRLoader.Playstation: return "PSVR Loader";

                default:
                {
                    Logger.LogError($"XRUtilManager: Found Unknown XRLoader {loader}");
                    return null;
                }
            }
        }

        private async Task<string> FindConnectedHMD()
        {
            // Don't search for an HMD if it's not a platform that requires a search
            if (this.headsetSearchPlatforms.Contains(Application.platform) == false)
            {
                return null;
            }

            var xrSettingsManager = XRGeneralSettings.Instance.Manager;
            var originalLoadersList = new List<UnityEngine.XR.Management.XRLoader>(xrSettingsManager.activeLoaders);
            var currentLoaderList = new List<UnityEngine.XR.Management.XRLoader>();
            string hmdDeviceName = null;

            for (int loaderIndex = 0; loaderIndex < this.headsetSearchOrder.Count; loaderIndex++)
            {
                var loaderToTest = FindLoader(originalLoadersList, this.headsetSearchOrder[loaderIndex]);

                if (loaderToTest == null)
                {
                    continue;
                }

                // Make sure one isn't already active
                if (xrSettingsManager.activeLoader != null)
                {
                    xrSettingsManager.DeinitializeLoader();
                }

                // Making a list of one to use with TrySetLoaders
                currentLoaderList.Clear();
                currentLoaderList.Add(loaderToTest);

                // Setting the new order and starting
                if (xrSettingsManager.TrySetLoaders(currentLoaderList))
                {
                    xrSettingsManager.InitializeLoaderSync();

                    if (xrSettingsManager.activeLoader != null)
                    {
                        if (xrSettingsManager.activeLoader.Start())
                        {
                            // Need to wait a few ticks so the headsets can be recognized by the XR system
                            await Task.Yield();
                            await Task.Yield();
                            await Task.Yield();

                            hmdDeviceName = FindFirstConnectedHMD();
                        }
                    }
                }

                if (hmdDeviceName != null)
                {
                    break;
                }
            }

            // Restoring the original loaders list
            xrSettingsManager.TrySetLoaders(originalLoadersList);

            return hmdDeviceName;

            UnityEngine.XR.Management.XRLoader FindLoader(List<UnityEngine.XR.Management.XRLoader> loaders, XRLoader loader)
            {
                string loaderName = this.GetLoaderName(loader);

                for (int i = 0; i < loaders.Count; i++)
                {
                    if (loaders[i].name == loaderName)
                    {
                        return loaders[i];
                    }
                }

                return null;
            }

            string FindFirstConnectedHMD()
            {
                var hmdDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head);

                if (hmdDevice.isValid)
                {
                    bool presenceSupported = hmdDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.userPresence, out bool userPresent);

                    if (this.printDebugInfo)
                    {
                        Logger.Log($"XRUtilManager: Head Device = {hmdDevice.name}, Presence Supported = {presenceSupported}, User Present = {userPresent}");
                    }

                    if (presenceSupported)
                    {
                        return hmdDevice.name;
                    }
                }

                return null;
            }
        }

#endif
    }
}
