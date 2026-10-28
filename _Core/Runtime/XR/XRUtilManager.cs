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
    using UnityEngine.XR;

#if USING_UNITY_XR_MANAGEMENT
    using UnityEngine.XR.Management;
#endif

    public class XRUtilManager : GameBehavior
    {
        private static readonly OGTLogger Logger = OGTLogger.XR;

        [SerializeField] private XRDevice flatMode;
        [SerializeField] private List<XRDevice> devices;

        [Header("HMD Search")]
        [SerializeField] private List<XRLoader> headsetSearchOrder;
        [SerializeField] private List<RuntimePlatform> headsetSearchPlatforms;

        [Header("Mobile AR")]
        [SerializeField] private bool setTargetFramerateOnMobileAR = true;
        [SerializeField] private int mobileArTargetFramerate = 30;

        private bool hasPrintedDebugOutput;
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
                    GetDevice("696688d106551344cbf55ef6d7639b50"), // Oculus Quest 3S
                    GetDevice("4d1e96dd709821f4eb1f1d8e6eed1e1b"), // Oculus Quest 3
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
#endif

        private async void Initialize(bool forceFlatMode)
        {
#if !USING_UNITY_XR_MANAGEMENT
            await Task.Yield();
            this.SetCurrentDevice(this.flatMode);
#else
            PrintDebugOutput();

            // Making sure we haven't already initialized a device (if one was, then deinitialize it)
            var xrSettingsManager = XRGeneralSettings.Instance.Manager;

            // If we're forcing flat mode, make sure to wait for active loader to finish before deinitializing it
            if (forceFlatMode)
            {
                if (xrSettingsManager.activeLoader != null)
                {
                    while (xrSettingsManager.isInitializationComplete == false)
                    {
                        await Task.Yield();
                    }

                    xrSettingsManager.DeinitializeLoader();
                }

                this.SetCurrentDevice(this.flatMode);
                return;
            }

            // We should be initialized so finding the connected device
            var activeHmdName = await this.WaitForConnectedHMD();
            var activeDevice = activeHmdName != null ? this.FindApplicableDevice(activeHmdName, xrSettingsManager.activeLoaders) : null;

            if (activeDevice != null)
            {
                this.SetCurrentDevice(activeDevice);
            }

            PrintDebugOutput();

            /*
            // Doing a search through all our devices for the most applicable one
            var hmdName = await this.FindConnectedHMD();
            var xrDevice = this.FindApplicableDevice(hmdName, xrSettingsManager.activeLoaders) ?? this.flatMode;

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
            */

            void PrintDebugOutput()
            {
                if (this.hasPrintedDebugOutput)
                {
                    return;
                }

                this.hasPrintedDebugOutput = true;

                InputDevices.deviceConnected += (device) => Logger.LogFormat(this, "XRUtilManager: Device Connected - {0}", device.name);
                InputDevices.deviceDisconnected += (device) => Logger.LogFormat(this, "XRUtilManager: Device Disconnected - {0}", device.name);
                InputDevices.deviceConfigChanged += (device) => Logger.LogFormat(this, "XRUtilManager: Device Config Changed - {0}", device);

                Logger.LogFormat(this, "XRUtilManager: SystemInfo.deviceName - {0}", SystemInfo.deviceName);

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
                        Logger.LogFormat(this, "XRUtilManager: XR Loader {0} of {1} - {2}", i + 1, loaders.Count, loaderName);
                    }
                }

                // Printing off all our devices
                var devices = new List<InputDevice>();

                InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, devices);

                foreach (var device in devices)
                {
                    Logger.Log($"XRUtilManager: Name: {device.name}");
                    Logger.Log($"XRUtilManager: Manufacturer: {device.manufacturer}");
                    Logger.Log($"XRUtilManager: Serial Number: {device.serialNumber}");
                    Logger.Log($"XRUtilManager: Valid: {device.isValid}");
                    Logger.Log($"XRUtilManager: Characteristics: {device.characteristics}");
                }
            }
#endif
        }

        private void SetCurrentDevice(XRDevice xrDevice)
        {
            bool deviceChanged = this.currentDevice != xrDevice;
            this.currentDevice = xrDevice;

            if (deviceChanged)
            {
                this.onXRDeviceChange.SafeInvoke();
            }

            // Setting target framerate on mobile
            if (this.setTargetFramerateOnMobileAR && this.currentDevice.XRType == XRType.ARHanheld)
            {
                Application.targetFrameRate = this.mobileArTargetFramerate;
            }

            Logger.LogFormat(this, "XRUtilManager: Current Device = {0}", currentDevice.name);
        }

