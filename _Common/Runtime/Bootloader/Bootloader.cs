//-----------------------------------------------------------------------
// <copyright file="Bootloader.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using UnityEngine;

    public enum AppOrientation
    {
        Landscape,
        Portrait,
        Both,
    }

    public class Bootloader : GameBehavior
    {
        private static readonly OGTLogger Logger = OGTLogger.Bootloader;

        public delegate void OnBootedDelegate(Bootloader bootloader);
        private OnBootedDelegate onBooted;

        [Header("App Settings")]
        [SerializeField] private AppOrientation supportedOrientation;

        private List<Manager> managers;

        public AppOrientation SupportedOrientation => this.supportedOrientation;

        public bool IsBooted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        public event OnBootedDelegate OnBooted
        {
            add
            {
                if (this.IsBooted)
                {
                    value?.Invoke(this);
                }
                else
                {
                    this.onBooted += value;
                }
            }

            remove => this.onBooted -= value;
        }

        public async void Boot()
        {
            // Registering all providers
            foreach (var provider in this.GetChildrenOfType<IProviderInitializer>())
            {
                provider.Register();
            }

            // Initializing all Managers
            this.managers = new List<Manager>(this.GetChildrenOfType<Manager>());

            var initializingTasks = new List<Task>(this.managers.Count);

            foreach (var manager in this.managers)
            {
                initializingTasks.Add(manager.Initialize(this));
            }

            await Task.WhenAll(initializingTasks);

            this.IsBooted = true;
            this.onBooted?.Invoke(this);
        }

        public T FindManager<T>() where T : class
        {
            foreach (var manager in this.managers)
            {
                if (manager is T instance)
                {
                    return instance;
                }
            }

            Logger.LogError($"Unable to find Manager {typeof(T).Name}!");
            return null;
        }
    }
}
