//-----------------------------------------------------------------------
// <copyright file="OrientationManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Threading.Tasks;
    using UnityEngine;

    public class OrientationManager : Manager, IUpdate
    {
        public enum Orientation
        {
            Unspecified,
            Portrait,
            Landscape,
        }

        public System.Action<Orientation> OrientationChanged;

        public Orientation CurrentOrientation => this.currentOrientation;

        private Orientation currentOrientation;

        public void OnUpdate(float deltaTime)
        {
            var newOrientation = GetCurrentOrientation();

            if (this.currentOrientation != newOrientation)
            {
                this.currentOrientation = newOrientation;
                this.OrientationChanged?.Invoke(newOrientation);
            }
        }

        public static Orientation GetCurrentOrientation()
        {
            Vector2 size = GetCurrentScreenSize();
            return size.x >= size.y ? Orientation.Landscape : Orientation.Portrait;
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }

        public static Vector2 GetCurrentScreenSize()
        {
#if UNITY_EDITOR
            // In the Editor, use the Game View size instead of Screen size
            return UnityEditor.Handles.GetMainGameViewSize();
#else
            return new Vector2(Screen.width, Screen.height);
#endif
        }
    }
}