#if USING_UNITY_XR_MANAGEMENT

        private async Task<string> WaitForConnectedHMD()
        {
            // The head device can take a few frames to show up after the XR session starts
            for (int frame = 0; frame < 120; frame++)
            {
                var hmdName = this.FindFirstConnectedHMD();

                if (hmdName != null)
                {
                    return hmdName;
                }

                await Task.Yield();
            }

            return null;
        }

        private string FindFirstConnectedHMD()
        {
            var hmdDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);

            if (hmdDevice.isValid == false)
            {
                return null;
            }

            bool presenceSupported = hmdDevice.TryGetFeatureValue(CommonUsages.userPresence, out bool userPresent);

            Logger.LogFormat(this, "XRUtilManager: Head Device = {0}, Presence Supported = {1}, User Present = {2}", hmdDevice.name, presenceSupported, userPresent);

            // NOTE: Not requiring presence support, OpenXR runtimes (Meta Quest / Link) don't always report it
            return hmdDevice.name;
        }

        private XRDevice FindApplicableDevice(string hmdName, IReadOnlyList<UnityEngine.XR.Management.XRLoader> loaders)
        {
            var platform = Application.platform;
            var deviceName = SystemInfo.deviceName;

            Logger.LogFormat((string)"Searching for applicable device: deviceName = {0}, hmdName = {1}, platform = {2}", (object)deviceName, (object)hmdName, (object)platform);

            for (int i = 0; i < this.devices.Count; i++)
            {
                Logger.LogFormat("Testing Device {0}", this.devices[i].name);
                var device = this.devices[i];

                // Skipping devices whose XR Loaders aren't configured for this build (keeps AR Core from matching a Quest)
                if (device == null || (this.HasLoader(loaders, device.PrimaryXRLoader) == false && this.HasLoader(loaders, device.SecondaryXRLoader) == false))
                {
                    Logger.LogFormat("Skipping Device {0} because its XR Loaders aren't configured for this build", this.devices[i].name);
                    continue;
                }

                if (device.IsApplicable(deviceName, hmdName, platform))
                {
                    Logger.LogFormat("Found applicable Device {0}", this.devices[i].name);
                    return device;
                }
                else
                {
                    Logger.LogFormat("Skipping Device {0} because it is not applicable", this.devices[i].name);
                }
            }

            Logger.Log("No applicable device found");
            return null;
        }

        private bool HasLoader(IReadOnlyList<UnityEngine.XR.Management.XRLoader> loaders, XRLoader xrLoader)
        {
            for (int i = 0; i < loaders.Count; i++)
            {
                if (this.IsLoader(loaders[i], xrLoader))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsLoader(UnityEngine.XR.Management.XRLoader loader, XRLoader xrLoader)
        {
            if (loader == null || xrLoader == XRLoader.None || xrLoader == XRLoader.Unknown)
            {
                return false;
            }

            // Matching on the loader's asset name (legacy display names) or its type (e.g. "OpenXRLoader")
            if (loader.name == GetLoaderName(xrLoader))
            {
                return true;
            }

            string typeName = GetLoaderTypeName(xrLoader);

            for (var type = loader.GetType(); typeName != null && type != null; type = type.BaseType)
            {
                if (type.Name == typeName)
                {
                    return true;
                }
            }

            return false;

            static string GetLoaderTypeName(XRLoader xrLoader) => xrLoader switch
            {
                XRLoader.Oculus => "OculusLoader",
                XRLoader.ARCore => "ARCoreLoader",
                XRLoader.ARKit => "ARKitLoader",
                XRLoader.MagicLeap => "MagicLeapLoader",
                XRLoader.WindowsMixedReality => "WindowsMRLoader",
                XRLoader.SteamVR => "OpenVRLoader",
                XRLoader.OpenXR => "OpenXRLoader",
                _ => null,
            };

            string GetLoaderName(XRLoader loader)
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
        }

        /*

        //// NOTE [bgish]: Old code that was good lat switch loaders around before initialization.  Now that OpenXR is
        ////               the standard, I think this code is obsolete, but keeping it around incase a need arises.

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
                if (this.GetLoaderName(xrLoader) == null)
                {
                    Logger.LogError($"XRUtilManager: Unkonwn XRLoader Type \"{primaryXRLoader}\" encountered");
                    return -1;
                }

                // Searching for the loader and moving to the front of the list
                for (int i = 0; i < activeLoaders.Count; i++)
                {
                    if (this.IsLoader(activeLoaders[i], xrLoader))
                    {
                        return i;
                    }
                }

                return -1;
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

                    while (xrSettingsManager.isInitializationComplete == false)
                    {
                        await Task.Yield();
                    }

                    if (xrSettingsManager.activeLoader != null)
                    {
                        //if (xrSettingsManager.activeLoader.Start())
                        {
                            // Need to wait a few ticks so the headsets can be recognized by the XR system
                            //await Task.Yield();
                            //await Task.Yield();
                            //await Task.Yield();

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
                for (int i = 0; i < loaders.Count; i++)
                {
                    if (this.IsLoader(loaders[i], loader))
                    {
                        return loaders[i];
                    }
                }

                return null;
            }
        }
        */

#endif
    }
}
