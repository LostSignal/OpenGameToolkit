namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public class ScreenManager : Manager
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private AppOrientation supportedOrientation;

        [Header("Limiting Resolution")]
        [SerializeField] private bool limitMobileScreenSize;
        [SerializeField] private int maxScreenSize = 1920;

        [Header("Editor Game View Resolutions")]
        [SerializeField]
        private bool shouldAddEditorResolutions;

        [SerializeField]
        private List<Resolution> editorPortraitResolutions = new()
        {
            new Resolution(1080, 1920, "Generic Phone"),
        };

        [SerializeField]
        private List<Resolution> editorLandscapeResolutions = new()
        {
            new Resolution(1920, 1080, "1080p"),
            new Resolution(3840, 2160, "4k"),
            new Resolution(1280, 800, "Steam Deck"),
            new Resolution(1280, 720, "Switch"),
        };

        protected override Task InitializeManager(Bootloader bootloader)
        {
            this.supportedOrientation = bootloader.SupportedOrientation;

            this.SetupGameView();
            this.LimitScreenSize();
            this.ForceCorrectOrientation();

            return Task.CompletedTask;
        }

        private void SetupGameView()
        {
#if UNITY_EDITOR
            if (this.shouldAddEditorResolutions == false)
            {
                return;
            }

            // NOTE [bgish]: You must do this before possibly adding new resolutions
            var currentResolution = GameViewUtils.GetResolution();

            // Adding Portrait Resolutions to the GameView
            if ((this.supportedOrientation == AppOrientation.Portrait || this.supportedOrientation == AppOrientation.Both) && this.editorPortraitResolutions.IsNullOrEmpty() == false)
            {
                foreach (var resolution in this.editorPortraitResolutions)
                {
                    GameViewUtils.AddResolution(resolution.Width, resolution.Height, resolution.Name);
                }
            }

            // Adding Landscape Resolutions to the GameView
            if ((this.supportedOrientation == AppOrientation.Landscape || this.supportedOrientation == AppOrientation.Both) && this.editorLandscapeResolutions.IsNullOrEmpty() == false)
            {
                foreach (var resolution in this.editorLandscapeResolutions)
                {
                    GameViewUtils.AddResolution(resolution.Width, resolution.Height, resolution.Name);
                }
            }

            // Special Case to switch the game view to portrait mode if it's currently landscape
            if (this.supportedOrientation == AppOrientation.Portrait && this.editorPortraitResolutions.IsNullOrEmpty() == false && currentResolution.x > currentResolution.y)
            {
                GameViewUtils.SetResolutionByName(this.editorPortraitResolutions[0].Name);
            }

            // Special Case to switch the game view to landscape mode if it's currently portrait
            if (this.supportedOrientation == AppOrientation.Landscape && this.editorLandscapeResolutions.IsNullOrEmpty() == false && currentResolution.x < currentResolution.y)
            {
                GameViewUtils.SetResolutionByName(this.editorLandscapeResolutions[0].Name);
            }
#endif
        }

        private void LimitScreenSize()
        {
            if (Application.isEditor || this.limitMobileScreenSize == false)
            {
                return;
            }

            // TODO [bgish]: These should be questions for the Platform class, or platform Manager
            bool isMobilePlatform = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
            bool isMobileVrDevice = isMobilePlatform && SystemInfo.deviceName.ToLower().Contains("quest");

            if (isMobilePlatform && isMobileVrDevice == false)
            {
                bool isLandscape = Screen.width > Screen.height;

                if (isLandscape && Screen.width > maxScreenSize)
                {
                    float aspectRatio = Screen.height / (float)Screen.width;
                    int newHeight = (int)(maxScreenSize * aspectRatio);
                    int newWidth = maxScreenSize;

                    Logger.LogFormat("Resizing Screen From {0}x{1} To {2}x{3}", Screen.width, Screen.height, newWidth, newHeight);
                    Screen.SetResolution(newWidth, newHeight, true);
                }
                else if (isLandscape == false && Screen.height > maxScreenSize)
                {
                    float aspectRatio = Screen.width / (float)Screen.height;
                    int newHeight = maxScreenSize;
                    int newWidth = (int)(maxScreenSize * aspectRatio);

                    Logger.LogFormat("Resizing Screen From {0}x{1} To {2}x{3}", Screen.width, Screen.height, newWidth, newHeight);
                    Screen.SetResolution(newWidth, newHeight, true);
                }
            }
        }

        private void ForceCorrectOrientation()
        {
            if (Application.isEditor)
            {
                return;
            }

            if (this.supportedOrientation == AppOrientation.Portrait)
            {
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = true;

                if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
                {
                    Screen.orientation = ScreenOrientation.Portrait;
                }
            }
            else if (this.supportedOrientation == AppOrientation.Landscape)
            {
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.autorotateToPortrait = false;
                Screen.autorotateToPortraitUpsideDown = false;

                if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
                {
                    Screen.orientation = ScreenOrientation.LandscapeLeft;
                }
            }
        }

        [Serializable]
        private class Resolution
        {
            public string Name;
            public int Width;
            public int Height;

            public Resolution(int x, int y, string name)
            {
                this.Width = x;
                this.Height = y;
                this.Name = name;
            }
        }
    }
}
