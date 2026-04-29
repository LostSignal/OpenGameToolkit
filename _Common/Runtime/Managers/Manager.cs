//-----------------------------------------------------------------------
// <copyright file="Manager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public abstract class Manager : GameBehavior
    {
        private bool isInitialized;

        private event System.Action onInitialized = delegate {};

        public event System.Action OnInitialize
        {
            add
            {
                onInitialized += value;

                if (isInitialized)
                {
                    value?.Invoke();
                }
            }

            remove
            {
                onInitialized -= value;
            }
        }

        public bool IsInitialized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => isInitialized;
        }

        public async Task Initialize(Bootloader bootloader)
        {
            await InitializeManager(bootloader);

            isInitialized = true;
            onInitialized?.Invoke();
        }

        public async Task WaitForInitialization()
        {
            while (this.isInitialized == false)
            {
                await Task.Yield();
            }
        }

        public virtual void ResetToDefaults()
        {
        }

        public virtual void OnManagerDestroyed()
        {
        }

        protected abstract Task InitializeManager(Bootloader bootloader);

#if UNITY_6000_0_OR_NEWER
        private void OnDestroy()
        {
            this.OnManagerDestroyed();
        }

        private void Reset()
        {
            this.ResetToDefaults();
        }

#endif
    }
}
